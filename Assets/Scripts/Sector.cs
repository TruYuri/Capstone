using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable] 
public class Sector : MonoBehaviour
{
    private static UnityEngine.Object Tile;

    private const string TILE_PREFAB = "Tile";   
    private const string PLANET_NAME = "AssignedName";

    // Adjoining sectors
    private Vector2 _gridPos;
    private List<Tile> _tiles;
    private Tile[,] _tileGrid;

    public Vector2 GridPosition { get { return _gridPos; } }
    public Tile[,] TileGrid { get { return _tileGrid; } }

	// Use this for initialization

    // note: ONLY RUN THIS ONCE, AT LOAD.
    // copy the first generated sector and just change values - faster
    public void Init(Vector2 gridPos)
    {
        _gridPos = gridPos;
    }

	void Start () 
    {
        this.transform.parent = MapManager.Instance.transform;
        _tiles = new List<Tile>();

        if(Tile == null)
            Tile = Resources.Load<GameObject>(TILE_PREFAB);

        _tileGrid = new Tile[18,18];
        var planetCounts = new Dictionary<string, int>();

        // Generate center columns
        for(int i = -85, j = 0; i <= 85; i += 10, j++)
        {
            CreateTile(new Vector2(8, j), new Vector3(-5, 0, i), planetCounts);
            CreateTile(new Vector2(9, j), new Vector3(5, 0, i), planetCounts);
        }

        // Generate middle columns
        for (int n = 0; n < 4; n++)
        {
            int shift = 2 * n * 10;
            int nx1 = 8 - n * 2 - 1;
            int px1 = 9 + n * 2 + 1;
            int nx2 = 8 - n * 2 - 2;
            int px2 = 9 + n * 2 + 2;

            for (int i = -75 + n * 10, j = n; i <= 75 - n * 10; i += 10, j++)
            {
                CreateTile(new Vector2(nx1, j + 1), new Vector3(-15 - shift, 0, i), planetCounts);
                CreateTile(new Vector2(nx2, j + 1), new Vector3(-25 - shift, 0, i), planetCounts);
                CreateTile(new Vector2(px1, j + 1), new Vector3(15 + shift, 0, i), planetCounts);
                CreateTile(new Vector2(px2, j + 1), new Vector3(25 + shift, 0, i), planetCounts);
            }
        }

        return;
	}

    private void CreateTile(Vector2 grid, Vector3 offset, Dictionary<string, int> planetCounts)
    {
        var tileObj = Instantiate(Tile, this.transform.position + offset, Quaternion.identity) as GameObject;
        this.transform.SetParent(this.transform);

        var chance = (float)GameManager.Generator.NextDouble();
        var planetType = string.Empty;
        foreach (var planet in MapManager.Instance.PlanetTypeSpawnTable)
        {
            if (chance <= planet.Value)
            {
                planetType = planet.Key;
                break;
            }
        }

        if (!planetCounts.ContainsKey(planetType))
            planetCounts.Add(planetType, 0);

        var name = MapManager.Instance.PlanetSpawnDetails[planetType][PLANET_NAME]
            + "-"
            + Math.Abs(_gridPos.x).ToString() + Math.Abs(_gridPos.y).ToString()
            + PlanetSuffix(planetType, planetCounts[planetType]);

        planetCounts[planetType]++;

        var tile = tileObj.GetComponent<Tile>();
        tile.Init(planetType, name);
        _tileGrid[(int)grid.x, (int)grid.y] = tile;
        _tiles.Add(tile);
    }
  
    private string PlanetSuffix(string type, int count)
    {
        string val = string.Empty;

        if(count > 26)
        {
            var n = count / 26;
            count -= 26 * n;
        }

        val += (char)('a' + count);

        if (_gridPos.y >= 0 && _gridPos.x >= 0)
            val += "-q1";
        else if (_gridPos.y < 0 && _gridPos.x >= 0)
            val += "-q2";
        else if (_gridPos.y < 0 && _gridPos.x < 0)
            val += "-q3";
        else
            val += "-q4";

        return val;
    }

    // convert position to grid position, based on 5-multiple offsets
    // this should work in constant time now.
    // Note: need way to filter off-hex tiles as non-usable.
    // X:
    // 0 to -9.999 = 8, 0 to 9.999 = 9 (handle special + or - there), -10 to -19.999 = 7, ...  
    // Y:
    // 
    public Tile GetTileAtPosition(Vector3 point)
    {
        var diff = point - this.transform.position;
        float x = 0, y = 0;
        // round to nearest multiple of 5
        if(diff.x < 0.0f) // on the left
            x = Mathf.Ceil(diff.x / 10.0f) * 10.0f;
        else
            x = Mathf.Floor(diff.x / 10.0f) * 10.0f;

        //if(diff.z > 0.0f) // 
        //else // on the right
            //y = Mathf.Floor(diff.z / 10.0f) * 10.0f;

        var arrayx = 0;
        var arrayy = 0;

        // convert x to array position
        if(x == 0.0f)
            arrayx = diff.x < 0.0f ? 8 : 9;
        else
            arrayx = (diff.x < 0.0f ? 8 : 9) + (int)(x / 10.0f);

        // convert y to array position
        
        foreach(var tile in _tiles)
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
