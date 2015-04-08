using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour 
{
    public static System.Random Generator = new System.Random();
    private const string HUMAN_PLAYER_PREFAB = "HumanPlayer";
    private const string AI_PLAYER_PREFAB = "AIPlayer";
    private const string PLAYER_PREFAB = "Player";
    private const string INI_PATH = "/Resources/Ships.ini";
    private const string SHIP_ICONS_PATH = "ShipIcons/";
    private const string SHIP_SECTION_HEADER = "[Ships]";
    private const string ICON_DETAIL = "IconName";
    private const string HULL_DETAIL = "Hull";
    private const string FIREPOWER_DETAIL = "Firepower";
    private const string SPEED_DETAIL = "Speed";
    private const string CAPACITY_DETAIL = "Capacity";
    private const string DEPLOYED_DEFENSE_DETAIL = "DeployedDefense";
    private const string DEPLOYED_CAPACITY_DETAIL = "DeployedCapacity";
    private const string GATHER_RATE_DETAIL = "GatherRate";
    private const string RANGE_DETAIL = "Range";
    private const string N_CONSTRUCTABLES_DETAIL = "nConstructables";
    private const string CONSTRUCTABLE_DETAIL = "Constructable";
    private const string RESOURCE_TYPE_DETAIL = "ResourceGatherType";
    private const string ORE_DETAIL = "Ore";
    private const string OIL_DETAIL = "Oil";
    private const string ASTERMINIUM_DETAIL = "Asterminium";
    private const string FOREST_DETAIL = "Forest";
    private const string STATIONS_DETAIL = "Stations";
    private const string DESCRIPTION_DETAIL = "Description";

    private static GameManager _instance;
    private bool _paused;
    private Queue<GameEvent> _eventQueue;
    private Queue<GameEvent> _nextEventQueue;
    private Dictionary<string, Ship> _shipDefinitions;
    private Texture2D _shipTextureAtlas;
    private Dictionary<Team, Player> _players;
    private bool _gameStarted;

    public static GameManager Instance 
    { 
        get 
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
            }
            return _instance; 
        }
    }

    public bool Paused 
    {
        get { return _paused; }
        set { _paused = value; }
    }

    public Dictionary<Team, Player> Players { get { return _players; } }

    void Awake()
    {
        _instance = this;
        _eventQueue = new Queue<GameEvent>();
        _nextEventQueue = new Queue<GameEvent>();
        _players = new Dictionary<Team, Player>()
        {
        };

        _shipDefinitions = new Dictionary<string, Ship>();
        var descriptions = new Dictionary<string, string>();
        var parser = new INIParser(Application.dataPath + INI_PATH);
        var shipDetails = parser.ParseINI();
        var textures = new Texture2D[shipDetails.Count - 1]; // to ensure order
        var shipNames = new string[shipDetails.Count - 1]; // to ensure order
        var shipCount = 0;

        foreach (var ship in shipDetails[SHIP_SECTION_HEADER])
        {
            // load texture for atlasing
            textures[shipCount] = Resources.Load<Texture2D>(SHIP_ICONS_PATH + shipDetails["[" + ship.Key + "]"][ICON_DETAIL]);
            shipNames[shipCount++] = ship.Key;
        }

        _shipTextureAtlas = new Texture2D(0, 0);
        var atlasEntries = _shipTextureAtlas.PackTextures(textures, 0);

        for (int i = 0; i < shipCount; i++)
        {
            var rect = new Rect(atlasEntries[i].xMin * _shipTextureAtlas.width, atlasEntries[i].yMin * _shipTextureAtlas.height, textures[i].width, textures[i].height);
            var icon = Sprite.Create(_shipTextureAtlas, rect, new Vector2(0.5f, 0.5f));
            var section = "[" + shipNames[i] + "]";
            var typeList = shipDetails[SHIP_SECTION_HEADER][shipNames[i]].Split('|');
            var type = ShipProperties.None;
            foreach (var t in typeList)
                type = type | (ShipProperties)Enum.Parse(typeof(ShipProperties), t);
            var hull = float.Parse(shipDetails[section][HULL_DETAIL]);
            var firepower = float.Parse(shipDetails[section][FIREPOWER_DETAIL]);
            var speed = float.Parse(shipDetails[section][SPEED_DETAIL]);
            var capacity = int.Parse(shipDetails[section][CAPACITY_DETAIL]);
            var name = shipNames[i];
            var resources = new Dictionary<Resource, int>()
            {
                { Resource.Oil, int.Parse(shipDetails[section][OIL_DETAIL]) },
                { Resource.Ore, int.Parse(shipDetails[section][ORE_DETAIL]) },
                { Resource.Forest, int.Parse(shipDetails[section][FOREST_DETAIL]) },
                { Resource.Asterminium, int.Parse(shipDetails[section][ASTERMINIUM_DETAIL]) },
                { Resource.Stations, int.Parse(shipDetails[section][STATIONS_DETAIL]) },
            };

            if ((type & ShipProperties.Structure) > 0)
            {
                var constructables = new List<string>();
                var n = int.Parse(shipDetails[section][N_CONSTRUCTABLES_DETAIL]);
                for (int j = 0; j < n; j++)
                    constructables.Add(shipDetails[section][CONSTRUCTABLE_DETAIL + j.ToString()]);
                var dDefense = float.Parse(shipDetails[section][DEPLOYED_DEFENSE_DETAIL]);
                var dCapacity = int.Parse(shipDetails[section][DEPLOYED_CAPACITY_DETAIL]);
                var rate = int.Parse(shipDetails[section][GATHER_RATE_DETAIL]);
                var range = int.Parse(shipDetails[section][RANGE_DETAIL]);
                var gatherList = shipDetails[section][RESOURCE_TYPE_DETAIL].Split('|');
                var gatherType = ResourceGatherType.None;
                foreach(var t in gatherList)
                    gatherType = gatherType | (ResourceGatherType)Enum.Parse(typeof(ResourceGatherType), t);
                _shipDefinitions.Add(name, new Structure(icon, name, hull, firepower, speed, capacity, dDefense, dCapacity, rate, range, constructables, type, gatherType, resources));
            }
            else
                _shipDefinitions.Add(name, new Ship(icon, name, hull, firepower, speed, capacity, type, resources));

            descriptions.Add(name, shipDetails[section][DESCRIPTION_DETAIL]);
        }

        parser.CloseINI();
        GUIManager.Instance.Init(descriptions);

        // Research.ini
    }
	// Use this for initialization
	void Start () 
    {
	}

    public void AddHumanPlayer(Team team)
    {
        if (!_players.ContainsKey(team))
            _players.Add(team, null);
        var playerObj = Resources.Load<GameObject>(HUMAN_PLAYER_PREFAB);
        _players[team] = (Instantiate(playerObj) as GameObject).GetComponent<HumanPlayer>();
        _players[team].Init(team);
    }

    public void AddAIPlayer(Team team)
    {
        if (!_players.ContainsKey(team))
            _players.Add(team, null);
        var playerObj = Resources.Load<GameObject>(AI_PLAYER_PREFAB);
        _players[team] = (Instantiate(playerObj) as GameObject).GetComponent<Player>();
        _players[team].Init(team);
    }

    public Dictionary<string, Ship> GenerateShipDefs()
    {
        var defs = new Dictionary<string, Ship>();

        foreach (var def in _shipDefinitions)
            defs.Add(def.Key, def.Value.Copy());

        return defs;
    }

    public ResearchTree GenerateMilitaryTree(Dictionary<string, Ship> shipDefs)
    {
        var tree = new ResearchTree(5);
        tree.AddResearch(1, new FighterResearch(shipDefs["Fighter"]));
        tree.AddResearch(2, new TransportResearch(shipDefs["Transport"]));
        tree.AddResearch(3, new GuardSatelliteResearch(shipDefs["Guard Satellite"]));
        tree.AddResearch(4, new HeavyFighterResearch(shipDefs["Heavy Fighter"]));
        tree.AddResearch(5, new BehemothResearch(shipDefs["Behemoth"]));

        return tree;
    }

    public ResearchTree GenerateScienceTree(Dictionary<string, Ship> shipDefs)
    {
        var tree = new ResearchTree(5);
        tree.AddResearch(1, new CommandShipResearch(shipDefs["Command Ship"]));
        tree.AddResearch(2, new EfficiencyResearch(shipDefs));
        tree.AddResearch(3, new ComplexResearch(shipDefs));
        tree.AddResearch(4, new RelayResearch(shipDefs["Relay"] as Structure));
        tree.AddResearch(5, new WarpPortalResearch(shipDefs["Warp Portal"] as Structure));

        return tree;
    }
	
	void Update () 
    {
        if (!_gameStarted)
        {
            AddHumanPlayer(Team.Union);

            AddAIPlayer(Team.Kharkyr);
            AddAIPlayer(Team.Plinthen);
            AddAIPlayer(Team.Indigenous);

            // debug
            var squad = _players[Team.Kharkyr].CreateNewSquad(new Vector3(0, 0, -10), null);
            var defs = GenerateShipDefs();
            squad.Ships.Add(defs["Fighter"]);
            squad.Ships.Add(defs["Transport"]);
            squad.Ships.Add(defs["Heavy Fighter"]);
            squad.Ships.Add(defs["Behemoth"]);

            _gameStarted = true;
        }

        // debug
        _players[Team.Kharkyr].EndTurn();
        _players[Team.Plinthen].EndTurn();
        _players[Team.Indigenous].EndTurn();

        if(!_paused)
            NextEvent();
	}

    public void AddEvent(GameEvent gameEvent)
    {
        _eventQueue.Enqueue(gameEvent);
    }

    public void NextEvent()
    {
        while (_eventQueue.Count > 0)
        {
            if (_eventQueue.Peek().AssertValid())
            {
                _eventQueue.Peek().Progress();
                _eventQueue.Peek().Update();
            }

            if (_eventQueue.Peek().Stage == GameEventStage.Continue)
                _nextEventQueue.Enqueue(_eventQueue.Dequeue());
            else // end
                _eventQueue.Dequeue();
        }

        var invalid = new List<GameEvent>();
        foreach (var item in _nextEventQueue)
            item.Update();

        int count = 0;
        foreach (var player in _players)
            if (player.Value.TurnEnded)
                count++;
        if(count == _players.Count)
        {
            _eventQueue = _nextEventQueue;
            _nextEventQueue = new Queue<GameEvent>();

            foreach (var player in _players)
                player.Value.TurnEnd();
        }

    }
}
