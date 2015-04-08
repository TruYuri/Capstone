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
    protected Ship _commandShip;
    protected Squad _commandShipSquad;
    protected Dictionary<string, Ship> _shipDefinitions;
    protected Dictionary<string, Research> _shipResearchMap;
    protected List<Squad> _squads;
    protected List<Tile> _tiles;
    protected int _numResearchStations;
    protected ResearchTree _militaryTree;
    protected ResearchTree _scienceTree;
    protected bool _controlledIsWithinRange;
    private int _relayDistance;

    public Team Team { get { return _team; } }
    public List<Tile> Tiles { get { return _tiles; } }
    public List<Squad> Squads { get { return _squads; } }
    public bool TurnEnded { get { return _turnEnded; } }
    public bool ControlledIsWithinRange { get { return _controlledIsWithinRange; } }

    protected Squad _playerSquad;
    protected Squad _enemySquad;
    protected float _winChance;
    protected BattleType _currentBattleType;

    public virtual void Init(Team team)
    {
        _team = team;
        _squads = new List<Squad>();
        _tiles = new List<Tile>();
        _shipDefinitions = GameManager.Instance.GenerateShipDefs();
        _militaryTree = GameManager.Instance.GenerateMilitaryTree(_shipDefinitions);
        _scienceTree = GameManager.Instance.GenerateScienceTree(_shipDefinitions);

        if(_team != global::Team.Indigenous)
            CreateNewCommandShip();
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

        if (_commandShipSquad == null || _commandShipSquad.Sector == null)
            return;
        // check range here
        var path = MapManager.Instance.AStarSearch(_commandShipSquad.Sector, _controlledSquad.Sector, 5, _team, "Relay");
        _controlledIsWithinRange = path != null;
        _relayDistance = path != null ? (path.Count - 1) / 5 : 0;
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

    public void CreateBattleEvent(Squad squad1, Tile tile)
    {
        GameManager.Instance.AddEvent(new BattleEvent(squad1, tile));
    }

    public void CreateBattleEvent(Squad squad1, Squad squad2)
    {
        GameManager.Instance.AddEvent(new BattleEvent(squad1, squad2));
    }

    public void CreateBuildEvent(string shipName)
    {
        if (!_controlledIsWithinRange)
            return;
        GameManager.Instance.AddEvent(new BuildEvent(_relayDistance + 1, _team, _controlledTile, _shipDefinitions[shipName].Copy()));
        EndTurn();
    }

    public void CreateDeployEvent(int shipIndex)
    {
        if (!_controlledIsWithinRange)
            return;
        GameManager.Instance.AddEvent(new DeployEvent(_relayDistance, this, _controlledSquad.Ships[shipIndex] as Structure, _controlledSquad, _controlledSquad.Tile));
        EndTurn();
    }

    public void CreateUndeployEvent(bool destroy)
    {
        CreateUndeployEvent(_controlledTile, destroy);
    }

    public void CreateUndeployEvent(Tile tile, bool destroy)
    {
        if (!_controlledIsWithinRange)
            return;
        GameManager.Instance.AddEvent(new UndeployEvent(_relayDistance, _team, tile, destroy));
        EndTurn();
    }

    public void CreateDiplomacyEvent()
    {
        if (!_controlledIsWithinRange)
            return;
        GameManager.Instance.AddEvent(new DiplomacyEvent(_relayDistance, this, _controlledTile));
        EndTurn();
    }

    public void CreateTravelEvent(Squad squad, Sector toSector, Vector3 dest, float speed)
    {
        if (!_controlledIsWithinRange)
            return;
        GameManager.Instance.AddEvent(new TravelEvent(1, squad, toSector, dest, speed));
    }

    public void CreateCommandShipLostEvent(Squad squad)
    {
        GameManager.Instance.AddEvent(new CommandShipLostEvent(squad, this));
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

        foreach (var tile in _tiles)
            tile.Gather();

        _turnEnded = false;
    }

    public virtual float PrepareBattleConditions(Squad squad1, Squad squad2, BattleType battleType)
    {
        _playerSquad = squad2;
        _enemySquad = squad1;
        _currentBattleType = battleType;

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
        else if (pt == null && et != null)
            WC = _playerSquad.GenerateWinChance(et);

        return Mathf.Clamp01(WC);
    }

    public bool DiplomaticEffort(Tile tile)
    {
        var IP = tile.Population * 2;

        int prim = 0, ind = 0, space = 0;
        for(int i = 0; i < _squads.Count; i++)
        {
            for(int j = 0; j < _squads[i].Ships.Count; j++)
            {
                prim += _squads[i].Ships[j].PrimitivePopulation;
                ind += _squads[i].Ships[j].IndustrialPopulation;
                space += _squads[i].Ships[j].SpaceAgePopulation;
            }
        }

        for(int i = 0; i < _tiles.Count; i++)
        {
            switch(_tiles[i].PopulationType)
            {
                case Inhabitance.Primitive:
                    prim += _tiles[i].Population;
                    break;
                case Inhabitance.Industrial:
                    ind += _tiles[i].Population;
                    break;
                case Inhabitance.SpaceAge:
                    space += _tiles[i].Population;
                    break;
            }

            if(tile.Structure != null)
            {
                prim += _tiles[i].Structure.PrimitivePopulation;
                ind += _tiles[i].Structure.IndustrialPopulation;
                space += _tiles[i].Structure.SpaceAgePopulation;
            }
        }

        int PP = 0;
        switch(tile.PopulationType)
        {
            case Inhabitance.Primitive:
                PP = prim * 2 + ind + space;
                break;
            case Inhabitance.Industrial:
                PP = prim + ind * 2 + space;
                break;
            case Inhabitance.SpaceAge:
                PP = prim + ind + space * 2;
                break;
        }

        var DC = (PP - IP) / 100.0f * 0.5f + 0.5f;
        var DP = GameManager.Generator.NextDouble();

        if (DP < DC) // we won!
        {
            tile.Claim(_team);
            tile.Population += Mathf.FloorToInt(IP * DC * ((float)GameManager.Generator.NextDouble() * (0.75f - 0.25f) + 0.25f));
            return true;
        }
        return false;
    }

    public virtual KeyValuePair<KeyValuePair<Team, BattleType>, Dictionary<string, int>> Battle(float playerChance, BattleType battleType, Squad player, Squad enemy)
    {
        KeyValuePair<KeyValuePair<Team, BattleType>, Dictionary<string, int>> winner = new KeyValuePair<KeyValuePair<Team, BattleType>, Dictionary<string, int>>();
        if (battleType == BattleType.Space)
        {
            var win = player.Combat(enemy, playerChance);
            winner = new KeyValuePair<KeyValuePair<Team, BattleType>, Dictionary<string, int>>
                (new KeyValuePair<Team, BattleType>(win.Key, battleType), win.Value);
        }
        else if (battleType == BattleType.Invasion)
        {
            var pt = player.GetComponent<Tile>();
            var et = enemy.GetComponent<Tile>();

            var win = new KeyValuePair<Team, Dictionary<string, int>>();
            if (pt != null) // player tile vs. enemy squad
                win = enemy.Combat(pt, 1.0f - playerChance);
            else if (et != null) // player squad vs. enemy tile
                win = player.Combat(et, playerChance);

            // do nothing / undeploy as necessary
            if (win.Key == _team && et != null)
            {
                et.Relinquish();
                et.Claim(_team);
                GameManager.Instance.Players[et.Team].CreateUndeployEvent(et, true);
            }
            else if (win.Key == enemy.Team && pt != null)
            {
                pt.Relinquish();
                pt.Claim(enemy.Team);
                GameManager.Instance.Players[pt.Team].CreateUndeployEvent(pt, true);
                var type = pt.Undeploy(true);
                pt.Claim(enemy.Team);
            }

            winner = new KeyValuePair<KeyValuePair<Team, BattleType>, Dictionary<string, int>>
                (new KeyValuePair<Team, BattleType>(win.Key, battleType), win.Value);
        }

        GameManager.Instance.Players[player.Team].CleanSquad(player);
        GameManager.Instance.Players[enemy.Team].CleanSquad(enemy);
        return winner;
    }

    public void EndBattleConditions(bool win)
    {
        if(win)
        {
            if (_currentBattleType == BattleType.Space)
            {
            }
        }
        else
        {
            if (_currentBattleType == BattleType.Space)
            {
            }
        }

        GameManager.Instance.Paused = false;

        if (_controlledSquad != null)
            Control(_controlledSquad.gameObject);
        _playerSquad = null;
        _enemySquad = null;
    }

    public virtual void CleanSquad(Squad squad)
    {
        foreach(var sq in squad.Colliders)
            if(sq != null && sq.gameObject != null)
                Squad.CleanSquadsFromList(this, sq.Colliders);
        Squad.CleanSquadsFromList(this, squad.Colliders);

        if ((squad != null && squad.Ships.Count == 0 && _controlledTile == null))
            DeleteSquad(squad);

        if(_controlledSquad == null || (_controlledSquad.Ships.Count == 0 && _controlledTile == null))
        {
            var colliders = squad.Colliders;
            if(colliders.Count > 0)
            {
                for(int i = 0; i < colliders.Count; i++)
                    if(colliders[i].Team == _team)
                        Control(colliders[0].gameObject);
            }

            if (_commandShipSquad == null || !_commandShipSquad.Ships.Contains(_commandShip))
                CreateCommandShipLostEvent(_commandShipSquad);
            else if (_squads.Count > 0 && _controlledSquad == null)
                Control(_squads[GameManager.Generator.Next(0, _squads.Count)].gameObject);
        }
    }

    public void DeleteSquad(Squad squad)
    {
        _squads.Remove(squad);

        if(squad != null)
            GameObject.Destroy(squad.gameObject);
    }

    public Squad CreateNewSquad(Squad fromSquad, string name = "Squad")
    {
        var val = GameManager.Generator.Next(2);

        float dist;
        var tile = fromSquad.GetComponent<Tile>();
        if (tile != null)
            dist = tile.Radius / 2.0f;
        else
            dist = fromSquad.GetComponent<SphereCollider>().radius / 2.0f;
        var offset = val == 0 ? new Vector3(dist, 0, 0) : new Vector3(0, 0, dist);
        var squad = CreateNewSquad(fromSquad.transform.position + offset, fromSquad.Sector, name);
        fromSquad.Colliders.Add(squad);
        squad.Colliders.Add(fromSquad);
        return squad;
    }

    public Squad CreateNewSquad(Vector3 position, Sector sector, string name = "Squad")
    {
        var squadobj = Resources.Load<GameObject>(SQUAD_PREFAB);
        var squad = Instantiate(squadobj, position, Quaternion.identity) as GameObject;
        var component = squad.GetComponent<Squad>();
        component.Team = _team;
        _squads.Add(component);
        component.Init(sector, name);
        return component;
    }

    public void CreateNewCommandShip()
    {
        // determine location
        var position = new Vector3(GameManager.Generator.Next() % 20 - 10, 0, GameManager.Generator.Next() % 20 - 10);
        _commandShipSquad = CreateNewSquad(position, null, "Command Squad");
        _commandShipSquad.Ships.Add(_commandShip = _shipDefinitions["Command Ship"].Copy());
        Control(_commandShipSquad.gameObject);
    }
}
