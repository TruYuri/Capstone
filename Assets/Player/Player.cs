using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
	// Use this for initialization
    Sector currentSector;
    Tile currentTile;

	void Start () 
    {
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Tile")
        {
        }
        else if(other.tag == "Sector")
        {
            var sector = other.transform.parent.GetComponent<Sector>();

            if (currentSector == null || (sector.transform.position - this.transform.position).sqrMagnitude
                < (currentSector.transform.position - this.transform.position).sqrMagnitude)
            {
                currentSector = sector;
                MapManager.Instance.GenerateNewSectors(currentSector);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Sector")
        {
            var sector = other.transform.parent.GetComponent<Sector>();

            if (currentSector == null || (sector.transform.position - this.transform.position).sqrMagnitude
                < (currentSector.transform.position - this.transform.position).sqrMagnitude)
            {
                currentSector = sector;
                MapManager.Instance.GenerateNewSectors(currentSector);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Tile")
        {
        }
    }
	
	
	// Update is called once per frame
	void Update () 
    {
        var tile = currentSector.GetTileAtPosition(transform.position);
        if(tile != null)
        {
            currentTile = tile;
        }

        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                float speed = 50.0f;
                transform.position = Vector3.MoveTowards(transform.position, hit.point, Time.deltaTime * speed);
                transform.position = transform.localPosition = new Vector3(transform.position.x, 0.0f, transform.position.z);
            }
        }

        var scrollChange = Input.GetAxis("Mouse ScrollWheel");
	}
}
