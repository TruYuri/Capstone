using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Primary manager. Handles players and events.
/// </summary>
public class GameManager : MonoBehaviour 
{
    public static System.Random Generator = new System.Random(); // The consistent random number generated used throughout the game.
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
    private const string RESOURCE_CAPACITY_DETAIL = "ResourceCapacity";
    private const string DEPLOYED_DEFENSE_DETAIL = "DeployedDefense";
    private const string DEPLOYED_CAPACITY_DETAIL = "DeployedCapacity";
    private const string GATHER_RATE_DETAIL = "GatherRate";
    private const string RANGE_DETAIL = "Range";
    private const string N_CONSTRUCTABLES_DETAIL = "nConstructables";
    private const string CONSTRUCTABLE_DETAIL = "Constructable";
    private const string RESOURCE_TYPE_DETAIL = "ResourceGatherType";
    private const string STATIONS_DETAIL = "Stations";
    private const string DESCRIPTION_DETAIL = "Description";

    private static GameManager _instance;
    private bool _paused;
    private Queue<GameEvent> _eventQueue;
    private Queue<GameEvent> _nextEventQueue;
    private Dictionary<string, Ship> _shipDefinitions;
    private Texture2D _shipTextureAtlas;
    private Dictionary<Team, Player> _players;
    private Dictionary<Team, Color> _playerColors;
    private int _turns = 0;

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
    public Dictionary<Team, Color> PlayerColors { get { return _playerColors; } }
    public int Turns { get { return _turns; } }

    /// <summary>
    /// Initializes the game.
    /// </summary>
    /// <param name="player">The team to set the human as.</param>
    public void Init(Team player)
    {
        _instance = this;
        _eventQueue = new Queue<GameEvent>();
        _nextEventQueue = new Queue<GameEvent>();
        _players = new Dictionary<Team, Player>();
        _playerColors = new Dictionary<Team, Color>()
        {
            { Team.Uninhabited, Color.grey },
            { Team.Indigenous, Color.white },
            { Team.Union, Color.blue },
            { Team.Plinthen, Color.green},
            { Team.Kharkyr, Color.red }
        };

        AddHumanPlayer(player);
        var teams = Enum.GetValues(typeof(Team)).Cast<Team>().ToList();
        teams.Remove(player);
        foreach (var t in teams)
            AddAIPlayer(t);

        _shipDefinitions = new Dictionary<string, Ship>();
        var descriptions = new Dictionary<string, string>();
        var shipIcons = new Dictionary<string, Sprite>();
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
            shipIcons.Add(shipNames[i], icon);
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
            var rCapacity = int.Parse(shipDetails[section][RESOURCE_CAPACITY_DETAIL]);

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
                foreach (var t in gatherList)
                    gatherType = gatherType | (ResourceGatherType)Enum.Parse(typeof(ResourceGatherType), t);
                _shipDefinitions.Add(name, new Structure(name, hull, firepower, speed, capacity, rCapacity, dDefense, dCapacity, rate, range, constructables, type, gatherType));
            }
            else
                _shipDefinitions.Add(name, new Ship(name, hull, firepower, speed, capacity, rCapacity, 0, type));

