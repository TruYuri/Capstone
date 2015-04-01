using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class Player : MonoBehaviour
{
    private const string MILITARY = "Military";
    private const string SCIENCE = "Scientific";
    protected const string SQUAD_PREFAB = "Squad";

    protected bool _turnEnded;
    protected Team _team;
    protected Squad _controlledSquad;
    protected Tile _controlledTile;
    protected CommandShip _commandShip;
    protected Dictionary<string, Ship> _shipDefinitions;
    protected Dictionary<string, Research> _shipResearchMap;
    protected List<Squad> _squads;
    protected int _numResearchStations;
    protected ResearchTree _militaryTree;
    protected ResearchTree _scienceTree;

    public Team Team { get { return _team; } }
    public List<Squad> Squads { get { return _squads; } }
    public bool TurnEnded { get { return _turnEnded; } }

	void Start () 
    {
        _shipDefinitions = GameManager.Instance.GenerateShipDefs();
        _militaryTree = GameManager.Instance.GenerateMilitaryTree(_shipDefinitions);
        _scienceTree = GameManager.Instance.GenerateScienceTree(_shipDefinitions);
	}
	
	public virtual void Control(GameObject gameObject)
    {
        _controlledSquad = gameObject.GetComponent<Squad>();
        _controlledTile = gameObject.GetComponent<Tile>();
    }

	// Update is called once per frame
	void Update () 
    {
        if (GameManager.Instance.Paused)
            return;
	}

    public bool UpgradeResearch(string type, string research, string property)
    {
        if(type == MILITARY)
            return _militaryTree.GetResearch(research).UpgradeResearch(property, _numResearchStations);
        else if(type == SCIENCE)
            return _scienceTree.GetResearch(research).UpgradeResearch(property, _numResearchStations);
        return false;
    }

    public void CreateDeployEvent(int shipIndex)
    {
        // calculate turns - this is gonna suck
        GameManager.Instance.AddEvent(new DeployEvent(_controlledSquad.Ships[shipIndex] as Structure, _controlledSquad, _controlledSquad.ColliderTile, 1));
        EndTurn();
    }

    public void CreateUndeployEvent()
    {
        GameManager.Instance.AddEvent(new UndeployEvent(_controlledTile, 1));
        EndTurn();
    }

    public void EndTurn()
    {
        _turnEnded = true;
    }

    // DON'T CALL THIS FROM HERE - for GameManager!
    public void TurnEnd()
    {
        _militaryTree.Advance();
        _scienceTree.Advance();
        _turnEnded = false;
    }

    public virtual Squad Battle(Squad squad1, Squad squad2)
    {
        Squad player = squad2;
        Squad enemy = squad1;

        if(squad1.Team == _team)
        {
            player = squad1;
            enemy = squad2;
        }

        Control(player.gameObject);

        var pt = player.GetComponent<Tile>();
        var et = enemy.GetComponent<Tile>();

        Squad win = null;
        Team winner;
        if (pt != null) // player tile vs. enemy squad
            winner = enemy.Combat(pt);
        else if(et != null) // player squad vs. enemy tile
            winner = player.Combat(et);
        else // squad vs. squad
            winner = player.Combat(enemy);

        // do nothing / undeploy as necessary
        if (winner == _team && et != null)
        {
            et.Undeploy(true);
            et.Claim(_team);
            win = player;
        }
        else if (winner == enemy.Team && pt != null)
        {
            pt.Undeploy(true);
            pt.Claim(enemy.Team);
            win = enemy;
        }

        GameManager.Instance.Players[player.Team].CleanSquad(player);
        GameManager.Instance.Players[enemy.Team].CleanSquad(enemy);
        return win;
    }

    public virtual void CleanSquad(Squad squad)
    {
        foreach(var sq in squad.Colliders)
            if(sq != null && sq.gameObject != null)
                Squad.CleanSquadsFromList(this, sq.Colliders);
        Squad.CleanSquadsFromList(this, squad.Colliders);

        if(_controlledSquad == null || _controlledSquad.gameObject == null)
        {
            // var n = 
        }
    }

    public Squad CreateNewSquad()
    {
        var squadobj = Resources.Load<GameObject>(SQUAD_PREFAB);
        var val = GameManager.Generator.Next(2);
        var dist = _controlledSquad.GetComponent<SphereCollider>().radius / 2.0f;
        var offset = val == 0 ? new Vector3(dist, 0, 0) : new Vector3(0, 0, dist);
        var squad = Instantiate(squadobj, _controlledSquad.transform.position + offset, Quaternion.identity) as GameObject;
        var component = squad.GetComponent<Squad>();
        component.Team = _team;
        _squads.Add(component);
        _controlledSquad.Colliders.Add(component);
        return component;
    }
}
