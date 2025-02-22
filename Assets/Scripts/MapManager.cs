﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages map generation, including the map of sectors and minimap generation.
/// </summary>
public class MapManager : MonoBehaviour
{
    private readonly Vector3 TOP_RIGHT_OFFSET = new Vector3(93.1f, 0.0f, 140.6f);
    private readonly Vector3 RIGHT_OFFSET = new Vector3(186.2f, 0, 0.0f);
    private readonly Vector3 BOTTOM_RIGHT_OFFSET = new Vector3(93.1f, 0.0f, -140.6f);
    private readonly Vector3 BOTTOM_LEFT_OFFSET = new Vector3(-93.1f, 0.0f, -140.6f);
    private readonly Vector3 LEFT_OFFSET = new Vector3(-186.2f, 0, 0);
    private readonly Vector3 TOP_LEFT_OFFSET = new Vector3(-93.1f, 0.0f, 140.6f);
    private const string SECTOR_PREFAB = "Sector";
    private const string INI_PATH = "/Resources/Planets.ini";
    private const string MATERIALS_PATH = "PlanetTextures/";
    private const string PLANET_SECTION_HEADER = "[Planet Spawn Rates]";
    private const string DEPLOYABLE_SECTION_HEADER = "[Deployable Planets]";
    private const string PLANET_TEXTURE_DETAIL = "SpriteName";
    private const string PLANET_UNINHABITED_DETAIL = "Uninhabited";
    private const string PLANET_PRIMITIVE_DETAIL = "InhabitedPrimitive";
    private const string PLANET_INDUSTRIAL_DETAIL = "InhabitedIndustrial";
    private const string PLANET_SPACEAGE_DETAIL = "InhabitedSpaceAge";
    private const string PLANET_FOREST_DETAIL = "Forest";
    private const string PLANET_ORE_DETAIL = "Ore";
    private const string PLANET_OIL_DETAIL = "Oil";
    private const string PLANET_ASTERMINIUM_DETAIL = "Asterminium";
    private const string MINIMAP_TILE_TEXTURE = "minimaptile";
    private const string MINIMAP_HIGHLIGHT = "minimapHighlight";
    private const string RESOURCE_RATES = "[Resource Type Spawn Rates]";

    private static MapManager _instance;
    private Color[] _minimapTile;
    private Color[] _minimapHighlight;
    private Dictionary<string, float> _planetTypeSpawnTable;
    private Dictionary<string, float> _deploySpawnTable;
    private Dictionary<string, TextureAtlasDetails> _planetTextureTable;
    private Dictionary<string, Dictionary<Inhabitance, float>> _planetInhabitanceSpawnTable;
    private Dictionary<string, Dictionary<Resource, float>> _planetResourceSpawnTable;
    private Dictionary<string, Dictionary<string, string>> _planetSpawnDetails;
    private Dictionary<Resource, List<string>> _resourcePlanetTypes;
    private Dictionary<Resource, float> _resourceRate;
    private Texture2D _textureAtlas;
    private Dictionary<int, Dictionary<int, Sector>> _sectorMap;

    private KeyValuePair<int, int> _minMapSectors;
    private KeyValuePair<int, int> _maxMapSectors;
    private Texture2D _minimap;

