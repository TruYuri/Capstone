using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour 
{
    public static System.Random Generator = new System.Random();
    private const string PLAYER_PREFAB = "Player";
    private const string INI_PATH = "/Resources/Ships.ini";
    private const string MATERIALS_PATH = "ShipIcons/";
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

    private static GameManager _instance;
    private GameObject _player;
    private bool _paused;
    private Queue<GameEvent> _eventQueue;
    private Queue<GameEvent> _nextEventQueue;
    private Dictionary<string, Ship> _shipDefinitions;
    private Texture2D _textureAtlas;

    public static GameManager Instance 
    { 
        get 
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<GameManager>();
            return _instance; 
        }
    }

    public bool Paused 
    {
        get { return _paused; }
        set { _paused = value; }
    }

    private GameObject enemy;
	// Use this for initialization
	void Start () 
    {
	    // create player
        _eventQueue = new Queue<GameEvent>();
        _nextEventQueue = new Queue<GameEvent>();
        var playerObj = Resources.Load<GameObject>(PLAYER_PREFAB);
        _player = Instantiate(playerObj) as GameObject;
        _instance = this;

        _shipDefinitions = new Dictionary<string, Ship>();
        var parser = new INIParser(Application.dataPath + INI_PATH);
        var shipDetails = parser.ParseINI();
        var textures = new Texture2D[shipDetails.Count - 1]; // to ensure order
        var shipNames = new string[shipDetails.Count - 1]; // to ensure order
        var shipCount = 0;
       
        foreach (var ship in shipDetails[SHIP_SECTION_HEADER])
        {
            // load texture for atlasing
            textures[shipCount] = Resources.Load<Texture2D>(MATERIALS_PATH + shipDetails["[" + ship.Key + "]"][ICON_DETAIL]);
            shipNames[shipCount++] = ship.Key;
        }

        _textureAtlas = new Texture2D(0, 0);
        var atlasEntries = _textureAtlas.PackTextures(textures, 0);

        for (int i = 0; i < shipCount; i++)
        {
            var rect = new Rect(atlasEntries[i].xMin * _textureAtlas.width, atlasEntries[i].yMin * _textureAtlas.height, textures[i].width, textures[i].height);
            var icon = Sprite.Create(_textureAtlas, rect, new Vector2(0.5f, 0.5f));
            var section = "[" + shipNames[i] + "]";
            var type = (ShipType)Enum.Parse(typeof(ShipType), shipDetails[SHIP_SECTION_HEADER][shipNames[i]]);
            var hull = float.Parse(shipDetails[section][HULL_DETAIL]);
            var firepower = float.Parse(shipDetails[section][FIREPOWER_DETAIL]);
            var speed = float.Parse(shipDetails[section][SPEED_DETAIL]);
            var capacity = int.Parse(shipDetails[section][CAPACITY_DETAIL]);
            var name = Regex.Replace(shipNames[i], "([a-z])([A-Z])", "$1 $2");

            switch(type)
            {
                case ShipType.ResourceTransport:
                case ShipType.Defense:
                case ShipType.Combat:
                    _shipDefinitions.Add(name, new Ship(icon, name, hull, firepower, speed, capacity, type));
                    break;
                case ShipType.Structure:
                    // extract constructables
                    var constructables = new List<string>();
                    var n = int.Parse(shipDetails[section][N_CONSTRUCTABLES_DETAIL]);
                    for (int j = 0; j < n; j++)
                        constructables.Add(Regex.Replace(shipDetails[section][CONSTRUCTABLE_DETAIL + j.ToString()], "([a-z])([A-Z])", "$1 $2"));

                    var dDefense = float.Parse(shipDetails[section][DEPLOYED_DEFENSE_DETAIL]);
                    var dCapacity = int.Parse(shipDetails[section][DEPLOYED_CAPACITY_DETAIL]);
                    var rate = int.Parse(shipDetails[section][GATHER_RATE_DETAIL]);
                    _shipDefinitions.Add(name, new Structure(icon, name, hull, firepower, speed, capacity, dDefense, dCapacity, 0, constructables, type));
                    break;
                case ShipType.WarpPortal:
                    _shipDefinitions.Add(name, new WarpPortal(icon, name, hull, firepower, speed, capacity, int.Parse(shipDetails[section][RANGE_DETAIL])));
                    break;
                case ShipType.Relay:
                    _shipDefinitions.Add(name, new Relay(icon, name, hull, firepower, speed, capacity, int.Parse(shipDetails[section][RANGE_DETAIL])));
                    break;
            }
        }

        parser.CloseINI();

        // debug
        var defs = GenerateShipDefs();
        enemy = Instantiate(Resources.Load<GameObject>("Squad"), new Vector3(0, 0, -10), Quaternion.identity) as GameObject;
        var squad = enemy.GetComponent<Squad>();
        squad.AddShip(defs["Fighter"]);
        squad.AddShip(defs["Transport"]);
        squad.AddShip(defs["Heavy Fighter"]);
        squad.AddShip(defs["Behemoth"]);
        squad.AddShip(defs["Command Ship"]);
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
        tree.AddResearch(4, new RelayResearch(shipDefs["Relay"] as Relay));
        tree.AddResearch(5, new WarpPortalResearch(shipDefs["Warp Portal"] as WarpPortal));

        return tree;
    }
	
	void Update () 
    {
       NextEvent();
	}

    public void AddEvent(GameEvent gameEvent)
    {
        _eventQueue.Enqueue(gameEvent);
    }

    public void NextEvent()
    {
        if (_eventQueue.Count > 0)
        {
            _paused = true;

            if (_eventQueue.Peek().Stage == GameEventStage.Begin)
                _eventQueue.Peek().Begin();
            else if (_eventQueue.Peek().Stage == GameEventStage.Continue)
                _nextEventQueue.Enqueue(_eventQueue.Dequeue());
            else // end
                _eventQueue.Dequeue();
        }
        else
            _paused = false;
    }

    public GameEvent CurrentEvent()
    {
        return _eventQueue.Peek();
    }

    public void EndTurn()
    {
        _eventQueue = _nextEventQueue;
        _nextEventQueue = new Queue<GameEvent>();
    }
}
