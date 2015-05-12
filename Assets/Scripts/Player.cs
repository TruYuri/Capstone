using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The base class for both player types (human and AI).
/// Contains many base functions that the player types can use or expand.
/// </summary>
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
    protected Dictionary<string, HashSet<Ship>> _shipRegistry;
    protected Dictionary<Inhabitance, int> _soldierRegistry;
    protected Dictionary<Resource, int> _resourceRegistry;
    protected List<Squad> _squads;
    protected List<Tile> _tiles;
    protected ResearchTree _militaryTree;
    protected ResearchTree _scienceTree;
    protected float _rcostReduction;

    // these variables control messaging and command range
    protected bool _controlledIsWithinRange;
    protected int _relayDistance;

    public Team Team { get { return _team; } }
    public List<Tile> Tiles { get { return _tiles; } }
    public List<Squad> Squads { get { return _squads; } }
    public bool TurnEnded { get { return _turnEnded; } }
    public bool ControlledIsWithinRange { get { return _controlledIsWithinRange; } }
    public Dictionary<string, Ship> ShipDefinitions { get { return _shipDefinitions; } }
    public float ResearchCostReduction
    {
        get { return _rcostReduction; }
        set { _rcostReduction = value; }
    }

    protected Squad _playerSquad;
    protected Squad _enemySquad;
    protected GameEvent _p1Mission;
    protected GameEvent _p2Mission;
    protected float _winChance;
    protected BattleType _currentBattleType;

    /// <summary>
    /// Initializes the player. 
    /// Asks the GameManager for new player definitions.
    /// </summary>
    /// <param name="team">This player's team.</param>
    public virtual void Init(Team team)
    {
        _team = team;
        _squads = new List<Squad>();
        _tiles = new List<Tile>();
        _shipDefinitions = GameManager.Instance.GenerateShipDefs();
        _militaryTree = GameManager.Instance.GenerateMilitaryTree(_shipDefinitions);
        _scienceTree = GameManager.Instance.GenerateScienceTree(_shipDefinitions, this);
        _shipRegistry = new Dictionary<string, HashSet<Ship>>();
        _soldierRegistry = new Dictionary<Inhabitance, int>();
        _shipRegistry = new Dictionary<string, HashSet<Ship>>();
        _resourceRegistry = new Dictionary<Resource, int>();
        foreach(var ship in _shipDefinitions)
        {
            ship.Value.RecalculateResources();
            if(!_shipRegistry.ContainsKey(ship.Key))
                _shipRegistry.Add(ship.Key, new HashSet<Ship>());
        }

        _soldierRegistry.Add(Inhabitance.Primitive, 0);
        _soldierRegistry.Add(Inhabitance.Industrial, 0);
        _soldierRegistry.Add(Inhabitance.SpaceAge, 0);
        _resourceRegistry.Add(Resource.Ore, 0);
        _resourceRegistry.Add(Resource.Oil, 0);
        _resourceRegistry.Add(Resource.Forest, 0);
        _resourceRegistry.Add(Resource.Asterminium, 0);
        _resourceRegistry.Add(Resource.Stations, 0);
    }

    /// <summary>
    /// Sets this player's current controlled object. Calculates if the new squad is in range of the command squad.
    /// </summary>
    /// <param name="gameObject">The object to control.</param>
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
        _relayDistance = path != null ? (path.Count) : 0;
    }

	/// <summary>
	/// Updates base player behavior.
	/// </summary>
	void Update () 
    {
        if (GameManager.Instance.Paused || _turnEnded)
            return;

        EndTurn();
	}

    /// <summary>
    /// Upgrades the research of the specified type, name, and detail.
    /// </summary>
    /// <param name="type">Military or Scientific</param>
    /// <param name="research">The name of the research to upgrade.</param>
    /// <param name="property">The specific property of that research to upgrade.</param>
    public void UpgradeResearch(string type, string research, string property)
    {
        Dictionary<Resource, int> change = null;
        if (property == "Unlock")
        {
            if (type == MILITARY)
                change = _militaryTree.GetResearch(research).Unlock(_rcostReduction);
            else if (type == SCIENCE)
                change = _scienceTree.GetResearch(research).Unlock(_rcostReduction);
        }
        else
        {
            if (type == MILITARY)
                change = _militaryTree.GetResearch(research).UpgradeResearch(property, _rcostReduction);
            else if (type == SCIENCE)
                change = _scienceTree.GetResearch(research).UpgradeResearch(property, _rcostReduction);
        }

        // subtract resources
        var types = change.Keys.ToList();
        foreach (var t in types)
        {
            while (change[t] > 0)
            {
                foreach (var tile in _tiles)
                    if (tile.Structure != null && tile.Structure.Resources[t] > 0)
                    {
                        RemoveResources(tile.Structure, t, 1);
                        change[t] -= 1;

                        if (change[t] == 0)
                            break;
                    }
            }
        }
    }

    /// <summary>
    /// Instantiates a battle event for this player.
    /// </summary>
    /// <param name="squad1">The combating squad.</param>
    /// <param name="tile">The combating tile.</param>
    public void CreateBattleEvent(Squad squad1, Tile tile)
    {
        GameManager.Instance.AddEvent(new BattleEvent(_relayDistance, squad1, tile), true);
    }

    /// <summary>
    /// Instantiates a battle event for this player.
    /// </summary>
    /// <param name="squad1">The first combating squad.</param>
    /// <param name="squad2">The second combating squad.</param>
    public void CreateBattleEvent(Squad squad1, Squad squad2)
    {
        GameManager.Instance.AddEvent(new BattleEvent(squad1, squad2), true);
    }

    /// <summary>
    /// Instantiates a retreat event for this player's controlled squad.
    /// </summary>
    public void CreateRetreatEvent()
    {
        GameManager.Instance.AddEvent(new RetreatEvent(this, _playerSquad, _enemySquad), true);
    }

    /// <summary>
    /// Instantiates a build event for this player at the controlled tile.
    /// </summary>
    /// <param name="shipName">The name of the ship to build.</param>
    public void CreateBuildEvent(string shipName)
    {
        GameManager.Instance.AddEvent(new BuildEvent(_relayDistance, this, _controlledTile, _shipDefinitions[shipName].Copy()), false);

        foreach(var r in _shipDefinitions[shipName].RequiredResources)
        {
            RemoveResources(_controlledTile.Structure, r.Key, r.Value);
        }

        EndTurn();
    }

    /// <summary>
    /// Instantiates a deploy event for this player.
    /// </summary>
    /// <param name="shipIndex">The index of the ship in the controlled squad's ship list to deploy.</param>
    public void CreateDeployEvent(int shipIndex)
    {
        GameManager.Instance.AddEvent(new DeployEvent(_relayDistance, this, _controlledSquad.Ships[shipIndex] as Structure, _controlledSquad, _controlledSquad.Tile), false);
        EndTurn();
    }

    /// <summary>
    /// Instantiates an undeploy event for this player at the controlled tile.
    /// </summary>
    /// <param name="destroy">Indicates whether the deployed structure should be destroyed.</param>
    public void CreateUndeployEvent(bool destroy)
    {
        CreateUndeployEvent(_controlledTile, destroy);
    }

    /// <summary>
    /// Instantiates an undeploy event for this player at a specified tile.
    /// </summary>
    /// <param name="tile">The tile to undeploy from.</param>
    /// <param name="destroy">Indicates whether the deployed structure should be destroyed.</param>
    public void CreateUndeployEvent(Tile tile, bool destroy)
    {
        GameManager.Instance.AddEvent(new UndeployEvent(_relayDistance, this, tile, destroy), false);
        EndTurn();
    }

    /// <summary>
    /// Instantiates a diplomacy event for this player at the controlled tile.
    /// </summary>
    public void CreateDiplomacyEvent()
    {
        GameManager.Instance.AddEvent(new DiplomacyEvent(_relayDistance, this, _controlledTile), false);
        EndTurn();
    }

    /// <summary>
    /// Instantiates a chase event for this player.
    /// </summary>
    /// <param name="squad">The player's squad.</param>
    /// <param name="chase">The squad to chase.</param>
    /// <param name="ht">The squad's anchor tile (can be null).</param>
    /// <param name="range">The range from the possible anchor it can go.</param>
    /// <param name="speed">The velocity at which to chase.</param>
    public void CreateChaseEvent(Squad squad, Squad chase, Tile ht, float range, float speed)
    {
        GameManager.Instance.AddEvent(new ChaseEvent(squad, chase, ht, range, speed), true);
    }

    /// <summary>
    /// Instantiates a travel event for this player.
    /// </summary>
    /// <param name="squad">The squad to travel.</param>
    /// <param name="toSector">The sector to travel to.</param>
    /// <param name="dest">The specific coordinates to reach.</param>
    /// <param name="speed">The speed at which to travel between turns.</param>
    public void CreateTravelEvent(Squad squad, Sector toSector, Vector3 dest, float speed)
    {
        GameManager.Instance.AddEvent(new TravelEvent(_relayDistance, this, squad, toSector, dest, speed), false);
        EndTurn();
    }

    /// <summary>
    /// Instantiates a Command Ship Lost event for this player.
    /// </summary>
    /// <param name="pos">The position the command ship was destroyed.</param>
    /// <param name="squad">The command ship's former squad, if it still exists.</param>
    public void CreateCommandShipLostEvent(Vector3 pos, Squad squad)
    {
        GameManager.Instance.AddEvent(new CommandShipLostEvent(pos, squad, this), true);
    }

    /// <summary>
    /// Instantiates a warp event for this player.
    /// </summary>
    /// <param name="exitGate">The tile/portal to exit from.</param>
    /// <param name="squad">The squad to warp.</param>
    public void CreateWarpEvent(Tile exitGate, Squad squad)
    {
        GameManager.Instance.AddEvent(new WarpEvent(_relayDistance, this, squad, exitGate), false);
        EndTurn();
    }

    /// <summary>
    /// Simply ends this player's turn.
    /// </summary>
    public void EndTurn()
    {
        _turnEnded = true;
    }

    /// <summary>
    /// Sums the number of applicable research resources.
    /// </summary>
    public void CountStations()
    {
        _resourceRegistry[Resource.Stations] = _shipRegistry["Research Complex"].Count(r => r.IsDeployed == true);
        _resourceRegistry[Resource.Stations] += _shipRegistry["Base"].Count(r => r.IsDeployed == true);
    }

    // DON'T CALL THIS FROM HERE - for GameManager!
    /// <summary>
    /// Used by GameManager at the end of each turn. Progresses any owned tile growth.
    /// </summary>
    public virtual void TurnEnd()
    {
        CountStations();

        foreach (var tile in _tiles)
            tile.GatherAndGrow();

        _turnEnded = false;
    }

    /// <summary>
    /// Initializes battle between two squads.
    /// </summary>
    /// <param name="squad1">The first battle squad.</param>
    /// <param name="squad2">The second battle squad.</param>
    /// <param name="battleType">The type of battle to start.</param>
    /// <returns>The clamped win chance for this player.</returns>
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

        _p1Mission = _playerSquad.Mission;
        _p2Mission = _enemySquad.Mission;

        var pt = _playerSquad.GetComponent<Tile>();
        var et = _enemySquad.GetComponent<Tile>();

        float WC = 0f;
        if ((pt == null && et == null) ||
            (pt != null && pt.Squad.Ships.Count > 0) ||
            (et != null && et.Squad.Ships.Count > 0))
            WC = _playerSquad.GenerateWinChance(_enemySquad);
        else if (pt != null && et == null)
            WC = 1.0f - _enemySquad.GenerateWinChance(pt);
        else if (pt == null && et != null)
            WC = _playerSquad.GenerateWinChance(et);

        return Mathf.Clamp01(WC);
    }

    /// <summary>
    /// Attempts a diplomatic effort.
    /// </summary>
    /// <param name="tile">The tile to attempt diplomacy upon.</param>
    /// <returns>A boolean indicating success.</returns>
    public bool DiplomaticEffort(Tile tile)
    {
        var IP = tile.Population * 2;

        int PP = 0;
        switch(tile.PopulationType)
        {
            case Inhabitance.Primitive:
                PP = _soldierRegistry[Inhabitance.Primitive] * 2 + _soldierRegistry[Inhabitance.Industrial] + _soldierRegistry[Inhabitance.SpaceAge];
                break;
            case Inhabitance.Industrial:
                PP = _soldierRegistry[Inhabitance.Primitive] + _soldierRegistry[Inhabitance.Industrial] * 2 + _soldierRegistry[Inhabitance.SpaceAge];
                break;
            case Inhabitance.SpaceAge:
                PP = _soldierRegistry[Inhabitance.Primitive] + _soldierRegistry[Inhabitance.Industrial] + _soldierRegistry[Inhabitance.SpaceAge] * 2;
                break;
        }

        //var DC = ((PP - IP) / 100.0f * 0.5f + 0.5f) * (_team == Team.Union ? 1.5f : 1f);
		var DC = (((float)PP / (PP + IP))  - ((float)IP / (PP + IP))) * (_team == Team.Union ? 1.5f : 1f);
        var DP = (float)GameManager.Generator.NextDouble();

        if (DP < DC) // we won!
        {
            if (this == HumanPlayer.Instance)
                GUIManager.Instance.AddEvent("Diplomacy successful at " + tile.Name + "!");

            tile.Claim(_team);
            tile.Population += Mathf.FloorToInt(tile.Population * ((float)GameManager.Generator.NextDouble() * (0.1f - 0.3f) + 0.25f)); // this is very broken
            // AddSoldiers(tile, tile.PopulationType, tile.Population);
            foreach(var ship in tile.Squad.Ships)
            {
                _shipRegistry[ship.Name].Add(ship);
                foreach(var p in ship.Population)
                {
                    AddSoldiers(p.Key, p.Value);
                }
            }
            return true;
        }
        else if (this == HumanPlayer.Instance)
            GUIManager.Instance.AddEvent("Diplomacy failed at " + tile.Name + ".");

        tile.EndDiplomaticEffort(_team);

        return false;
    }

    /// <summary>
    /// Battles two squads.
    /// </summary>
    /// <param name="playerChance">The odds of this player's success.</param>
    /// <param name="battleType">The type of battle.</param>
    /// <param name="player">This player's squad.</param>
    /// <param name="enemy">The combating squad.</param>
    /// <returns>The winning team and battle type, as well as the list of units lost.</returns>
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
                et.Undeploy(true);
                et.Claim(_team);
            }
            else if (win.Key == enemy.Team && pt != null)
            {
                pt.Undeploy(true);
                pt.Claim(enemy.Team);
            }

            winner = new KeyValuePair<KeyValuePair<Team, BattleType>, Dictionary<string, int>>
                (new KeyValuePair<Team, BattleType>(win.Key, battleType), win.Value);
        }

        GameManager.Instance.Players[player.Team].CleanSquad(player);
        GameManager.Instance.Players[enemy.Team].CleanSquad(enemy);
        return winner;
    }

    /// <summary>
    /// Cleans up variables post-battle.
    /// </summary>
    /// <param name="win">Whether or not this player won the ending battle.</param>
    public void EndBattleConditions(bool win)
    {
        GameManager.Instance.Paused = false;

        if (_playerSquad != null)
            _playerSquad.Mission = _p1Mission;
        if (_enemySquad != null)
            _enemySquad.Mission = _p2Mission;

        if (_controlledSquad != null)
            Control(_controlledSquad.gameObject);
        _playerSquad = null;
        _enemySquad = null;
    }

    /// <summary>
    /// Utility; cleans up squads around the selected if they have been removed from play.
    /// </summary>
    /// <param name="squad">The squad to clean up.</param>
    public virtual void CleanSquad(Squad squad)
    {
        var p = squad.transform.position;

        foreach(var sq in squad.Colliders)
            if(sq != null && sq.gameObject != null)
                Squad.CleanSquadsFromList(this, sq.Colliders);
        Squad.CleanSquadsFromList(this, squad.Colliders);

        if ((squad != null && squad.Ships.Count == 0 && squad.GetComponent<Tile>() == null))
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
         
            if (_squads.Count > 0 && _controlledSquad == null)
                Control(_squads[GameManager.Generator.Next(0, _squads.Count)].gameObject);
        }
        
        if(_team == HumanPlayer.Instance.Team)
        {
            if(_commandShipSquad == null || _commandShipSquad.gameObject == null)
                CreateCommandShipLostEvent(p, _commandShipSquad);
            else if (!_commandShipSquad.Ships.Contains(_commandShip))
            {
                _commandShipSquad.name = "Squad";
                _commandShipSquad = null;
                CreateCommandShipLostEvent(p, _commandShipSquad);
            }

            if (_squads.Count > 0 && _controlledSquad == null && _commandShipSquad != null)
                Control(_squads[GameManager.Generator.Next(0, _squads.Count)].gameObject);
        }
    }

    /// <summary>
    /// Removes a squad from this player's registry.
    /// </summary>
    /// <param name="squad">The squad to remove.</param>
    public void DeleteSquad(Squad squad)
    {
        _squads.Remove(squad);

        if(squad != null)
            GameObject.Destroy(squad.gameObject);
    }

    /// <summary>
    /// Creates a new squad for this player.
    /// </summary>
    /// <param name="fromSquad">Squad to branch from.</param>
    /// <param name="name">Name of the new squad.</param>
    /// <returns>The new squad.</returns>
    public Squad CreateNewSquad(Squad fromSquad, string name = "Squad")
    {
        float dist;
        var tile = fromSquad.GetComponent<Tile>();
        if (tile != null)
            dist = tile.Radius;
        else
            dist = fromSquad.GetComponent<SphereCollider>().radius;

        var angle = (float)GameManager.Generator.NextDouble() * Mathf.PI * 2;
        var x = Mathf.Cos(angle) * dist;
        var z = Mathf.Sin(angle) * dist;

        var offset = new Vector3(x, 0, z);
        var squad = CreateNewSquad(fromSquad.transform.position + offset, fromSquad.Sector, name);
        fromSquad.Colliders.Add(squad);
        squad.Colliders.Add(fromSquad);
        return squad;
    }

    /// <summary>
    /// Creates a new squad for this player.
    /// </summary>
    /// <param name="position">Position to create this squad at.</param>
    /// <param name="sector">The sector for the new squad.</param>
    /// <param name="name">The new squad's name.</param>
    /// <returns>The new squad.</returns>
    public Squad CreateNewSquad(Vector3 position, Sector sector, string name = "Squad")
    {
        var squadobj = Resources.Load<GameObject>(SQUAD_PREFAB);
        var squad = Instantiate(squadobj, position, Quaternion.identity) as GameObject;
        var component = squad.GetComponent<Squad>();
        component.Team = _team;
        _squads.Add(component);
        component.Init(_team, sector, name);
        return component;
    }

    /// <summary>
    /// Instantiates a new command ship for this player.
    /// </summary>
    /// <param name="tile">The tile at which to spawn.</param>
    public void CreateNewCommandShip(Tile tile)
    {
        if(tile != null)
            _commandShipSquad = CreateNewSquad(tile.Squad, "Command Squad");
        else
            _commandShipSquad = CreateNewSquad(Vector3.zero, null, "Command Squad");
        _commandShipSquad.Ships.Add(_commandShip = _shipDefinitions["Command Ship"]);
        _shipRegistry[_commandShip.Name].Add(_commandShip);
        Control(_commandShipSquad.gameObject);
    }

    // Utility functions to ensure proper management of resources

    /// <summary>
    /// Claims a tile for this player.
    /// </summary>
    /// <param name="tile">The tile to claim.</param>
    public void ClaimTile(Tile tile)
    {
        _tiles.Add(tile);
        tile.transform.parent.GetComponent<Sector>().ClaimTile(_team);
    }

    /// <summary>
    /// Removes a tile from this player.
    /// </summary>
    /// <param name="tile">The tile to remove.</param>
    public void RelinquishTile(Tile tile)
    {
        _tiles.Remove(tile);
        tile.transform.parent.GetComponent<Sector>().RelinquishTile(_team);
    }

    /// <summary>
    /// Adds a ship to a squad.
    /// </summary>
    /// <param name="squad">The squad to add to.</param>
    /// <param name="name">The name of the ship to add.</param>
    /// <returns>The new ship.</returns>
    public Ship AddShip(Squad squad, string name)
    {
        var ship = _shipDefinitions[name].Copy();
        AddShip(squad, ship);
        return ship;
    }

    /// <summary>
    /// Adds an existing ship to a squad.
    /// </summary>
    /// <param name="squad">The squad to add to.</param>
    /// <param name="ship">The ship to add.</param>
    /// <returns>The added ship.</returns>
    public Ship AddShip(Squad squad, Ship ship)
    {
        _shipRegistry[ship.Name].Add(ship);
        squad.Ships.Add(ship);
        return ship;
    }

    /// <summary>
    /// Removes a ship from a squad.
    /// </summary>
    /// <param name="squad">The squad to remove from.</param>
    /// <param name="ship">The ship to remove.</param>
    public void RemoveShip(Squad squad, Ship ship)
    {
        _shipRegistry[ship.Name].Remove(ship);
        squad.Ships.Remove(ship);

        var pops = ship.Population.Keys.ToList();
        foreach (var type in pops)
            RemoveSoldiers(ship, type, ship.Population[type]);
    }

    /// <summary>
    /// Removes all ships from a squad.
    /// </summary>
    /// <param name="squad">The squad to remove from.</param>
    public void RemoveAllShips(Squad squad)
    {
        var types = _soldierRegistry.Keys.ToList();
        for (int i = 0; i < squad.Ships.Count; i++)
        {
            foreach (var type in types)
                RemoveSoldiers(squad.Ships[i], type, squad.Ships[i].Population[type]);

            _shipRegistry[squad.Ships[i].Name].Remove(squad.Ships[i]);
            squad.Ships[i] = null;
        }

        squad.Ships.Clear();
    }

    /// <summary>
    /// Adds soldiers to a tile.
    /// </summary>
    /// <param name="tile">The tile to add to.</param>
    /// <param name="type">The type of soldier.</param>
    /// <param name="count">The amount of soldiers to add.</param>
    public void AddSoldiers(Tile tile, Inhabitance type, int count)
    {
        tile.Population += count;
        _soldierRegistry[type] += count;
    }

    /// <summary>
    /// Adds soldiers to a ship.
    /// </summary>
    /// <param name="ship">The ship to add to.</param>
    /// <param name="type">The type of soldier.</param>
    /// <param name="count">The amount of soldiers to add.</param>
    public void AddSoldiers(Ship ship, Inhabitance type, int count)
    {
        ship.Population[type] += count;
        _soldierRegistry[type] += count;
    }

    /// <summary>
    /// Adds soldiers to the general registry.
    /// </summary>
    /// <param name="type">The soldier type to add.</param>
    /// <param name="count">The amount of soldiers to add.</param>
    public void AddSoldiers(Inhabitance type, int count)
    {
        _soldierRegistry[type] += count;
    }

    /// <summary>
    /// Removes soldiers from a tile.
    /// </summary>
    /// <param name="tile">The tile to remove from.</param>
    /// <param name="structure">Indicates if the planet structure should be considered.</param>
    /// <param name="type">The soldier type.</param>
    /// <param name="count">The soldier count.</param>
    public void RemoveSoldiers(Tile tile, bool structure, Inhabitance type, int count)
    {
        if(structure && tile.Structure != null)
        {
            var min = tile.Structure.Population[type] < count ? tile.Structure.Population[type] : count;
            count -= min;

            RemoveSoldiers(tile.Structure, type, min);

            if (min > 0 && tile.PopulationType == type)
                tile.Population -= min;
        }
        else
            tile.Population -= count;
        _soldierRegistry[type] -= count;
    }

    /// <summary>
    /// Removes soldiers from a ship.
    /// </summary>
    /// <param name="ship">The ship to remove from.</param>
    /// <param name="type">The soldier type.</param>
    /// <param name="count">The amount of soldiers to remove.</param>
    public void RemoveSoldiers(Ship ship, Inhabitance type, int count)
    {
        ship.Population[type] -= count;
        _soldierRegistry[type] -= count;
    }

    /// <summary>
    /// Adds resources to a ship.
    /// </summary>
    /// <param name="ship">The ship to add to.</param>
    /// <param name="type">The resource type.</param>
    /// <param name="count">The amount of resoures to add.</param>
    public void AddResources(Ship ship, Resource type, int count)
    {
        ship.Resources[type] += count;
        _resourceRegistry[type] += count;
    }

    /// <summary>
    /// Removes resources from a ship.
    /// </summary>
    /// <param name="ship">The ship to remove from.</param>
    /// <param name="type">The resource type.</param>
    /// <param name="count">The amount of resources to remove.</param>
    public void RemoveResources(Ship ship, Resource type, int count)
    {
        ship.Resources[type] -= count;
        _resourceRegistry[type] -= count;
    }
}