            descriptions.Add(name, shipDetails[section][DESCRIPTION_DETAIL]);
        }

        parser.CloseINI();
        GUIManager.Instance.Init(descriptions, shipIcons);
        MapManager.Instance.Init();

        foreach (var p in _players)
            p.Value.Init(p.Key);
    }

    /// <summary>
    /// Adds a human player to the player registry.
    /// </summary>
    /// <param name="team">The human player team.</param>
    public void AddHumanPlayer(Team team)
    {
        var playerObj = Resources.Load<GameObject>(HUMAN_PLAYER_PREFAB);
        var p = (Instantiate(playerObj) as GameObject).GetComponent<HumanPlayer>();
        _players.Add(team, p);
    }

    /// <summary>
    /// Adds an AI player to the player registry.
    /// </summary>
    /// <param name="team">The AI player team.</param>
    public void AddAIPlayer(Team team)
    {
        var playerObj = Resources.Load<GameObject>(AI_PLAYER_PREFAB);
        var p = (Instantiate(playerObj) as GameObject).GetComponent<AIPlayer>();
        _players.Add(team, p);
    }

    /// <summary>
    /// Generates a new copy of the ship definitions read in at init()
    /// </summary>
    /// <returns>New player ship definitions.</returns>
    public Dictionary<string, Ship> GenerateShipDefs()
    {
        var defs = new Dictionary<string, Ship>();

        foreach (var def in _shipDefinitions)
            defs.Add(def.Key, def.Value.Copy());

        return defs;
    }

    /// <summary>
    /// Generates a new military research tree.
    /// </summary>
    /// <param name="shipDefs">The ship definitions to base the research upon.</param>
    /// <returns>A new military research tree.</returns>
    public ResearchTree GenerateMilitaryTree(Dictionary<string, Ship> shipDefs)
    {
        var tree = new ResearchTree(5);
        tree.AddResearch(1, new FighterResearch(shipDefs["Fighter"], null));
        tree.AddResearch(2, new TransportResearch(shipDefs["Transport"], new List<Research>() { tree.GetResearch(1) }));
        tree.AddResearch(3, new GuardSatelliteResearch(shipDefs["Guard Satellite"], new List<Research>() { tree.GetResearch(2) }));
        tree.AddResearch(4, new HeavyFighterResearch(shipDefs["Heavy Fighter"], new List<Research>() { tree.GetResearch(3) }));
        tree.AddResearch(5, new BehemothResearch(shipDefs["Behemoth"], new List<Research>() { tree.GetResearch(4) }));

        return tree;
    }

    /// <summary>
    /// Generates a new scientific research tree.
    /// </summary>
    /// <param name="shipDefs">The ship definitions to base the research on.</param>
    /// <param name="player">The player this tree will belong to.</param>
    /// <returns>A new scientific research tree.</returns>
    public ResearchTree GenerateScienceTree(Dictionary<string, Ship> shipDefs, Player player)
    {
        var tree = new ResearchTree(5);
        tree.AddResearch(1, new CommandShipResearch(shipDefs["Command Ship"], null));
        tree.AddResearch(2, new EfficiencyResearch(shipDefs, null, player));
        tree.AddResearch(3, new ComplexResearch(shipDefs, null));
        tree.AddResearch(4, new RelayResearch(shipDefs["Relay"] as Structure, null));
        tree.AddResearch(5, new WarpPortalResearch(shipDefs["Warp Portal"] as Structure, new List<Research>() { tree.GetResearch(4) }));

        return tree;
    }
	
    /// <summary>
    /// Processes player turns.
    /// Processes any events in the queue.
    /// </summary>
	void Update () 
    {
        if (_instance == null)
            return;

        // debug
        foreach(var team in _players)
        {
            if(team.Key != HumanPlayer.Instance.Team)
                team.Value.EndTurn();
        }
        // debug

        if (_paused)
            return;

        ProcessEvents();

        if (_paused)
            return;

        int count = 0;
        foreach (var player in _players)
            if (player.Value.TurnEnded)
                count++;
        if (count == _players.Count)
        {
            _eventQueue = _nextEventQueue;
            _nextEventQueue = new Queue<GameEvent>();
            _turns++;
            foreach (var player in _players)
                player.Value.TurnEnd();
        }
	}

    /// <summary>
    /// Registers a new event to process.
    /// </summary>
    /// <param name="gameEvent">The event to process.</param>
    /// <param name="immediate">Indicates whether this event should begin this turn or the next.</param>
    public void AddEvent(GameEvent gameEvent, bool immediate)
    {
        if (immediate)
            _eventQueue.Enqueue(gameEvent);
        else
            _nextEventQueue.Enqueue(gameEvent);
    }

    /// <summary>
    /// Updates all events in the queue.
    /// </summary>
    private void ProcessEvents()
    {
        while (_eventQueue.Count > 0 && !_paused)
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

        foreach (var item in _nextEventQueue)
        {
            if (item.AssertValid())
                item.Update();
        }
    }
}
