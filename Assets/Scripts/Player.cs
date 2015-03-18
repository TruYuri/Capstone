using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    private static Player _instance;

    private const string COMMAND_SHIP_TAG = "CommandShip";
    private const string SQUAD_TAG = "Squad";
    private const string TILE_TAG = "Tile";
    private const string SECTOR_TAG = "Sector";
    private const string COMMAND_SHIP_PREFAB = "CommandShip";
    private const string SQUAD_PREFAB = "Squad";
    private const string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    private readonly Vector3 CAMERA_OFFSET = new Vector3(0, 20, -13);

    // Human player variables - new class? Derive from this for AI later
    private Squad _controlledSquad;
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
    public Team Team { get { return _team; } }

	void Start () 
    {
        _instance = this;
        _team = Team.Union;

        // Initialize ship definitions
        // TODO: .ini this
        _shipStats = new Dictionary<string, Ship>()
        {
            { "Fighter", new Ship("Fighter", 1, 1, 5, 0, ShipType.Combat) },
            { "Transport", new Ship("Transport", 10, 0, 2, 100, ShipType.Combat) },
            { "Guard Satellite", new Ship("Guard Satellite", 2, 3, 0, 0, ShipType.Defense) },
            { "Heavy Fighter", new Ship("Heavy Fighter", 3, 3, 2, 10, ShipType.Combat) },
            { "Behemoth", new Ship("Behemoth", 20, 10, 1, 50, ShipType.Combat) },
            { "Command Ship", new Ship("Command Ship", 20, 2, 5, 0, ShipType.Combat) },
            { "Resource Transport", new Ship("Resource Transport", 50, 0, 1, 0, ShipType.ResourceTransport) },
            { "Gathering Complex", new Structure("Gathering Complex", 50, 0, 1, 0, 50, 50, 100, ShipType.Structure) },
            { "Research Complex", new Structure("Research Complex", 50, 0, 1, 0, 25, 100, 0, ShipType.Structure) },
            { "Military Complex", new Structure("Military Complex", 50, 0, 1, 0, 150, 500, 0, ShipType.Structure) },
            { "Base", new Structure("Base", 50, 0, 1, 0, 200, 1000, 100, ShipType.Structure) },
            { "Relay", new Relay("Relay", 20, 0, 1, 0, 1, 0, ShipType.Combat) },
            { "Warp Portal", new WarpPortal("Warp Portal", 20, 0, 1, 0, 2, 0, ShipType.Combat)}
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
        _commandShip.Team = _team;

        _squads = new List<Squad>();
        _squads.Add(_commandShip);
        
        _commandShip.Ship = _shipStats["Command Ship"].Copy();
        _commandShip.AddShip(_commandShip.Ship);
        _commandShip.AddShip(_shipStats["Base"].Copy());
        _commandShip.AddShip(_shipStats["Base"].Copy());

        _controlledSquad = _commandShip;
        _controlledSquad.IsPlayerControlled = true;
        Camera.main.transform.position = _commandShip.transform.position + CAMERA_OFFSET;
        Camera.main.transform.LookAt(_commandShip.transform);
        GUIManager.Instance.SquadSelected(_commandShip);
        GUIManager.Instance.SetMainListControls(_controlledSquad, null, null);
	}
	
	public void Control(Squad gameObject)
    {
        _controlledSquad.IsPlayerControlled = false;
        _controlledSquad = gameObject;
        _controlledSquad.IsPlayerControlled = true;
        transform.position = _controlledSquad.transform.position + _currentCameraDistance;
    }

	// Update is called once per frame
	void Update () 
    {
        if (GameManager.Instance.Paused)
            return;

        // right click - control
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                switch(hit.collider.tag)
                {
                    case TILE_TAG:
                        Control(hit.collider.gameObject.GetComponent<Squad>());
                        GUIManager.Instance.TileSelected(hit.collider.GetComponent<Tile>());
                        break;
                    case COMMAND_SHIP_TAG:
                    case SQUAD_TAG:
                        Control(hit.collider.gameObject.GetComponent<Squad>());
                        GUIManager.Instance.SquadSelected(hit.collider.GetComponent<Squad>());
                        break;
                }
            }
        }
        
        if(Input.GetKey(KeyCode.C))
            Control(_commandShip);

        var scrollChange = Input.GetAxis(MOUSE_SCROLLWHEEL);
        Camera.main.transform.position += 10.0f * scrollChange * Camera.main.transform.forward;

        if (_controlledSquad != null)
        {
            _currentCameraDistance = this.transform.position - _controlledSquad.transform.position;

            switch (_controlledSquad.tag)
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

        // cleanup dead squads
        foreach(var squad in _squads)
        {
            if (squad.Size < 1)
                Destroy(squad);
        }

        // only run these at turn start or end
        //_militaryResearch.Advance(_numResearchStations);
        //_scientificResearch.Advance(_numResearchStations);
	}

    private void UpdateCommandShip()
    {
        EventSystem eventSystem = EventSystem.current;
        if (Input.GetMouseButton(0) && !eventSystem.IsPointerOverGameObject())
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

        UpdateSquad();
    }

    public bool UpgradeResearch(string type, string research, string property)
    {
        if(type == "Military")
            return _militaryResearch.GetResearch(research).UpgradeResearch(property, _numResearchStations);

        return _scientificResearch.GetResearch(research).UpgradeResearch(property, _numResearchStations);
    }

    private void UpdateSelectedPlanet()
    {

    }

    private void UpdateSquad()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                switch(hit.collider.tag)
                {
                    case TILE_TAG:
                        GUIManager.Instance.SquadToTile(_controlledSquad.GetComponent<Squad>(), hit.collider.GetComponent<Tile>());
                        break;
                    case SECTOR_TAG: // empty space
                        GUIManager.Instance.SquadToSpace(_controlledSquad.GetComponent<Squad>(), hit.point);
                        break;
                    case SQUAD_TAG:
                        GUIManager.Instance.SquadToSquad(_controlledSquad.GetComponent<Squad>(), hit.collider.GetComponent<Squad>());
                        break;
                }
            }
        }
    }

	public Ship GetShipDefinition(string name)
	{
		return _shipStats[name];
	}

    public void Deploy(int shipIndex)
    {
        var tile = _controlledSquad.Deploy(shipIndex);
        Control(tile.Squad);
        GUIManager.Instance.TileSelected(tile);
    }
}
