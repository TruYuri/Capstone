using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable] 
public class Sector : MonoBehaviour
{
    private static UnityEngine.Object Tile;

    private const string TILE_PREFAB = "Tile";   
    private const string PLANET_NAME = "AssignedName";

    private const string PLANET_SMALL_SPAWN_DETAIL = "SmallSizeSpawnRate";
    private const string PLANET_SMALL_RESOURCE_MIN_DETAIL = "ResourceAmountSmallMinimum";
    private const string PLANET_SMALL_RESOURCE_MAX_DETAIL = "ResourceAmountSmallMaximum";
    private const string PLANET_SMALL_POPULATION_MIN_DETAIL = "PopulationAmountSmallMinimum";
    private const string PLANET_SMALL_POPULATION_MAX_DETAIL = "PopulationAmountSmallMaximum";

    private const string PLANET_LARGE_SPAWN_DETAIL = "LargeSizeSpawnRate";
    private const string PLANET_LARGE_RESOURCE_MIN_DETAIL = "ResourceAmountLargeMinimum";
    private const string PLANET_LARGE_RESOURCE_MAX_DETAIL = "ResourceAmountLargeMaximum";
    private const string PLANET_LARGE_POPULATION_MIN_DETAIL = "PopulationAmountLargeMinimum";
    private const string PLANET_LARGE_POPULATION_MAX_DETAIL = "PopulationAmountLargeMaximum";

    // Adjoining sectors
    private KeyValuePair<int, int> _gridPos;
    private Tile[,] _tileGrid = new Tile[18, 18];
    private Dictionary<string, int> _planetCounts = new Dictionary<string, int>();
    private Dictionary<Team, Dictionary<string, List<Structure>>> _deployedSpaceStructures;
    private Dictionary<Team, int> _ownershipCounts;
    private Team _owner;

    public KeyValuePair<int, int> GridPosition { get { return _gridPos; } }

    public void Init(KeyValuePair<int, int> gridPos)
    {
        _gridPos = gridPos;
        _deployedSpaceStructures = new Dictionary<Team, Dictionary<string, List<Structure>>>();
        _ownershipCounts = new Dictionary<Team, int>()
        {
            { Team.Indigenous, 0 },
            { Team.Kharkyr, 0 },
            { Team.Plinthen, 0 },
            { Team.Union, 0 }
        };
    }

    /* Old
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
     */

