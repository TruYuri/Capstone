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
    private GameObject _playerCommandShip;
    private Vector3 _currentCameraDistance;
    private Team _team;
    private List<Squad> _squads;

    public static Player Instance { get { return _instance; } }

	void Start () 
    {
        _instance = this;
        _team = Team.Union;

        // create command ship, look at it, control it
        var cmdShip = Resources.Load<GameObject>(COMMAND_SHIP_PREFAB);
        _playerCommandShip = Instantiate(cmdShip) as GameObject;

        _controlledObject = _playerCommandShip;
        Camera.main.transform.position = _playerCommandShip.transform.position + CAMERA_OFFSET;
        Camera.main.transform.LookAt(_playerCommandShip.transform);
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

        switch(_controlledObject.tag)
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

        var scrollChange = Input.GetAxis(MOUSE_SCROLLWHEEL);
        Camera.main.transform.position += 10.0f * scrollChange * Camera.main.transform.forward;

        if (_controlledObject != null)
            _currentCameraDistance = this.transform.position - _controlledObject.transform.position;
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

                var dir = hit.point - _playerCommandShip.transform.position;
                dir.Normalize();
                _playerCommandShip.transform.position += dir * speed * Time.deltaTime;

                _playerCommandShip.transform.position = new Vector3(_playerCommandShip.transform.position.x, 0.0f, _playerCommandShip.transform.position.z);
                _playerCommandShip.transform.LookAt(hit.point);
                transform.position = _playerCommandShip.transform.position + _currentCameraDistance;
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
