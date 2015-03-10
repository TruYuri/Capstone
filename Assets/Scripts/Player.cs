using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    private static Player _instance;

    private const string COMMAND_SHIP_TAG = "CommandShip";
    private const string SQUAD_TAG = "Squad";
    private const string TILE_TAG = "Tile";
    private const string COMMAND_SHIP_PREFAB = "CommandShip";
    private const string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    private readonly Vector3 CAMERA_OFFSET = new Vector3(0, 20, -13);

    private GameObject _controlledObject;
    private GameObject _commandShip;
    private Vector3 _currentCameraDistance;
    private Team _team;
    private List<Squad> _squads;
    private ResearchTree _militaryResearch;
    private ResearchTree _scientificResearch;
    private int _numResearchStations;

    public static Player Instance { get { return _instance; } }

	void Start () 
    {
        _instance = this;
        _team = Team.Union;

        _militaryResearch = new ResearchTree(5);
        _militaryResearch.AddResearch(1, new FighterResearch());
        _militaryResearch.AddResearch(2, new TransportResearch());
        _militaryResearch.AddResearch(3, new GuardSatelliteResearch());
        _militaryResearch.AddResearch(4, new HeavyFighterResearch());
        _militaryResearch.AddResearch(5, new BehemothResearch());

        _scientificResearch = new ResearchTree(5);

        // create command ship, look at it, control it
        var cmdShip = Resources.Load<GameObject>(COMMAND_SHIP_PREFAB);
        _commandShip = Instantiate(cmdShip) as GameObject;

        _controlledObject = _commandShip;
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
                if (hit.collider.tag == TILE_TAG || hit.collider.tag == COMMAND_SHIP_TAG || hit.collider.tag == SQUAD_TAG)
                {
                    _controlledObject = hit.collider.gameObject;
                    transform.position = _controlledObject.transform.position + _currentCameraDistance;
                }
            }
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
