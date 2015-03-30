using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HumanPlayer : Player
{
    public static HumanPlayer _instance;
    private const string COMMAND_SHIP_TAG = "CommandShip";
    private const string SQUAD_TAG = "Squad";
    private const string TILE_TAG = "Tile";
    private const string SECTOR_TAG = "Sector";
    private const string COMMAND_SHIP_PREFAB = "CommandShip";
    private const string SQUAD_PREFAB = "Squad";
    private const string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    private readonly Vector3 CAMERA_OFFSET = new Vector3(0, 20, -13);

    private CommandShip _commandShip;
    private Vector3 _currentCameraDistance;
    public static HumanPlayer Instance { get { return _instance; } } // move this to a GameManager registry!

    void Start()
    {
        _instance = this;
        _team = Team.Union;
        _shipDefinitions = GameManager.Instance.GenerateShipDefs();
        _militaryTree = GameManager.Instance.GenerateMilitaryTree(_shipDefinitions);
        _scienceTree = GameManager.Instance.GenerateScienceTree(_shipDefinitions);

        // create command ship, look at it, control it
        var cmdShip = Resources.Load<GameObject>(COMMAND_SHIP_PREFAB);
        var commandShip = Instantiate(cmdShip) as GameObject;
        _commandShip = commandShip.GetComponent<CommandShip>();
        _commandShip.Team = _team;

        _squads = new List<Squad>();
        _squads.Add(_commandShip);

        _commandShip.Ship = _shipDefinitions["Command Ship"].Copy();
        _commandShip.AddShip(_commandShip.Ship);
        _commandShip.AddShip(_shipDefinitions["Base"].Copy());

        _controlledSquad = _commandShip;
        _controlledSquad.IsPlayerControlled = true;
        Camera.main.transform.position = _commandShip.transform.position + CAMERA_OFFSET;
        Camera.main.transform.LookAt(_commandShip.transform);
        GUIManager.Instance.SquadSelected(_commandShip);
        GUIManager.Instance.SetMainListControls(_controlledSquad, null, null);
    }

    void Update()
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
                switch (hit.collider.tag)
                {
                    case TILE_TAG:
                        Control(hit.collider.gameObject.GetComponent<Squad>());
                        GUIManager.Instance.TileSelected(hit.collider.GetComponent<Tile>(), _shipDefinitions);
                        break;
                    case COMMAND_SHIP_TAG:
                    case SQUAD_TAG:
                        Control(hit.collider.gameObject.GetComponent<Squad>());
                        GUIManager.Instance.SquadSelected(hit.collider.GetComponent<Squad>());
                        break;
                }
            }
        }

        if (Input.GetKey(KeyCode.C))
        {
            Control(_commandShip);
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
                case COMMAND_SHIP_TAG:
                    UpdateCommandShip();
                    break;
                case SQUAD_TAG:
                    UpdateSquad();
                    break;
            }
        }
    }

    public override void Control(Squad gameObject)
    {
        _controlledSquad.IsPlayerControlled = false;
        base.Control(gameObject);
        _controlledSquad.IsPlayerControlled = true;
        transform.position = _controlledSquad.transform.position + _currentCameraDistance;
    }

    private void UpdateSelectedPlanet()
    {
        GUIManager.Instance.UpdateSelectedPlanet(_controlledSquad.gameObject.GetComponent<Tile>(), _shipDefinitions);
    }

    private void UpdateSquad()
    {
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

    public override void Deploy(int shipIndex)
    {
        var tile = _controlledSquad.Deploy(shipIndex);
        Control(tile.Squad);
        GUIManager.Instance.TileSelected(tile, _shipDefinitions);
        GameManager.Instance.EndTurn();
    }

    public override void Battle()
    {
        BattleEvent gameEvent = GameManager.Instance.CurrentEvent() as BattleEvent;
        Control(gameEvent.Player.GetComponent<Squad>());

        if (gameEvent.Type == GameEventType.SquadBattle)
            _controlledSquad.Combat(gameEvent.Enemy.GetComponent<Squad>());
        else
            _controlledSquad.Combat(gameEvent.Enemy.GetComponent<Tile>());
        gameEvent.Progress();
    }
}

