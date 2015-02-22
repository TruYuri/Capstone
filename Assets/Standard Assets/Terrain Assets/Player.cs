using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
	// Use this for initialization
    Sector currentSector;
    Tile currentTile;
    public static Player Instance;

	void Start () 
    {
        Instance = this;
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Tile")
        {
        }
        else if(other.tag == "Sector")
        {
            var sector = other.transform.parent.GetComponent<Sector>();

            if (currentSector == null)
                currentSector = sector;
            else if((sector.transform.position - this.transform.position).sqrMagnitude
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
        Tile tile = null;// = currentSector.GetTileAtPosition(transform.position);
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

                var dir = hit.point - transform.position;
                dir.Normalize();
                transform.position += dir * speed * Time.deltaTime;

                transform.position = transform.localPosition = new Vector3(transform.position.x, 0.0f, transform.position.z);
                transform.DetachChildren();
                transform.LookAt(hit.point);
                Camera.main.transform.parent = transform;
            }
        }

        var scrollChange = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.transform.position += 100.0f * scrollChange * Camera.main.transform.forward;
	}
}
