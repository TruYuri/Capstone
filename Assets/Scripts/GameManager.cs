using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour 
{
    public static System.Random Generator = new System.Random();
    private const string PLAYER_PREFAB = "Player";

    private static GameManager _instance;
    private GameObject _player;
    private bool _paused;
    private Queue<GameEvent> _eventQueue;

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

    private Dictionary<string, Ship> _shipStats;
    private GameObject enemy;
	// Use this for initialization
	void Start () 
    {
	    // create player
        _eventQueue = new Queue<GameEvent>();
        var playerObj = Resources.Load<GameObject>(PLAYER_PREFAB);
        _player = Instantiate(playerObj) as GameObject;
        _instance = this;

        _shipStats = new Dictionary<string, Ship>()
        {
            { "Fighter", new Ship("Fighter", 1, 1, 5, 0, false) },
            { "Transport", new Ship("Transport", 10, 0, 2, 100, false) },
            { "Guard Satellite", new Ship("Guard Satellite", 2, 3, 0, 0, true) },
            { "Heavy Fighter", new Ship("Heavy Fighter", 3, 3, 2, 10, false) },
            { "Behemoth", new Ship("Behemoth", 20, 10, 1, 50, false) },
            { "Command Ship", new Ship("Command Ship", 20, 2, 5, 0, false) },
            { "Resource Transport", new Ship("Resource Transport", 50, 0, 1, 0, false) },
            { "Gathering Complex", new Structure("Gathering Complex", 50, 0, 1, 0, 50, 50, 100, false) },
            { "Research Complex", new Structure("Research Complex", 50, 0, 1, 0, 25, 100, 0, false) },
            { "Military Complex", new Structure("Military Complex", 50, 0, 1, 0, 150, 500, 0, false) },
            { "Base", new Structure("Base", 50, 0, 1, 0, 200, 1000, 100, false) },
            { "Relay", new Relay("Relay", 20, 0, 1, 0, 1, 0, false) },
            { "Warp Portal", new WarpPortal("Warp Portal", 20, 0, 1, 0, 2, 0, false)}
        };

        enemy = Instantiate(Resources.Load<GameObject>("Squad"), new Vector3(0, 0, -10), Quaternion.identity) as GameObject;
        var squad = enemy.GetComponent<Squad>();
        squad.AddShip(_shipStats["Fighter"]);
        squad.AddShip(_shipStats["Transport"]);
        squad.AddShip(_shipStats["Heavy Fighter"]);
        squad.AddShip(_shipStats["Behemoth"]);
        squad.AddShip(_shipStats["Command Ship"]);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (_eventQueue.Count > 0)
        {
            _paused = true;

            if (!_eventQueue.Peek().Started)
                _eventQueue.Peek().Begin();
        }
        else
            _paused = false;
	}

    public void AddEvent(GameEvent gameEvent)
    {
        _eventQueue.Enqueue(gameEvent);
    }

    public void PopQueue()
    {
        _eventQueue.Dequeue();
    }

    public void EndTurn()
    {

    }
}
