using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    private static Player _instance;

    private const string TILE_TAG = "Tile";
    private const string SECTOR_TAG = "Sector";
    private const string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";

    private Sector _currentSector;
    private Tile _currentTile;

    public static Player Instance { get { return _instance; } }

	void Start () 
    {
        _instance = this;
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == TILE_TAG)
        {
        }
        else if(other.tag == SECTOR_TAG)
        {
            var sector = other.transform.parent.GetComponent<Sector>();

            if (_currentSector == null || (sector.transform.position - this.transform.position).sqrMagnitude
                < (_currentSector.transform.position - this.transform.position).sqrMagnitude)
            {
                if (_currentSector != null)
                    _currentSector.renderer.material.color = Color.white;
                _currentSector = sector;
                _currentSector.renderer.material.color = Color.green;
                MapManager.Instance.GenerateNewSectors(_currentSector);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == SECTOR_TAG)
        {
            var sector = other.transform.parent.GetComponent<Sector>();

            if (_currentSector == null || (sector.transform.position - this.transform.position).sqrMagnitude
                < (_currentSector.transform.position - this.transform.position).sqrMagnitude)
            {
                if (_currentSector != null)
                    _currentSector.renderer.material.color = Color.white;
                _currentSector = sector;
                _currentSector.renderer.material.color = Color.green;
                MapManager.Instance.GenerateNewSectors(_currentSector);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == TILE_TAG)
        {
        }
    }
	
	
	// Update is called once per frame
	void Update () 
    {
        Camera.main.transform.LookAt(transform);
        Tile tile = _currentSector.GetTileAtPosition(transform.position);
        if(tile != null)
        {
            _currentTile = tile;
        }

        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == TILE_TAG)
                {

                }
                else
                {
                    float speed = 10.0f;

                    var dir = hit.point - transform.position;
                    dir.Normalize();
                    transform.position += dir * speed * Time.deltaTime;

                    transform.position = transform.localPosition = new Vector3(transform.position.x, 0.0f, transform.position.z);
                    transform.DetachChildren();
                    transform.LookAt(hit.point);
                    Camera.main.transform.parent = transform;
                }
            }
        }

        var scrollChange = Input.GetAxis(MOUSE_SCROLLWHEEL);
        Camera.main.transform.position += 10.0f * scrollChange * Camera.main.transform.forward;
	}
}
