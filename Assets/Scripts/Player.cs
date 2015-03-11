using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    private static Player _instance;

    private const string COMMAND_SHIP_TAG = "CommandShip";
    private const string SQUAD_TAG = "Squad";
    private const string TILE_TAG = "Tile";
    private const string COMMAND_SHIP_PREFAB = "CommandShip";
    private const string SQUAD_PREFAB = "Squad";
    private const string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    private readonly Vector3 CAMERA_OFFSET = new Vector3(0, 20, -13);

    // Human player variables - new class? Derive from this for AI later
    private GameObject _controlledObject;
    private CommandShip _commandShip;
    private Vector3 _currentCameraDistance;
    //

    private Team _team;
    private Dictionary<string, Ship> _shipStats;
    private List<Squad> _squads;
    private ResearchTree _militaryResearch;
    private ResearchTree _scientificResearch;
    private int _numResearchStations;

    public static Player Instance { get { return _instance; } }

	void Start () 
    {
        _instance = this;
        _team = Team.Union;

        // Initialize ship definitions
        // TODO: .ini this
        _shipStats = new Dictionary<string, Ship>()
        {
            { "Fighter", new Ship("Fighter", 1, 1, 5, 0) },
            { "Transport", new Ship("Transport", 10, 0, 2, 100) },
            { "Guard Satellite", new Ship("Guard Satellite", 2, 3, 0, 0) },
            { "Heavy Fighter", new Ship("Heavy Fighter", 3, 3, 2, 10) },
            { "Behemoth", new Ship("Behemoth", 20, 10, 1, 50) },
            { "Command Ship", new Ship("Command Ship", 20, 2, 5, 0) },
            { "Resource Transport", new Ship("Resource Transport", 1, 0, 0, 0) },
            { "Gathering Complex", new Structure("Gathering Complex", 50, 0, 1, 0, 50, 50) },
            { "Research Complex", new Structure("Research Complex", 50, 0, 1, 0, 25, 100) },
            { "Military Complex", new Structure("Military Complex", 50, 0, 1, 0, 150, 500) },
            { "Base", new Structure("Base", 50, 0, 1, 0, 200, 1000) },
            { "Relay", new Relay("Relay", 20, 0, 1, 0, 1, 0) },
            { "Warp Portal", new WarpPortal("Warp Portal", 20, 0, 1, 0, 2, 0)}
        };

        // Initialize research trees
        // Todo: .ini this
        _militaryResearch = new ResearchTree(5);
        _militaryResearch.AddResearch(1, new FighterResearch(_shipStats["Fighter"]));
        _militaryResearch.AddResearch(2, new TransportResearch(_shipStats["Transport"]));
        _militaryResearch.AddResearch(3, new GuardSatelliteResearch(_shipStats["Guard Satellite"]));
        _militaryResearch.AddResearch(4, new HeavyFighterResearch(_shipStats["Heavy Fighter"]));
        _militaryResearch.AddResearch(5, new BehemothResearch(_shipStats["Behemoth"]));

        _scientificResearch = new ResearchTree(5);
        _scientificResearch.AddResearch(1, new CommandShipResearch(_shipStats["Command Ship"]));
        _scientificResearch.AddResearch(2, new EfficiencyResearch(_shipStats));
        _scientificResearch.AddResearch(3, new ComplexResearch(_shipStats));
        _scientificResearch.AddResearch(4, new RelayResearch(_shipStats["Relay"] as Relay));
        _scientificResearch.AddResearch(5, new WarpPortalResearch(_shipStats["Warp Portal"] as WarpPortal));

        // create command ship, look at it, control it
        var cmdShip = Resources.Load<GameObject>(COMMAND_SHIP_PREFAB);
        var commandShip = Instantiate(cmdShip) as GameObject;
        _commandShip = commandShip.GetComponent<CommandShip>();

        _squads = new List<Squad>();
        _squads.Add(_commandShip);
        
        _commandShip.Ship = _shipStats["Command Ship"].Copy();
        _commandShip.AddShip(_commandShip.Ship);
        _commandShip.AddShip(_shipStats["Base"].Copy());

        _controlledObject = _commandShip.gameObject;
        Camera.main.transform.position = _commandShip.transform.position + CAMERA_OFFSET;
        Camera.main.transform.LookAt(_commandShip.transform);
	}
	
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                switch(hit.collider.tag)
                {
                    case TILE_TAG:
                    case COMMAND_SHIP_TAG:
                    case SQUAD_TAG:
                        _controlledObject = hit.collider.gameObject;
                        transform.position = _controlledObject.transform.position + _currentCameraDistance;
                        break;
                }
            }
        }
        
        if(Input.GetKey(KeyCode.C))
        {
            _controlledObject = _commandShip.gameObject;
            transform.position = _controlledObject.transform.position + _currentCameraDistance;
        }

        var scrollChange = Input.GetAxis(MOUSE_SCROLLWHEEL);
        Camera.main.transform.position += 10.0f * scrollChange * Camera.main.transform.forward;

        if (_controlledObject != null)
        {
            _currentCameraDistance = this.transform.position - _controlledObject.transform.position;

            switch (_controlledObject.tag)
            {
                case TILE_TAG:
                    UpdateSelectedPlanet();
                    break;
                case COMMAND_SHIP_TAG:
                    UpdateCommandShip();
                    break;
                case SQUAD_TAG:
                    UpdateSquad();
                    break;
            }
        }

        foreach(var squad in _squads)
        {
            //if(squad.e)
        }
        // only run these at turn start or end
        //_militaryResearch.Advance(_numResearchStations);
        //_scientificResearch.Advance(_numResearchStations);
	}

    private void UpdateCommandShip()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                float speed = 10.0f;

                var dir = hit.point - _commandShip.transform.position;
                dir.Normalize();
                _commandShip.transform.position += dir * speed * Time.deltaTime;

                _commandShip.transform.position = new Vector3(_commandShip.transform.position.x, 0.0f, _commandShip.transform.position.z);
                _commandShip.transform.LookAt(hit.point);
                transform.position = _commandShip.transform.position + _currentCameraDistance;
            }
        }
    }

    private void UpdateSelectedPlanet()
    {

    }

    private void UpdateSquad()
    {

    }
}
