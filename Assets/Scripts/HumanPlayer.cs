using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class HumanPlayer : Player
{
    public static HumanPlayer _instance;
    private const string COMMAND_SHIP_TAG = "CommandShip";
    private const string SQUAD_TAG = "Squad";
    private const string TILE_TAG = "Tile";
    private const string SECTOR_TAG = "Sector";
    private const string COMMAND_SHIP_PREFAB = "CommandShip";
    private const string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    private readonly Vector3 CAMERA_OFFSET = new Vector3(0, 20, -13);

    private Vector3 _currentCameraDistance;
    public static HumanPlayer Instance { get { return _instance; } } // move this to a GameManager registry!
    public Squad Squad { get { return _controlledSquad; } }
    public Tile Tile { get { return _controlledTile; } }

    public override void Init(Team team)
    {
        base.Init(team);

        _instance = this;

        // create command ship, look at it, control it     
        _commandShip = CreateNewSquad(Vector3.zero);
        _commandShip.AddShip(_shipDefinitions["Command Ship"]);

        var squad = CreateNewSquad(new Vector3(0, 0, 10.5f));
        squad.AddShip(_shipDefinitions["Base"].Copy());
        squad.AddShip(_shipDefinitions["Gathering Complex"].Copy());
        squad.AddShip(_shipDefinitions["Military Complex"].Copy());
        squad.AddShip(_shipDefinitions["Research Complex"].Copy());

        squad = CreateNewSquad(new Vector3(0, 0, 11f));
        squad.AddShip(_shipDefinitions["Fighter"].Copy());
        squad.AddShip(_shipDefinitions["Transport"].Copy());
        squad.AddShip(_shipDefinitions["Heavy Fighter"].Copy());
        squad.AddShip(_shipDefinitions["Behemoth"].Copy());
        /* debug */

        _controlledSquad = _commandShip;
        Camera.main.transform.position = _commandShip.transform.position + CAMERA_OFFSET;
        Camera.main.transform.LookAt(_commandShip.transform);
        GUIManager.Instance.SquadSelected(_commandShip);
        GUIManager.Instance.SetSquadControls(_controlledSquad);
    }

    void Start()
    {
    }

    void Update()
    {
        if (GameManager.Instance.Paused || _turnEnded)
            return;

        // right click - control
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) // check for squads
            {
                Control(hit.collider.gameObject);
                ReloadGameplayUI();
            }
            else if (Physics.Raycast(ray, out hit)) // check for planets
            {
                Control(hit.collider.gameObject);
                ReloadGameplayUI();
            }
        }

        if (Input.GetKey(KeyCode.C))
        {
            Control(_commandShip.gameObject);
            GUIManager.Instance.SquadSelected(_commandShip);
        }

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
                case SQUAD_TAG:
                    if (_commandShip == _controlledSquad)
                        UpdateCommandShip();
                    else
                        UpdateSquad();
                    break;
            }
        }
    }

    private void UpdateSelectedPlanet()
    {
        GUIManager.Instance.SetSquadControls(_controlledSquad);
    }

    private void UpdateSquad()
    {
        /*
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                switch (hit.collider.tag)
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
        */

        GUIManager.Instance.SetSquadControls(_controlledSquad);
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

    public Ship GetShipDefinition(string name)
    {
        return _shipDefinitions[name];
    }

    public override void TurnEnd()
    {
        base.TurnEnd();
        ReloadGameplayUI();
    }

    public void ReloadGameplayUI()
    {
        switch(_controlledSquad.tag)
        {
            case TILE_TAG:
                GUIManager.Instance.TileSelected(_controlledTile, _numResearchStations, _shipDefinitions);
                break;
            case SQUAD_TAG:
            case COMMAND_SHIP_TAG:
                GUIManager.Instance.SquadSelected(_controlledSquad);
                break;
        }
    }

    public override void Control(GameObject gameObject)
    {
        base.Control(gameObject);
        transform.position = _controlledSquad.transform.position + _currentCameraDistance;
    }

    public override float PrepareBattleConditions(Squad squad1, Squad squad2)
    {
        _winChance = base.PrepareBattleConditions(squad1, squad2);
        Control(_playerSquad.gameObject);
        GUIManager.Instance.ConfigureBattleScreen(_winChance, _playerSquad, _enemySquad);
        return _winChance;
    }

    public override Squad Battle(float playerChance, Squad player, Squad enemy)
    {
        return base.Battle(_winChance, _playerSquad, _enemySquad);
    }
}

