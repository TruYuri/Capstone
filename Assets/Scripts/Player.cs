using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class Player : MonoBehaviour
{
    private const string MILITARY = "Military";
    private const string SCIENCE = "Scientific";
    protected const string SQUAD_PREFAB = "Squad";

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

    public virtual void Deploy(int shipIndex)
    {
        Control(_controlledSquad.Deploy(shipIndex).gameObject);
        CleanSquad(_controlledSquad);
    }

    public virtual void Undeploy()
    {
        _controlledTile.Undeploy(false);
    }

    public void EndTurn()
    {
        _militaryTree.Advance();
        _scienceTree.Advance();
    }

    public virtual Team Battle()
    {
        BattleEvent gameEvent = GameManager.Instance.CurrentEvent() as BattleEvent;
        Squad player = gameEvent.Squad2;
        Squad enemy = gameEvent.Squad1;

        if(gameEvent.Squad1.Team == _team)
        {
            player = gameEvent.Squad1;
            enemy = gameEvent.Squad2;
        }

        Control(player.gameObject);

        Team winner;
        if (gameEvent.Type == GameEventType.SquadBattle)
            winner = _controlledSquad.Combat(enemy);
        else
        {
            var playerTile = player.GetComponent<Tile>();
            var enemyTile = enemy.GetComponent<Tile>();

            if(playerTile == null)
                winner = _controlledSquad.Combat(enemyTile);
            else
                winner = enemy.Combat(playerTile);

            // do nothing / undeploy as necessary
            if (winner == _team && playerTile == null)
            {
                enemyTile.Undeploy(true);
                enemyTile.Claim(_team);
            }
            else
            {
                playerTile.Undeploy(true);
                playerTile.Claim(enemy.Team);
            }
        }

        gameEvent.Progress();

        return winner;
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
