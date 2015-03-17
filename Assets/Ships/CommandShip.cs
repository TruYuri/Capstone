using UnityEngine;
using System.Collections;

public class CommandShip : Squad
{
    private const string TILE_TAG = "Tile";
    private const string SECTOR_TAG = "SectorCollider";
    private Sector _currentSector;
    private Tile _currentTile;
    private Ship _ship;

    public Ship Ship 
    {
        get { return _ship; }
        set { _ship = value; }
    }

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
            CheckSector(other.transform.parent.GetComponent<Sector>());
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == SECTOR_TAG)
        {
            CheckSector(other.transform.parent.GetComponent<Sector>());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == TILE_TAG)
        {
        }
    }

    private void CheckSector(Sector sector)
    {
        if (_currentSector == null || (sector.transform.position - this.transform.position).sqrMagnitude
            < (_currentSector.transform.position - this.transform.position).sqrMagnitude)
        {
            if (_currentSector != null)
                _currentSector.GetComponent<Renderer>().material.color = Color.white;
            _currentSector = sector;
            _currentSector.GetComponent<Renderer>().material.color = Color.green;
            MapManager.Instance.GenerateNewSectors(_currentSector);
            GameManager.Instance.EndTurn();
        }
    }
}
