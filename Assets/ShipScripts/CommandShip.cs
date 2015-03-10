using UnityEngine;
using System.Collections;

public class CommandShip : MonoBehaviour
{
    private const string TILE_TAG = "Tile";
    private const string SECTOR_TAG = "Sector";
    private Sector _currentSector;
    private Tile _currentTile;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (_currentSector != null)
        {
            Tile tile = _currentSector.GetTileAtPosition(transform.position);
            if (tile != null)
            {
                _currentTile = tile;
            }
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == TILE_TAG)
        {
        }
        else if (other.tag == SECTOR_TAG)
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
}
