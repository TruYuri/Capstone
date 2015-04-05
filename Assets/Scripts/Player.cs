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
    protected Squad _commandShip;
    protected Dictionary<string, Ship> _shipDefinitions;
    protected Dictionary<string, Research> _shipResearchMap;
    protected List<Squad> _squads;
    protected int _numResearchStations;
    protected ResearchTree _militaryTree;
    protected ResearchTree _scienceTree;

    public Team Team { get { return _team; } }
    public List<Squad> Squads { get { return _squads; } }
    public bool TurnEnded { get { return _turnEnded; } }

    protected Squad _playerSquad;
    protected Squad _enemySquad;
    protected float _winChance;

    public virtual void Init(Team team)
    {
        _team = team;
        _squads = new List<Squad>();
        _shipDefinitions = GameManager.Instance.GenerateShipDefs();
        _militaryTree = GameManager.Instance.GenerateMilitaryTree(_shipDefinitions);
        _scienceTree = GameManager.Instance.GenerateScienceTree(_shipDefinitions);
    }

	void Start () 
    {
	}
	
	public virtual void Control(GameObject gameObject)
    {
        if (gameObject.GetComponent<Squad>() == null)
            return;
        _controlledSquad = gameObject.GetComponent<Squad>();
        _controlledTile = gameObject.GetComponent<Tile>();
    }

	// Update is called once per frame
	void Update () 
    {
        if (GameManager.Instance.Paused || _turnEnded)
            return;

        EndTurn();
	}

    public bool UpgradeResearch(string type, string research, string property)
    {
        if(type == MILITARY)
            return _militaryTree.GetResearch(research).UpgradeResearch(property, _numResearchStations);
        else if(type == SCIENCE)
            return _scienceTree.GetResearch(research).UpgradeResearch(property, _numResearchStations);
        return false;
    }

    public void CreateBattleEvent(Squad squad1, Squad squad2)
    {
        GameManager.Instance.AddEvent(new BattleEvent(squad1, squad1));
    }

    public void CreateBuildEvent(string shipName)
    {
        GameManager.Instance.AddEvent(new BuildEvent(1, _controlledTile, _shipDefinitions[shipName].Copy()));
        EndTurn();
    }

    public void CreateDeployEvent(int shipIndex)
    {
        // calculate turns - this is gonna suck
        GameManager.Instance.AddEvent(new DeployEvent(1, _controlledSquad.Ships[shipIndex] as Structure, _controlledSquad, _controlledSquad.Tile));
        EndTurn();
    }

    public void CreateUndeployEvent(bool destroy)
    {
        GameManager.Instance.AddEvent(new UndeployEvent(1, _controlledTile, destroy));
        EndTurn();
    }

    public void EndTurn()
    {
        _turnEnded = true;
    }

    // DON'T CALL THIS FROM HERE - for GameManager!
    public virtual void TurnEnd()
    {
        _militaryTree.Advance();
        _scienceTree.Advance();
        _turnEnded = false;
    }

    public virtual float PrepareBattleConditions(Squad squad1, Squad squad2)
    {
        _playerSquad = squad2;
        _enemySquad = squad1;

        if (squad1.Team == _team)
        {
            _playerSquad = squad1;
            _enemySquad = squad2;
        }

        var pt = _playerSquad.GetComponent<Tile>();
        var et = _enemySquad.GetComponent<Tile>();

        float WC = 0f;
        if (pt == null && et == null)
            WC = _playerSquad.GenerateWinChance(_enemySquad);
        else if (pt != null && et == null)
            WC = 1.0f - _enemySquad.GenerateWinChance(pt);
        else if (pt == null && pt != null)
            WC = _playerSquad.GenerateWinChance(et);

        return Mathf.Clamp01(WC);
    }

    public virtual Squad Battle(float playerChance, Squad player, Squad enemy)
    {
        var pt = player.GetComponent<Tile>();
        var et = enemy.GetComponent<Tile>();

        Squad win = null;
        Team winner;
        if (pt != null) // player tile vs. enemy squad
            winner = enemy.Combat(pt, 1.0f - playerChance);
        else if(et != null) // player squad vs. enemy tile
            winner = player.Combat(et, playerChance);
        else // squad vs. squad
            winner = player.Combat(enemy, playerChance);

        // do nothing / undeploy as necessary
        if (winner == _team && et != null)
        {
            et.Relinquish();
            et.Undeploy(true);
            et.Claim(_team);
            win = player;
        }
        else if (winner == enemy.Team && pt != null)
        {
            pt.Relinquish();
            pt.Undeploy(true);
            pt.Claim(enemy.Team);
            win = enemy;
        }
        else
            win = winner == player.Team ? player : enemy;

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

        if(_controlledSquad == null || _controlledSquad.gameObject == null || (_controlledSquad.Ships.Count == 0 && _controlledTile != null))
        {
            var colliders = squad.Colliders;
            if(colliders.Count > 0)
                Control(colliders[0].gameObject);
            else
                Control(_squads[GameManager.Generator.Next(0, _squads.Count)].gameObject);
        }
    }

    public void DeleteSquad(Squad squad)
    {
        _squads.Remove(squad);

        if(squad != null)
            GameObject.DestroyImmediate(squad.gameObject);
    }

    public Squad CreateNewSquad(Squad fromSquad)
    {
        var val = GameManager.Generator.Next(2);

        float dist;
        var tile = fromSquad.GetComponent<Tile>();
        if (tile != null)
            dist = tile.Radius / 2.0f;
        else
            dist = fromSquad.GetComponent<SphereCollider>().radius / 2.0f;
        var offset = val == 0 ? new Vector3(dist, 0, 0) : new Vector3(0, 0, dist);
        var squad = CreateNewSquad(fromSquad.transform.position + offset);
        fromSquad.Colliders.Add(squad);
        squad.Colliders.Add(fromSquad);
        return squad;
    }

    public Squad CreateNewSquad(Vector3 position)
    {
        var squadobj = Resources.Load<GameObject>(SQUAD_PREFAB);
        var squad = Instantiate(squadobj, position, Quaternion.identity) as GameObject;
        var component = squad.GetComponent<Squad>();
        component.Team = _team;
        _squads.Add(component);
        component.Init();
        return component;
    }
}