    public static MapManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<MapManager>();
            }
            return _instance;
        }
    }

    public Dictionary<string, float> PlanetTypeSpawnTable { get { return _planetTypeSpawnTable; } }
    public Dictionary<string, float> DeploySpawnTable { get { return _deploySpawnTable; } }
    public Dictionary<string, TextureAtlasDetails> PlanetTextureTable { get { return _planetTextureTable; } }
    public Dictionary<string, Dictionary<Inhabitance, float>> PlanetInhabitanceSpawnTable { get { return _planetInhabitanceSpawnTable; } }
    public Dictionary<string, Dictionary<Resource, float>> PlanetResourceSpawnTable { get { return _planetResourceSpawnTable; } }
    public Dictionary<string, Dictionary<string, string>> PlanetSpawnDetails { get { return _planetSpawnDetails; } }
    public Dictionary<int, Dictionary<int, Sector>> SectorMap { get { return _sectorMap; } }
    public Dictionary<Resource, List<string>> ResourcePlanetTypes { get { return _resourcePlanetTypes; } } // new
    public Dictionary<Resource, float> ResourceRates { get { return _resourceRate; } } // new
    public Texture2D Minimap { get { return _minimap; } }

    /// <summary>
    /// Initializes the MapManager, reading in values from Planets.ini and generating the first sector.
    /// </summary>
    public void Init()
    {
        _instance = this;
        _planetTypeSpawnTable = new Dictionary<string, float>();
        _deploySpawnTable = new Dictionary<string, float>();
        _planetTextureTable = new Dictionary<string, TextureAtlasDetails>();
        _planetInhabitanceSpawnTable = new Dictionary<string, Dictionary<Inhabitance, float>>();
        _planetResourceSpawnTable = new Dictionary<string, Dictionary<Resource, float>>();
        _planetSpawnDetails = new Dictionary<string, Dictionary<string, string>>();
        _sectorMap = new Dictionary<int, Dictionary<int, Sector>>();
        _planetSpawnDetails = new Dictionary<string, Dictionary<string, string>>();
        _resourceRate = new Dictionary<Resource, float>();
        _minimap = new Texture2D(0, 0);

        _resourcePlanetTypes = new Dictionary<Resource, List<string>>()
        {
            { Resource.NoResource, new List<string>() },
            { Resource.Oil, new List<string>() },
            { Resource.Ore, new List<string>() },
            { Resource.Asterminium, new List<string>() },
            { Resource.Forest, new List<string>() },
            { Resource.Stations, new List<string>() },
        };

        _minMapSectors = new KeyValuePair<int, int>();
        _maxMapSectors = new KeyValuePair<int, int>();
        var tex = Resources.Load<Texture2D>(MINIMAP_TILE_TEXTURE);
        _minimapTile = tex.GetPixels();
        tex = Resources.Load<Texture2D>(MINIMAP_HIGHLIGHT);
        _minimapHighlight = tex.GetPixels();

        var parser = new INIParser(Application.dataPath + INI_PATH);
        var spawnTables = parser.ParseINI();
        var textures = new Texture2D[spawnTables.Count]; // to ensure correct order
        var planetNames = new string[spawnTables.Count]; // to ensure correct order
        var planetCount = 0;
        var runningTotal = 0.0f;

        // cache planet spawn probabilities
        foreach (var planet in spawnTables[PLANET_SECTION_HEADER])
        {
            // Generate upper limit in 0.0 - 1.0 spectrum for planet
            runningTotal += float.Parse(planet.Value);
            _planetTypeSpawnTable.Add(planet.Key, runningTotal);
        }
        foreach (var deploy in spawnTables[DEPLOYABLE_SECTION_HEADER])
        {
            _deploySpawnTable.Add(deploy.Key, 0);
        }
        spawnTables.Remove(PLANET_SECTION_HEADER);
        spawnTables.Remove(DEPLOYABLE_SECTION_HEADER);

        /* new */
        float t = 0f;
        _resourceRate.Add(Resource.Asterminium, t = float.Parse(spawnTables[RESOURCE_RATES][PLANET_ASTERMINIUM_DETAIL]));
        _resourceRate.Add(Resource.Oil, t += float.Parse(spawnTables[RESOURCE_RATES][PLANET_OIL_DETAIL]));
        _resourceRate.Add(Resource.Ore, t += float.Parse(spawnTables[RESOURCE_RATES][PLANET_ORE_DETAIL]));
        _resourceRate.Add(Resource.Forest, t += float.Parse(spawnTables[RESOURCE_RATES][PLANET_FOREST_DETAIL]));
        spawnTables.Remove(RESOURCE_RATES);

        foreach (var planet in spawnTables)
        {
            var key = planet.Key.TrimStart('[');
            key = key.TrimEnd(']');
            _planetInhabitanceSpawnTable.Add(key, new Dictionary<Inhabitance, float>());
            _planetResourceSpawnTable.Add(key, new Dictionary<Resource, float>());

            // load texture for atlasing
            textures[planetCount] = Resources.Load<Texture2D>(MATERIALS_PATH + spawnTables[planet.Key][PLANET_TEXTURE_DETAIL]);
            planetNames[planetCount++] = key;

            // cache per-planet Inhabitance probabilities
            _planetInhabitanceSpawnTable[key].Add(Inhabitance.Uninhabited, runningTotal = float.Parse(spawnTables[planet.Key][PLANET_UNINHABITED_DETAIL]));
            _planetInhabitanceSpawnTable[key].Add(Inhabitance.Primitive, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_PRIMITIVE_DETAIL]));
            _planetInhabitanceSpawnTable[key].Add(Inhabitance.Industrial, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_INDUSTRIAL_DETAIL]));
            _planetInhabitanceSpawnTable[key].Add(Inhabitance.SpaceAge, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_SPACEAGE_DETAIL]));

            /* old method - still used, but need to separate
            // cache per-planet Resource probabilities
            _planetResourceSpawnTable[key].Add(Resource.Forest, runningTotal = float.Parse(spawnTables[planet.Key][PLANET_FOREST_DETAIL]));
            _planetResourceSpawnTable[key].Add(Resource.Ore, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_ORE_DETAIL]));
            _planetResourceSpawnTable[key].Add(Resource.Oil, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_OIL_DETAIL]));
            _planetResourceSpawnTable[key].Add(Resource.Asterminium, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_ASTERMINIUM_DETAIL]));
            */

            // new
            var val = float.Parse(spawnTables[planet.Key][PLANET_FOREST_DETAIL]);
            runningTotal = val;
            _planetResourceSpawnTable[key].Add(Resource.Forest, runningTotal);
            if (val > 0f) _resourcePlanetTypes[Resource.Forest].Add(key);

            val = float.Parse(spawnTables[planet.Key][PLANET_ORE_DETAIL]);
            runningTotal += val;
            _planetResourceSpawnTable[key].Add(Resource.Ore, runningTotal);
            if (val > 0f) _resourcePlanetTypes[Resource.Ore].Add(key);

            val = float.Parse(spawnTables[planet.Key][PLANET_OIL_DETAIL]);
            runningTotal += val;
            _planetResourceSpawnTable[key].Add(Resource.Oil, runningTotal);
            if (val > 0f) _resourcePlanetTypes[Resource.Oil].Add(key);

            val = float.Parse(spawnTables[planet.Key][PLANET_ASTERMINIUM_DETAIL]);
            runningTotal += val;
            _planetResourceSpawnTable[key].Add(Resource.Asterminium, runningTotal);
            if (val > 0f) _resourcePlanetTypes[Resource.Asterminium].Add(key);
            // end new

            // remove used data from the table
            spawnTables[planet.Key].Remove(PLANET_UNINHABITED_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_PRIMITIVE_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_INDUSTRIAL_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_SPACEAGE_DETAIL);

            spawnTables[planet.Key].Remove(PLANET_FOREST_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_ORE_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_OIL_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_ASTERMINIUM_DETAIL);

            _planetSpawnDetails.Add(key, spawnTables[planet.Key]);
        }

        _textureAtlas = new Texture2D(0, 0);
        var atlasEntries = _textureAtlas.PackTextures(textures, 0);

        for (int i = 0; i < planetCount; i++)
        {
            _planetTextureTable.Add(planetNames[i],
                new TextureAtlasDetails((atlasEntries[i].width == 0 && atlasEntries[i].height == 0 ? null : _textureAtlas),
                                        new Vector2(atlasEntries[i].x, atlasEntries[i].y),
                                        new Vector2(atlasEntries[i].width, atlasEntries[i].height)));
            spawnTables["[" + planetNames[i] + "]"].Remove(PLANET_TEXTURE_DETAIL);
        }

        parser.CloseINI();
        GenerateSector(Vector3.zero, 0, 0);
    }

    /// <summary>
    /// Generates new sectors around the specific one, if they don't exist yet.
    /// </summary>
    /// <param name="realPosition">The position to generated around in real coords.</param>
    /// <param name="gridPosition">The position to generated around in grid coords.</param>
    public void GenerateNewSectors(Vector3 realPosition, KeyValuePair<int, int> gridPosition)
    {
        var position = realPosition;
        var v = gridPosition.Key;
        var h = gridPosition.Value;
        var gen = 0;
        if (Mathf.Abs(v) % 2 == 0) // even grid row
        {
            gen += GenerateSector(position + TOP_RIGHT_OFFSET, v + 1, h);
            gen += GenerateSector(position + RIGHT_OFFSET, v, h + 1);
            gen += GenerateSector(position + BOTTOM_RIGHT_OFFSET, v - 1, h);
            gen += GenerateSector(position + BOTTOM_LEFT_OFFSET, v - 1, h - 1);
            gen += GenerateSector(position + LEFT_OFFSET, v, h - 1);
            gen += GenerateSector(position + TOP_LEFT_OFFSET, v + 1, h - 1);
        }
        else // odd row
        {
            gen += GenerateSector(position + TOP_RIGHT_OFFSET, v + 1, h + 1);
            gen += GenerateSector(position + RIGHT_OFFSET, v, h + 1);
            gen += GenerateSector(position + BOTTOM_RIGHT_OFFSET, v - 1, h + 1);
            gen += GenerateSector(position + BOTTOM_LEFT_OFFSET, v - 1, h);
            gen += GenerateSector(position + LEFT_OFFSET, v, h - 1);
            gen += GenerateSector(position + TOP_LEFT_OFFSET, v + 1, h);
        }
    }

    /// <summary>
    /// Finds the sector closest to the specified sector and position.
    /// </summary>
    /// <param name="sector">The current sector.</param>
    /// <param name="detail">The real coordinates to find closest to.</param>
    /// <returns>The closest sector.</returns>
    public Sector FindNearestSector(Sector sector, Vector3 detail)
    {
        var v = sector.GridPosition.Key;
        var h = sector.GridPosition.Value;

        Sector closest = null;
        float mgn = 0f;
        List<Sector> secs = new List<Sector>();

        if (Mathf.Abs(v) % 2 == 0) // even grid row
        {
            secs.Add(_sectorMap[v + 1][h]);
            secs.Add(_sectorMap[v][h + 1]);
            secs.Add(_sectorMap[v - 1][h]);
            secs.Add(_sectorMap[v - 1][h - 1]);
            secs.Add(_sectorMap[v][h - 1]);
            secs.Add(_sectorMap[v + 1][h - 1]);
        }
        else // odd row
        {
            secs.Add(_sectorMap[v + 1][h + 1]);
            secs.Add(_sectorMap[v][h + 1]);
            secs.Add(_sectorMap[v - 1][h + 1]);
            secs.Add(_sectorMap[v - 1][h]);
            secs.Add(_sectorMap[v][h - 1]);
            secs.Add(_sectorMap[v + 1][h]);
        }

        foreach(var sec in secs)
        {
            var dist = (detail - sec.transform.position).sqrMagnitude;
            if(closest == null || dist < mgn)
            {
                closest = sec;
                mgn = dist;
            }
        }

        return closest;
    }

    /// <summary>
    /// Generates the minimap center of a tile in minimap coordinates.
    /// </summary>
    /// <param name="centerX">The texture's center x.</param>
    /// <param name="centerY">The texture's center y.</param>
    /// <param name="v">The sector map vertical coord.</param>
    /// <param name="h">The sector map horizontal coord.</param>
    /// <param name="even">Indicates if the sector is originally on an even row, before minimap adjustments.</param>
    /// <returns>Center of the tile in minimap coordinates.</returns>
    private KeyValuePair<int, int> GenerateCenter(int centerX, int centerY, int v, int h, bool even)
    {
        // offset stuff
        var ev = Mathf.Abs(v) % 2 == 0;
        var paddingY = (v < 0 ? 1 : -1) * 15 * Math.Abs(v) + (even ? 0 : 32);
        var paddingX = (h < 0 ? 1 : -1) * 6 * Math.Abs(h) - (even ? 0 : 32);
        if (!ev)
            paddingX -= 3;

        var xoffset = ev ? 0 : 32;
        var yoffset = v * 64;
        var posX = centerX + h * 64 + xoffset +paddingX;
        var posY = centerY + yoffset +paddingY;

        return new KeyValuePair<int, int>(posX, posY);
    }

    /// <summary>
    /// Generates a minimap at the specified center with specified color highlights.
    /// </summary>
    /// <param name="center">The sector at minimap center.</param>
    /// <param name="highlights">Sectors that should be highlighted with a specific color (not team ownership!)</param>
    /// <returns>The new minimap.</returns>
    public Texture2D GenerateMap(Sector center, Dictionary<Sector, Color> highlights)
    {
        var width = 256 * 5;
        var height = 192 * 5;

        if (highlights == null)
            highlights = new Dictionary<Sector, Color>();

        DestroyImmediate(_minimap);
        _minimap = new Texture2D(width, height);
        var centerX = _minimap.width / 2;
        var centerY = _minimap.height / 2;
        var color = _minimapTile[32 * 64 + 32];

        var bv = center.GridPosition.Key;
        var bh = center.GridPosition.Value;

        var even = bv % 2 == 0;
        bv += even ? 0 : 1;
        for (int v = bv - 6; v < bv + 6; v++ )
        {
            for(int h = bh - 10; h < bh + 10; h++)
            {
                // check if this falls outside of the minimap boundaries

                Sector sec = null;
                if (_sectorMap.ContainsKey(v) && _sectorMap[v].ContainsKey(h))
                    sec = _sectorMap[v][h];
                else
                    continue;

                var centerp = GenerateCenter(centerX, centerY, v - bv, h - bh, even);
                var posX = centerp.Key - 32;
                var posY = centerp.Value - 32;

                var pixels = _minimapTile;
                var c = GameManager.Instance.PlayerColors[sec.GetOwner()];

                for (int y = 0; y < 64; y++)
                {
                    var yn = y * 64;
                    for (int x = 0; x < 64; x++)
                    {
                        if (pixels[yn + x] == color)
                            _minimap.SetPixel(posX + x, posY + y, c);
                    }
                }

                if (highlights.ContainsKey(sec))
                {
                    c = highlights[sec];
                    pixels = _minimapHighlight;

                    for (int y = 0; y < 64; y++)
                    {
                        var yn = y * 64;
                        for (int x = 0; x < 64; x++)
                        {
                            if (pixels[yn + x] == color)
                                _minimap.SetPixel(posX + x, posY + y, c);
                        }
                    }
                }
            }
        }

        // grab rectangle so the texture doesn't shrink/etc
        _minimap.Apply();
        return _minimap;
    }

    /// <summary>
    /// Creates a new sector.
    /// </summary>
    /// <param name="position">The real world coords of the sector.</param>
    /// <param name="vertical">The vertical grid position.</param>
    /// <param name="horizontal">The horizontal grid position.</param>
    /// <returns>Integer indicating the number of sectors generated.</returns>
    private int GenerateSector(Vector3 position, int vertical, int horizontal)
    {
        if (!_sectorMap.ContainsKey(vertical))
            _sectorMap.Add(vertical, new Dictionary<int, Sector>());

        if (!_sectorMap[vertical].ContainsKey(horizontal))
        {
            var sectorPrefab = Resources.Load<UnityEngine.Object>(SECTOR_PREFAB);
            var sector = Instantiate(sectorPrefab, position, Quaternion.Euler(-90f, 0, 0)) as GameObject;
            var component = sector.GetComponent<Sector>();
            component.Init(new KeyValuePair<int, int>(vertical, horizontal));
            _sectorMap[vertical].Add(horizontal, component);

            if (vertical < _minMapSectors.Key)
                _minMapSectors = new KeyValuePair<int, int>(vertical, _minMapSectors.Value);
            if (horizontal < _minMapSectors.Value)
                _minMapSectors = new KeyValuePair<int,int>(_minMapSectors.Key, horizontal);
            if (vertical > _maxMapSectors.Key)
                _maxMapSectors = new KeyValuePair<int, int>(vertical, _maxMapSectors.Value);
            if (horizontal > _maxMapSectors.Value)
                _maxMapSectors = new KeyValuePair<int, int>(_maxMapSectors.Key, horizontal);

            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Finds neighboring sectors.
    /// </summary>
    /// <param name="sector">The sector to get neighbors of.</param>
    /// <returns>A list of neighbor sectors.</returns>
    public List<Sector> GetNeighbors(Sector sector)
    {
        var n = new List<Sector>();
        var v = sector.GridPosition.Key;
        var h = sector.GridPosition.Value;

        if (Math.Abs(v) % 2 == 0)
        {
            if (_sectorMap.ContainsKey(v + 1) && _sectorMap[v + 1].ContainsKey(h)) n.Add(_sectorMap[v + 1][h]);
            if (_sectorMap.ContainsKey(v) && _sectorMap[v].ContainsKey(h + 1)) n.Add(_sectorMap[v][h + 1]);
            if (_sectorMap.ContainsKey(v - 1) && _sectorMap[v - 1].ContainsKey(h)) n.Add(_sectorMap[v - 1][h]);
            if (_sectorMap.ContainsKey(v - 1) && _sectorMap[v - 1].ContainsKey(h - 1)) n.Add(_sectorMap[v - 1][h - 1]);
            if (_sectorMap.ContainsKey(v) && _sectorMap[v].ContainsKey(h - 1)) n.Add(_sectorMap[v][h - 1]);
            if (_sectorMap.ContainsKey(v + 1) && _sectorMap[v + 1].ContainsKey(h - 1)) n.Add(_sectorMap[v + 1][h - 1]);
        }
        else
        {
            if (_sectorMap.ContainsKey(v + 1) && _sectorMap[v + 1].ContainsKey(h + 1)) n.Add(_sectorMap[v + 1][h + 1]);
            if (_sectorMap.ContainsKey(v) && _sectorMap[v].ContainsKey(h + 1)) n.Add(_sectorMap[v][h + 1]);
            if (_sectorMap.ContainsKey(v - 1) && _sectorMap[v - 1].ContainsKey(h + 1)) n.Add(_sectorMap[v - 1][h + 1]);
            if (_sectorMap.ContainsKey(v - 1) && _sectorMap[v - 1].ContainsKey(h)) n.Add(_sectorMap[v - 1][h]);
            if (_sectorMap.ContainsKey(v) && _sectorMap[v].ContainsKey(h - 1)) n.Add(_sectorMap[v][h - 1]);
            if (_sectorMap.ContainsKey(v + 1) && _sectorMap[v + 1].ContainsKey(h)) n.Add(_sectorMap[v + 1][h]);
        }

        return n;
    }

    /// <summary>
    /// Finds warp portals around a start portal and sector.
    /// </summary>
    /// <param name="team">The team the portals belong to.</param>
    /// <param name="portal">The starting portal.</param>
    /// <param name="start">The starting sector.</param>
    /// <returns>Table mapping portals to their sectors.</returns>
    public Dictionary<Structure, Sector> FindPortals(Team team, Structure portal, Sector start)
    {
        var sectors = FindWarpsRecursive(team, start.GridPosition.Key, start.GridPosition.Value, portal.Range);

        var gates = new Dictionary<Structure, Sector>();
        foreach(var p in sectors)
        {
            var ps = p.GetSpaceStructures(team, "Warp Portal");
            foreach(var g in ps)
            {
                gates.Add(g, p);
            }
        }
        return gates;
    }

    /// <summary>
    /// Recursively search neighboring sectors for warp portals.
    /// </summary>
    /// <param name="team">The portals' team.</param>
    /// <param name="v">The sector vertical position.</param>
    /// <param name="h">The sector horizontal position. </param>
    /// <param name="range">Remaining range from the starting portal.</param>
    /// <returns>A table of found portals.</returns>
    private HashSet<Sector> FindWarpsRecursive(Team team, int v, int h, int range)
    {
        HashSet<Sector> portals = new HashSet<Sector>();

        if (range < 0)
            return portals;

        if (_sectorMap.ContainsKey(v) && _sectorMap[v].ContainsKey(h))
        {
            var ports = _sectorMap[v][h].GetSpaceStructures(team, "Warp Portal");
            if(ports != null && ports.Count > 0)
                portals.Add(_sectorMap[v][h]);
        }

        if(Math.Abs(v) % 2 == 0)
        {
            portals.UnionWith(FindWarpsRecursive(team, v + 1, h, range - 1));
            portals.UnionWith(FindWarpsRecursive(team, v, h + 1, range - 1));
            portals.UnionWith(FindWarpsRecursive(team, v - 1, h, range - 1));
            portals.UnionWith(FindWarpsRecursive(team, v - 1, h - 1, range - 1));
            portals.UnionWith(FindWarpsRecursive(team, v, h - 1, range - 1));
            portals.UnionWith(FindWarpsRecursive(team, v + 1, h - 1, range - 1));
        }
        else
        {
            portals.UnionWith(FindWarpsRecursive(team, v + 1, h + 1, range - 1));
            portals.UnionWith(FindWarpsRecursive(team, v, h + 1, range - 1));
            portals.UnionWith(FindWarpsRecursive(team, v - 1, h + 1, range - 1));
            portals.UnionWith(FindWarpsRecursive(team, v - 1, h, range - 1));
            portals.UnionWith(FindWarpsRecursive(team, v, h - 1, range - 1));
            portals.UnionWith(FindWarpsRecursive(team, v + 1, h, range - 1));
        }

        return portals;
    }

    /// <summary>
    /// A node class for searching sectors paths through A*. A node is a combination of the current path and specified search parameters.
    /// </summary>
    private class AStarSectorNode
    {
        public List<KeyValuePair<int, int>> sectorPath;
        public int g;
        public int h;
        public int r;
        public string type;
        public Team team;

        /// <summary>
        /// Constructor for the A* node class.
        /// </summary>
        /// <param name="path">The sector path this node contains.</param>
        /// <param name="gf">g(x) cost function</param>
        /// <param name="startRange">Range remaining from the start, if applicable.</param>
        /// <param name="team">The team to restrict the search to.</param>
        /// <param name="type">The object type to search for.</param>
        public AStarSectorNode(List<KeyValuePair<int, int>> path, int gf, int startRange, Team team, string type)
        {
            this.sectorPath = path;
            this.g = gf;
            this.h = path.Count; // heuristic = size of path.
            this.r = startRange;
            this.team = team;
            this.type = type;
            // alternative: h = sum of sqrt dist from goal
        }

        /// <summary>
        /// Generate successor nodes.
        /// </summary>
        /// <returns>A list of successor nodes.</returns>
        public List<AStarSectorNode> succ()
        {
            List<AStarSectorNode> children = new List<AStarSectorNode>();

            if (type != "" && r - 1 < 0) // don't consider any more nodes if they are out of range
                return children;

            var v = sectorPath[sectorPath.Count - 1].Key;
            var h = sectorPath[sectorPath.Count - 1].Value;
            if (Mathf.Abs(v) % 2 == 0)
            {
                children.Add(New(v + 1, h));
                children.Add(New(v, h + 1));
                children.Add(New(v - 1, h));
                children.Add(New(v - 1, h - 1));
                children.Add(New(v, h - 1));
                children.Add(New(v + 1, h - 1));
            }
            else
            {
                children.Add(New(v + 1, h + 1));
                children.Add(New(v, h + 1));
                children.Add(New(v - 1, h + 1));
                children.Add(New(v - 1, h));
                children.Add(New(v, h - 1));
                children.Add(New(v + 1, h));
            }

            children.RemoveAll(s => s == null);
            return children;
        }

        /// <summary>
        /// Create a new successor node.
        /// </summary>
        /// <param name="x">Sector map vertical position.</param>
        /// <param name="y">Sector map horizontal position.</param>
        /// <returns></returns>
        private AStarSectorNode New(int x, int y)
        {
            var list = new List<KeyValuePair<int, int>>(sectorPath);
            list.Add(new KeyValuePair<int, int>(x, y));
            var sm = MapManager.Instance.SectorMap;

            if (sm.ContainsKey(x) && sm[x].ContainsKey(y))
            {
                var extension = 0;
                if (type != "")
                    extension = sm[x][y].GetBestRangeExtension(team, "Relay"); // if sector has a relay, extend range;

                return new AStarSectorNode(list, g + 1, r - 1 + extension, team, type);
            }

            return null;
        }
    }

    /// <summary>
    /// A* search for general pathfinding and command/relay ranges.
    /// </summary>
    /// <param name="start">The starting sector.</param>
    /// <param name="goal">The goal sector.</param>
    /// <param name="startRange">The initial range, if applicable.</param>
    /// <param name="team">The team to restrict to, if applicable.</param>
    /// <param name="type">The type of object to search for.</param>
    /// <returns>A working path between start and goal, or null if none exists.</returns>
    public List<Sector> AStarSearch(Sector start, Sector goal, int startRange = 0, Team team = Team.Uninhabited, string type = "")
    {
        var fringe = new List<AStarSectorNode>();
        var fringeSet = new Dictionary<KeyValuePair<int, int>, AStarSectorNode>();
        var explored = new HashSet<KeyValuePair<int, int>>();

        var startPos = start.GridPosition;
        var endpos = goal.GridPosition;

        var initList = new List<KeyValuePair<int, int>>();
        initList.Add(startPos);

        AStarSectorNode init = new AStarSectorNode(initList, 0, startRange, team, type);
        fringe.Add(init);
        fringeSet.Add(startPos, init);

        while (fringe.Count > 0)
        {
            AStarSectorNode cur = fringe[0];

            var last = cur.sectorPath[cur.sectorPath.Count - 1];
            if (last.Key == endpos.Key && last.Value == endpos.Value)
            {
                var sectors = new List<Sector>();
                for (int i = 0; i < cur.sectorPath.Count; i++)
                    sectors.Add(_sectorMap[cur.sectorPath[i].Key][cur.sectorPath[i].Value]);
                return sectors;
            }

            fringe.RemoveAt(0);
            fringeSet.Remove(last);
            explored.Add(last);

            List<AStarSectorNode> exp = cur.succ();

            for (int i = 0; i < exp.Count; i++) // 6 possible movements
            {
                last = exp[i].sectorPath[exp[i].sectorPath.Count - 1];
                if (explored.Contains(last))
                    continue;

                bool inFringe = fringeSet.ContainsKey(last);
                if (!inFringe)
                {
                    fringe.Add(exp[i]);
                    fringe = fringe.OrderBy(o => o.h + o.g).ToList();
                    fringeSet.Add(last, exp[i]);
                }
                else
                {
                    var val = fringeSet[last];

                    if (exp[i].g + exp[i].h < val.g + val.h)
                    {
                        val.g = exp[i].g;
                        val.h = exp[i].h;
                        val.sectorPath = exp[i].sectorPath;
                    }
                }
            }
        }

        return null;
    }
}