    void Start()
    {
        this.transform.parent = MapManager.Instance.transform;
        int count = 0;
        var MAX = 4;
        var chance = 1.0f / 244f;
        var gen = GameManager.Generator;

        if (Tile == null)
            Tile = Resources.Load<GameObject>(TILE_PREFAB);

        if (_gridPos.Key == 0 && _gridPos.Value == 0)
        {
            int x = 8 + (int)(GameManager.Generator.NextDouble() * 2);
            int y = 8 + (int)(GameManager.Generator.NextDouble() * 2);

            CreateTile(new KeyValuePair<int,int>(x, y), GridToWorld(x, y), "Terran", Resource.Forest, Inhabitance.SpaceAge, HumanPlayer.Instance.Team);
            HumanPlayer.Instance.AddShip(_tileGrid[x, y].Squad, "Base");
            HumanPlayer.Instance.AddShip(_tileGrid[x, y].Squad, "Resource Transport");
            _tileGrid[x, y].Squad.Deploy(_tileGrid[x, y].Squad.Ships[0] as Structure, _tileGrid[x, y]);
            HumanPlayer.Instance.CommandSquad.transform.position = _tileGrid[x, y].transform.position + new Vector3(-5f, 0, 0);
            HumanPlayer.Instance.ClaimTile(_tileGrid[x, y]);

            while(!IsValidLocation(new KeyValuePair<int, int>(x, y)) || _tileGrid[x,y] != null)
            {
                x = (int)(GameManager.Generator.NextDouble() * 18);
                y = (int)(GameManager.Generator.NextDouble() * 18);
            }

            CreateTile(new KeyValuePair<int,int>(x, y), GridToWorld(x, y), null, Resource.Oil, Inhabitance.SpaceAge, HumanPlayer.Instance.Team);
            HumanPlayer.Instance.AddShip(_tileGrid[x, y].Squad, "Gathering Complex");
            HumanPlayer.Instance.AddShip(_tileGrid[x, y].Squad, "Resource Transport");
            _tileGrid[x, y].Squad.Deploy(_tileGrid[x, y].Squad.Ships[0] as Structure, _tileGrid[x, y]);
            HumanPlayer.Instance.ClaimTile(_tileGrid[x, y]);

            while(!IsValidLocation(new KeyValuePair<int, int>(x, y)) || _tileGrid[x,y] != null)
            {
                x = (int)(GameManager.Generator.NextDouble() * 18);
                y = (int)(GameManager.Generator.NextDouble() * 18);
            }

            CreateTile(new KeyValuePair<int, int>(x, y), GridToWorld(x, y), null, Resource.Ore, Inhabitance.SpaceAge, HumanPlayer.Instance.Team);
            HumanPlayer.Instance.AddShip(_tileGrid[x, y].Squad, "Gathering Complex");
            HumanPlayer.Instance.AddShip(_tileGrid[x, y].Squad, "Resource Transport");
            _tileGrid[x, y].Squad.Deploy(_tileGrid[x, y].Squad.Ships[0] as Structure, _tileGrid[x, y]);
            HumanPlayer.Instance.ClaimTile(_tileGrid[x, y]);
        }
        else // regular sector generation
        {
            // Generate center columns
            for (int i = -85, j = 0; i <= 85; i += 10, j++)
            {
                if (gen.NextDouble() < chance && count < MAX) { CreateTile(new KeyValuePair<int, int>(8, j), new Vector3(-5, 0, i)); count++; }
                if (gen.NextDouble() < chance && count < MAX) { CreateTile(new KeyValuePair<int, int>(9, j), new Vector3(5, 0, i)); count++; }
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
                    if (gen.NextDouble() < chance && count < MAX) { CreateTile(new KeyValuePair<int, int>(nx1, j + 1), new Vector3(-15 - shift, 0, i)); count++; }
                    if (gen.NextDouble() < chance && count < MAX) { CreateTile(new KeyValuePair<int, int>(nx2, j + 1), new Vector3(-25 - shift, 0, i)); count++; }
                    if (gen.NextDouble() < chance && count < MAX) { CreateTile(new KeyValuePair<int, int>(px1, j + 1), new Vector3(15 + shift, 0, i)); count++; }
                    if (gen.NextDouble() < chance && count < MAX) { CreateTile(new KeyValuePair<int, int>(px2, j + 1), new Vector3(25 + shift, 0, i)); count++; }
                }
            }
        }
    }

    void Update()
    {
    }

    private void CreateTile(KeyValuePair<int, int> grid, Vector3 offset, string type = null,
        Resource rType = Resource.NoResource, Inhabitance pType = Inhabitance.Uninhabited, Team team = Team.Uninhabited)
    {
        var suffix = string.Empty;
        var mm = MapManager.Instance;
        var population = 0;
        var rCount = 0;
        var size = TileSize.Small;

        // generate resource type
        var chance = (float)GameManager.Generator.NextDouble();
        var rates = mm.ResourceRates.Keys.OrderBy(r => mm.ResourceRates[r]).ToList();

        if (rType == Resource.NoResource)
        {
            if (!(type != null && MapManager.Instance.DeploySpawnTable.ContainsKey(type)))
            {
                foreach (var r in rates)
                {
                    if (chance <= mm.ResourceRates[r])
                    {
                        rType = r;
                        break;
                    }
                }
            }
        }

        if (type == null)
        {
            // generate planet type from resource type
            chance = (float)GameManager.Generator.NextDouble();
            var step = 1.0f / mm.ResourcePlanetTypes[rType].Count();
            int i = 0;
            while (chance > (i + 1) * step)
                i++;
            type = mm.ResourcePlanetTypes[rType][i];
        }

        if (pType == Inhabitance.Uninhabited)
        {
            // generate population type
            chance = (float)GameManager.Generator.NextDouble();
            var pt = mm.PlanetInhabitanceSpawnTable[type].Keys.OrderBy(p => mm.PlanetInhabitanceSpawnTable[type][p]).ToList();
            foreach (var t in pt)
            {
                if (chance <= mm.PlanetInhabitanceSpawnTable[type][t])
                {
                    pType = t;
                    break;
                }
            }

            if (pType != Inhabitance.Uninhabited)
                team = Team.Indigenous;
        }

        if(!MapManager.Instance.DeploySpawnTable.ContainsKey(type))
        {
            // generate large/small
            chance = (float)GameManager.Generator.NextDouble();
            if (chance < float.Parse(mm.PlanetSpawnDetails[type][PLANET_SMALL_SPAWN_DETAIL]))
            {
                size = TileSize.Small;

                // generate resource and population amount
                int minimum, maximum;
                if (pType != Inhabitance.Uninhabited)
                {
                    minimum = int.Parse(mm.PlanetSpawnDetails[type][PLANET_SMALL_POPULATION_MIN_DETAIL]);
                    maximum = int.Parse(mm.PlanetSpawnDetails[type][PLANET_SMALL_POPULATION_MAX_DETAIL]);
                    population = GameManager.Generator.Next(minimum, maximum + 1);
                }

                minimum = int.Parse(mm.PlanetSpawnDetails[type][PLANET_SMALL_RESOURCE_MIN_DETAIL]);
                maximum = int.Parse(mm.PlanetSpawnDetails[type][PLANET_SMALL_RESOURCE_MAX_DETAIL]);
                rCount = GameManager.Generator.Next(minimum, maximum + 1);
            }
            else
            {
                size = TileSize.Large;

                // generate resource and population amount
                int minimum, maximum;
                if (pType != Inhabitance.Uninhabited)
                {
                    minimum = int.Parse(mm.PlanetSpawnDetails[type][PLANET_LARGE_POPULATION_MIN_DETAIL]);
                    maximum = int.Parse(mm.PlanetSpawnDetails[type][PLANET_LARGE_POPULATION_MAX_DETAIL]);
                    population = GameManager.Generator.Next(minimum, maximum + 1);
                }

                minimum = int.Parse(mm.PlanetSpawnDetails[type][PLANET_LARGE_RESOURCE_MIN_DETAIL]);
                maximum = int.Parse(mm.PlanetSpawnDetails[type][PLANET_LARGE_RESOURCE_MAX_DETAIL]);
                rCount = GameManager.Generator.Next(minimum, maximum + 1);
            }

            if (!_planetCounts.ContainsKey(type))
                _planetCounts.Add(type, 0);
            _planetCounts[type]++;

            suffix = "-"
            + Math.Abs(_gridPos.Key).ToString() + Math.Abs(_gridPos.Value).ToString()
            + PlanetSuffix(type, _planetCounts[type]);
        }

        if (MapManager.Instance.PlanetTextureTable[type].Texture == null)
            return;

        var name = MapManager.Instance.PlanetSpawnDetails[type][PLANET_NAME] + suffix;
        var tileObj = Instantiate(Tile, this.transform.position + offset, Quaternion.identity) as GameObject;

        var tile = tileObj.GetComponent<Tile>();
        tile.Init(this, type, name, pType, population, rType, rCount, size, team);
        _tileGrid[grid.Key, grid.Value] = tile;
    }

    // Old tile generation - keeping because it's beautiful
    /*
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
            + Math.Abs(_gridPos.Key).ToString() + Math.Abs(_gridPos.Value).ToString()
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
    } */

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

        if (_gridPos.Key >= 0 && _gridPos.Value >= 0)
            val += "-q1";
        else if (_gridPos.Key < 0 && _gridPos.Value >= 0)
            val += "-q2";
        else if (_gridPos.Key < 0 && _gridPos.Value < 0)
            val += "-q3";
        else
            val += "-q4";

        return val;
    }

    public void RegisterSpaceStructure(Team team, Structure structure)
    {
        if (!_deployedSpaceStructures.ContainsKey(team))
            _deployedSpaceStructures.Add(team, new Dictionary<string, List<Structure>>());

        if (!_deployedSpaceStructures[team].ContainsKey(structure.Name))
            _deployedSpaceStructures[team].Add(structure.Name, new List<Structure>());

        _deployedSpaceStructures[team][structure.Name].Add(structure);
        _deployedSpaceStructures[team][structure.Name].Sort(delegate(Structure a, Structure b)
        {
            if (a.Range == b.Range) return 0;
            if (a.Range < b.Range) return -1;
            return 1;
        });
    }

    public List<Structure> GetSpaceStructures(Team team, string type)
    {
        if(_deployedSpaceStructures.ContainsKey(team) && _deployedSpaceStructures[team].ContainsKey(type))
            return _deployedSpaceStructures[team][type];
        return null;
    }

    public void UnregisterSpaceStructure(Team team, Structure structure)
    {
        _deployedSpaceStructures[team][structure.Name].Remove(structure);
    }

    public int GetBestRangeExtension(Team team, string type)
    {
        if(_deployedSpaceStructures.ContainsKey(team) && _deployedSpaceStructures[team].ContainsKey(type) && _deployedSpaceStructures[team][type].Count > 0)
        {
            return _deployedSpaceStructures[team][type][0].Range;
        }

        return 0;
    }
    // convert position to grid position, based on 10-multiple offsets
    // this should work in constant time now.
    // Note: need way to filter off-hex tiles as non-usable.
    // X:
    // 0 to -9.999 = 8, 0 to 9.999 = 9 (handle special + or - there), -10 to -19.999 = 7, ...  
    // Y:
    // Same, actually.

    public bool IsValidLocation(Vector3 pos)
    {
        var gridPos = WorldToGridArray(pos);

        return IsValidLocation(gridPos);
    }

    public bool IsValidLocation(KeyValuePair<int, int> gridPos)
    {
        var x = gridPos.Key;
        if (gridPos.Key > 9)
            x = 17 - gridPos.Key;

        var offset = (18 - (10 + (x / 2) * 2)) / 2;
        if (gridPos.Value < 18 - offset && gridPos.Value > -1 + offset)
            return true;

        return false;
    }

    private Vector3 GridToWorld(int x, int y)
    {
        return GridToWorld(new KeyValuePair<int, int>(x, y));
    }

    private Vector3 GridToWorld(KeyValuePair<int, int> pos)
    {
        var x = pos.Key;
        var y = pos.Value;

        float fx, fy;
        if (x <= 8)
            fx = -5f - 10f * (8 - x);
        else
            fx = 5f + 10f * (x - 9);

        if (y <= 8)
            fy = -5f - 10f * (8 - y);
        else
            fy = 5f + 10f * (y - 9);

        return new Vector3(fx, 0f, fy);
    }

    private KeyValuePair<int, int> RealWorldToGrid(Vector3 point)
    {
        var diff = point - this.transform.position;
        int x = 0, y = 0;
        // round to nearest multiple of 10
        if (diff.x < 0.0f) // on the left
            x = Mathf.CeilToInt(diff.x / 10.0f) * 10;
        else
            x = Mathf.FloorToInt(diff.x / 10.0f) * 10;

        if (diff.z < 0.0f) // on the left
            y = Mathf.CeilToInt(diff.z / 10.0f) * 10;
        else
            y = Mathf.FloorToInt(diff.z / 10.0f) * 10;

        return new KeyValuePair<int, int>(x, y);
    }

    private KeyValuePair<int, int> WorldToGridArray(Vector3 point)
    {
        var diffreal = point - this.transform.position;
        var diff = RealWorldToGrid(point);
        var x = 0;
        var y = 0;

        // convert x to array position
        if (diff.Key == 0)
            x = diffreal.x < 0f ? 8 : 9;
        else
            x = (diffreal.x < 0f ? 8 : 9) + (int)(diff.Key / 10.0f);

        if (diff.Value == 0)
            y = diffreal.z < 0f ? 8 : 9;
        else
            y = (diffreal.z < 0f ? 8 : 9) + (int)(diff.Value / 10.0f);

        return new KeyValuePair<int, int>((int)(x + 0.5f), (int)(y + 0.5f));
    }

    public Tile GetTileAtPosition(Vector3 point)
    {
        var pos = WorldToGridArray(point);

        if (pos.Key >= 0 && pos.Value >= 0 &&
            pos.Key <= 17 && pos.Value <= 17)
        {
            return _tileGrid[pos.Key, pos.Value];
        }
        return null;
    }
	
    public Tile CreateTileAtPosition(string type, Vector3 pos)
    {
        var fixedPos = pos - this.transform.position;
        var relativePos = RealWorldToGrid(pos);
        var gridPos = WorldToGridArray(pos);
        var realPosition = new Vector3(relativePos.Key, 0f, relativePos.Value)
            + new Vector3(fixedPos.x >= 0 ? 5 : -5, 0.0f, fixedPos.z >= 0 ? 5 : -5);

        CreateTile(gridPos, realPosition, type);
        return _tileGrid[gridPos.Key, gridPos.Value];
    }

    public void DeleteTile(Tile tile)
    {
        var grid = WorldToGridArray(tile.transform.position);
        _tileGrid[grid.Key, grid.Value] = null;
        GameObject.Destroy(tile);
    }

    public void ClaimTile(Team team)
    {
        _ownershipCounts[team]++;
        DetermineOwner();
        GetComponent<Renderer>().material.SetColor("_Color", GameManager.Instance.PlayerColors[_owner]);
    }

    public void RelinquishTile(Team team)
    {
        _ownershipCounts[team]--;
        DetermineOwner();
        GetComponent<Renderer>().material.SetColor("_Color", GameManager.Instance.PlayerColors[_owner]);
    }

    private void DetermineOwner()
    {
        int n = 0;
        foreach (var t in _ownershipCounts)
        {
            if (t.Value > n)
            {
                _owner = t.Key;
                n = t.Value;
            }
        }
    }

    public Team GetOwner()
    {
        Team owner = Team.Uninhabited;

        if (!HumanPlayer.Instance.ExploredSectors.ContainsKey(this))
            return owner;

        return _owner;
    }
}
