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
    private Tile[,] _tileGrid;
    private Dictionary<string, int> _planetCounts;

    public Vector2 GridPosition { get { return _gridPos; } }

    public void Init(Vector2 gridPos)
    {
        _gridPos = gridPos;
        _tileGrid = new Tile[18, 18];
        _planetCounts = new Dictionary<string, int>();
    }

	void Start () 
    {
        this.transform.parent = MapManager.Instance.transform;

        if(Tile == null)
            Tile = Resources.Load<GameObject>(TILE_PREFAB);

        // Generate center columns
        for(int i = -85, j = 0; i <= 85; i += 10, j++)
        {
            CreateTile(new KeyValuePair<int, int>(8, j), new Vector3(-5, 0, i));
            CreateTile(new KeyValuePair<int, int>(9, j), new Vector3(5, 0, i));
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
                CreateTile(new KeyValuePair<int, int>(nx1, j + 1), new Vector3(-15 - shift, 0, i));
                CreateTile(new KeyValuePair<int, int>(nx2, j + 1), new Vector3(-25 - shift, 0, i));
                CreateTile(new KeyValuePair<int, int>(px1, j + 1), new Vector3(15 + shift, 0, i));
                CreateTile(new KeyValuePair<int, int>(px2, j + 1), new Vector3(25 + shift, 0, i));
            }
        }
	}

    private void CreateTile(KeyValuePair<int, int> grid, Vector3 offset, string type = null)
    {
        var suffix = string.Empty;
        if (type == null)
        {
            var chance = (float)GameManager.Generator.NextDouble();
            foreach (var planet in MapManager.Instance.PlanetTypeSpawnTable)
            {
                if (chance <= planet.Value)
                {
                    type = planet.Key;
                    break;
                }
            }

            if (!_planetCounts.ContainsKey(type))
                _planetCounts.Add(type, 0);
            _planetCounts[type]++;

            suffix = "-"
            + Math.Abs(_gridPos.x).ToString() + Math.Abs(_gridPos.y).ToString()
            + PlanetSuffix(type, _planetCounts[type]);
        }

        // crappy way to check if it's empty space, but it works for now
        if (MapManager.Instance.PlanetTextureTable[type].Texture == null)
            return;

        var name = MapManager.Instance.PlanetSpawnDetails[type][PLANET_NAME] + suffix;
        var tileObj = Instantiate(Tile, this.transform.position + offset, Quaternion.identity) as GameObject;

        var tile = tileObj.GetComponent<Tile>();
        tile.Init(type, name, this);
        _tileGrid[grid.Key, grid.Value] = tile;
    }

    public void GenerateNewSectors()
    {
        MapManager.Instance.GenerateNewSectors(transform.position, _gridPos);
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

    // convert position to grid position, based on 10-multiple offsets
    // this should work in constant time now.
    // Note: need way to filter off-hex tiles as non-usable.
    // X:
    // 0 to -9.999 = 8, 0 to 9.999 = 9 (handle special + or - there), -10 to -19.999 = 7, ...  
    // Y:
    // Same, actually.

    private Vector2 WorldToGridReal(Vector3 point)
    {
        var diff = point - this.transform.position;
        float x = 0, y = 0;
        // round to nearest multiple of 10
        if (diff.x < 0.0f) // on the left
            x = Mathf.Ceil(diff.x / 10.0f) * 10.0f;
        else
            x = Mathf.Floor(diff.x / 10.0f) * 10.0f;

        if (diff.z < 0.0f) // on the left
            y = Mathf.Ceil(diff.z / 10.0f) * 10.0f;
        else
            y = Mathf.Floor(diff.z / 10.0f) * 10.0f;

        return new Vector2(x, y);
    }

    private KeyValuePair<int, int> WorldToGridArray(Vector3 point)
    {
        var diffreal = point - this.transform.position;
        var diff = WorldToGridReal(point);
        var x = 0;
        var y = 0;

        // convert x to array position
        if (diff.x == 0.0f)
            x = diffreal.x < 0.0f ? 8 : 9;
        else
            x = (diffreal.x < 0.0f ? 8 : 9) + (int)(diff.x / 10.0f);

        if (diff.y == 0.0f)
            y = diffreal.z < 0.0f ? 8 : 9;
        else
            y = (diffreal.z < 0.0f ? 8 : 9) + (int)(diff.y / 10.0f);

        return new KeyValuePair<int, int>((int)(x + 0.5f), (int)(y + 0.5f));
    }

    public Tile GetTileAtPosition(Vector3 point)
    {
        var pos = WorldToGridArray(point);

        if(pos.Key >= 0 && pos.Value >= 0 &&
            pos.Key <= 17 && pos.Value <= 17)
            return _tileGrid[pos.Key, pos.Value];
        return null;
    }
	
    public Tile CreateTileAtPosition(string type, Vector3 pos)
    {
        var relativePos = WorldToGridReal(pos);
        var gridPos = WorldToGridArray(relativePos);
        var realPosition = new Vector3(relativePos.x, 0f, relativePos.y)
            + new Vector3(relativePos.x >= 0 ? 5 : -5, 0.0f, relativePos.y >= 0 ? 5 : -5);

        CreateTile(gridPos, realPosition, type);
        return _tileGrid[gridPos.Key, gridPos.Value];
    }

	// Update is called once per frame
	void Update () 
    {
	}
}
