using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable] 
public class Sector : MonoBehaviour
{
    // Adjoining sectors
    public GameObject TopRight { get; set; }
    public GameObject Right { get; set; }
    public GameObject BottomRight { get; set; }
    public GameObject BottomLeft { get; set; }
    public GameObject Left { get; set; }
    public GameObject TopLeft { get; set; }

    private List<GameObject> Tiles;

	// Use this for initialization

    // note: ONLY RUN THIS ONCE, AT LOAD.
    // copy the first generated sector and just change values - faster
	void Start () 
    {
        this.transform.parent = MapManager.Instance.transform;
   
        if (MapManager.Sector != this.gameObject)
        {
            return;
        }

        // this is our first sector, so generate it. all other sectors will just copy it
        MapManager.Sector = this.gameObject;       
        Tiles = new List<GameObject>();
        var baseTile = MapManager.Tile;

        GameObject tile;

        // Generate center columns
        for(int i = -85; i <= 85; i += 10)
        {
            Vector3 position = this.transform.position + new Vector3(-5, 0, i);
            tile = Instantiate(baseTile, position, Quaternion.identity) as GameObject;
            tile.transform.parent = this.transform;
            Tiles.Add(tile);

            position = this.transform.position + new Vector3(5, 0, i);
            tile = Instantiate(baseTile, position, Quaternion.identity) as GameObject;
            tile.transform.parent = this.transform;
            Tiles.Add(tile);
        }

        // Generate middle columns
        for (int n = 0; n < 4; n++)
        {
            int shift = 2 * n * 10;
            for (int i = -75 + n * 10; i <= 75 - n * 10; i += 10)
            {
                Vector3 position = this.transform.position + new Vector3(-15 - shift, 0, i);
                tile = Instantiate(baseTile, position, Quaternion.identity) as GameObject;
                tile.transform.parent = this.transform;
                Tiles.Add(tile);

                position = this.transform.position + new Vector3(-25 - shift, 0, i);
                tile = Instantiate(baseTile, position, Quaternion.identity) as GameObject;
                tile.transform.parent = this.transform;
                Tiles.Add(tile);

                position = this.transform.position + new Vector3(15 + shift, 0, i);
                tile = Instantiate(baseTile, position, Quaternion.identity) as GameObject;
                tile.transform.parent = this.transform;
                Tiles.Add(tile);

                position = this.transform.position + new Vector3(25 + shift, 0, i);
                tile = Instantiate(baseTile, position, Quaternion.identity) as GameObject;
                tile.transform.parent = this.transform;
                Tiles.Add(tile);
            }
        }

        MapManager.Instance.GenerateNewSectors(this);
	}

    public Tile GetTileAtPosition(Vector3 point)
    {
        foreach(var tile in Tiles)
        {
            var tileComp = tile.GetComponent<Tile>();
            if(tileComp.Bounds.Contains(point))
            {
                return tileComp;
            }
        }

        return null;
    }
	
	// Update is called once per frame
	void Update () 
    {
	}
}
