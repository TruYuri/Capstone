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
        _commandShip = CreateNewSquad(Vector3.zero, "Command Ship");
        _commandShip.Ships.Add(_shipDefinitions["Command Ship"]);
        _commandShip.Ships.Add(_shipDefinitions["Research Complex"]);

        var squad = CreateNewSquad(new Vector3(0, 0, 10.5f));
        squad.Ships.Add(_shipDefinitions["Base"].Copy());
        squad.Ships.Add(_shipDefinitions["Gathering Complex"].Copy());
        squad.Ships.Add(_shipDefinitions["Military Complex"].Copy());
        squad.Ships.Add(_shipDefinitions["Research Complex"].Copy());

        Ship b, t, t1, t2, t3, t4, f;
        squad = CreateNewSquad(new Vector3(0, 0, 11f));
        squad.Ships.Add(_shipDefinitions["Fighter"].Copy());
        squad.Ships.Add(t = _shipDefinitions["Transport"].Copy());
        squad.Ships.Add(t1 = _shipDefinitions["Transport"].Copy());
        squad.Ships.Add(t2 = _shipDefinitions["Transport"].Copy());
        squad.Ships.Add(t3 = _shipDefinitions["Transport"].Copy());
        squad.Ships.Add(t4 = _shipDefinitions["Transport"].Copy());
        squad.Ships.Add(f = _shipDefinitions["Heavy Fighter"].Copy());
        squad.Ships.Add(b = _shipDefinitions["Behemoth"].Copy());
        b.PrimitivePopulation = 25;
        b.IndustrialPopulation = 15;
        b.SpaceAgePopulation = 10;
        f.SpaceAgePopulation = 3;
        t.PrimitivePopulation = t1.PrimitivePopulation = t2.PrimitivePopulation = t3.PrimitivePopulation = t4.PrimitivePopulation = 50;
        /* debug */

        _controlledSquad = _commandShip;
        _controlledIsWithinRange = true;
        Camera.main.transform.position = _commandShip.transform.position + CAMERA_OFFSET;
        Camera.main.transform.LookAt(_commandShip.transform);
        GUIManager.Instance.SquadSelected(_commandShip);
        GUIManager.Instance.SetSquadControls(_controlledSquad);
        _currentCameraDistance = Camera.main.transform.position - _commandShip.transform.position;
        GUIManager.Instance.SetSquadList(false);
        GUIManager.Instance.SetTileList(false);
    }

    void Start()
    {
        ReloadGameplayUI();
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
            EventSystem eventSystem = EventSystem.current;
            if (Physics.Raycast(ray, out hit) && !eventSystem.IsPointerOverGameObject())
            {
                switch(hit.collider.tag)
                {
                    case SQUAD_TAG:
                        Control(hit.collider.gameObject);
                        break;
                    case SECTOR_TAG:
                        var sector = hit.collider.gameObject.GetComponent<Sector>();
                        var tile = sector.GetTileAtPosition(hit.point);
                        if(tile != null)
                            Control(tile.gameObject);
                        break;
                }

                ReloadGameplayUI();
            }
        }

        if (Input.GetKey(KeyCode.C))
        {
            Control(_commandShip.gameObject);
            ReloadGameplayUI();
        }

        var scrollChange = Input.GetAxis(MOUSE_SCROLLWHEEL);
        var change = 50.0f * scrollChange * Camera.main.transform.forward;
        Camera.main.transform.position += change;
        _currentCameraDistance += change;

        if (_controlledSquad != null)
        {
            transform.position = _controlledSquad.transform.position + _currentCameraDistance;
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
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            EventSystem eventSystem = EventSystem.current;
            if (Physics.Raycast(ray, out hit) && !eventSystem.IsPointerOverGameObject()
                && _controlledSquad.Team == _team && _controlledSquad.OnMission == false)
            {
                CreateTravelEvent(_controlledSquad, hit.collider.gameObject.GetComponent<Sector>(), hit.point, 10.0f);
            }
            
        }

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
                float speed = 50.0f;

                var dir = hit.point - _commandShip.transform.position;
                dir.Normalize();
                _commandShip.transform.position += dir * speed * Time.deltaTime;

                _commandShip.transform.position = new Vector3(_commandShip.transform.position.x, 0.0f, _commandShip.transform.position.z);
                _commandShip.transform.LookAt(hit.point);
                transform.position = _commandShip.transform.position + _currentCameraDistance;
            }
        }

        GUIManager.Instance.SetSquadControls(_controlledSquad);
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
        ReloadGameplayUI();
    }

    public override float PrepareBattleConditions(Squad squad1, Squad squad2, BattleType battleType)
    {
        _winChance = base.PrepareBattleConditions(squad1, squad2, battleType);
        Control(_playerSquad.gameObject);
        GUIManager.Instance.ConfigureBattleScreen(_winChance, _playerSquad, _enemySquad);
        return _winChance;
    }

    public override KeyValuePair<KeyValuePair<Team, BattleType>, Dictionary<string, int>> Battle(float playerChance, BattleType battleType, Squad player, Squad enemy)
    {
        return base.Battle(_winChance, _currentBattleType, _playerSquad, _enemySquad);
    }
}

