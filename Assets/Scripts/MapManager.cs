using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

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

    private static MapManager _instance;
    private Texture2D _minimapTile;
    private Dictionary<string, float> _planetTypeSpawnTable;
    private Dictionary<string, float> _deploySpawnTable;
    private Dictionary<string, TextureAtlasDetails> _planetTextureTable;
    private Dictionary<string, Dictionary<Inhabitance, float>> _planetInhabitanceSpawnTable;
    private Dictionary<string, Dictionary<Resource, float>> _planetResourceSpawnTable;
    private Dictionary<string, Dictionary<string, string>> _planetSpawnDetails;
    private Texture2D _textureAtlas;
    private Dictionary<int, Dictionary<int, Sector>> _sectorMap;
    private KeyValuePair<int, int> minMapSectors;
    private KeyValuePair<int, int> maxMapSectors;

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

    void Awake()
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
        minMapSectors = new KeyValuePair<int, int>();
        maxMapSectors = new KeyValuePair<int, int>();
        _minimapTile = Resources.Load<Texture2D>(MINIMAP_TILE_TEXTURE);

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

            // cache per-planet Resource probabilities
            _planetResourceSpawnTable[key].Add(Resource.Forest, runningTotal = float.Parse(spawnTables[planet.Key][PLANET_FOREST_DETAIL]));
            _planetResourceSpawnTable[key].Add(Resource.Ore, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_ORE_DETAIL]));
            _planetResourceSpawnTable[key].Add(Resource.Oil, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_OIL_DETAIL]));
            _planetResourceSpawnTable[key].Add(Resource.Asterminium, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_ASTERMINIUM_DETAIL]));

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

    // Use this for initialization
    public void Start()
    {
    }

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

        if (gen == 0)
            return;

        var minimap = GenerateMap(_sectorMap, new Dictionary<Sector,Color>());
        GUIManager.Instance.UpdateMinimap(minimap);
    }

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

    private KeyValuePair<int, int> GenerateCenter(int centerX, int centerY, int v, int h)
    {
        // offset stuff
        var even = Mathf.Abs(v) % 2 == 0;
        var paddingY = (v < 0 ? 1 : -1) * 15 * Math.Abs(v);
        var paddingX = (h < 0 ? 1 : -1) * 6 * Math.Abs(h);
        if (!even)
            paddingX -= 3;

        var xoffset = even ? 0 : 32;
        var yoffset = v * 64;
        var posX = centerX + h * 64 + xoffset + paddingX;
        var posY = centerY + yoffset + paddingY;

        return new KeyValuePair<int, int>(posX, posY);
    }

    public Texture2D GenerateMap(Dictionary<int, Dictionary<int, Sector>> map, Dictionary<Sector, Color> colors)
    {
        // update minimap
        // generate map so it'll always be able to center it on minimap.
        var width = Math.Abs(maxMapSectors.Value - minMapSectors.Value + 1) * 64 + 64;
        var height = Math.Abs(maxMapSectors.Key - minMapSectors.Key + 1) * 64 + 64;

        if (colors == null)
            colors = new Dictionary<Sector, Color>();

        // find nearest 192x256 multiple

        var miniMap = new Texture2D(width, height);
        miniMap.alphaIsTransparency = true;
        var centerX = miniMap.width / 2;
        var centerY = miniMap.height / 2;
        var pixels = _minimapTile.GetPixels();
        var color = pixels[32 * 64 + 32];
        foreach (var vertical in _sectorMap)
        {
            foreach (var horizontal in vertical.Value)
            {
                var center = GenerateCenter(centerX, centerY, vertical.Key, horizontal.Key);
                var posX = center.Key - 32;
                var posY = center.Value - 32;

                var c = color;
                if (colors.ContainsKey(horizontal.Value))
                    c = colors[horizontal.Value];

                // this is a major performance hog, need to redo
                for (int y = 0; y < 64; y++)
                {
                    var yn = y * 64;
                    for (int x = 0; x < 64; x++)
                    {
                        if (pixels[yn + x] == color)
                            miniMap.SetPixel(posX + x, posY + y, c);
                    }
                }
            }
        }
        // grab rectangle so the texture doesn't shrink/etc
        miniMap.Apply();
        return miniMap;
    }

    // return relative coordinates
    public Vector2 GetMiniMapPosition(Texture2D map, Sector center, Vector3 pos)
    {
        // center of map is center tile, so...
        int v = center.GridPosition.Key;
        int h = center.GridPosition.Value;
        var centerX = map.width / 2;
        var centerY = map.height / 2;

        var c = GenerateCenter(centerX, centerY, v, h);

        // positional math here

        return new Vector2(c.Key / (float)map.width, c.Value / (float)map.height);
    }

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

            if (vertical < minMapSectors.Key)
                minMapSectors = new KeyValuePair<int, int>(vertical, minMapSectors.Value);
            if (horizontal < minMapSectors.Value)
                minMapSectors = new KeyValuePair<int,int>(minMapSectors.Key, horizontal);
            if (vertical > maxMapSectors.Key)
                maxMapSectors = new KeyValuePair<int, int>(vertical, maxMapSectors.Value);
            if (horizontal > maxMapSectors.Value)
                maxMapSectors = new KeyValuePair<int, int>(maxMapSectors.Key, horizontal);

            return 1;
        }

        return 0;
    }

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

    private class AStarSectorNode
    {
        public List<KeyValuePair<int, int>> sectorPath;
        public int g;
        public int h;
        public int r;
        public string type;
        public Team team;

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

            return children;
        }

        private AStarSectorNode New(int x, int y)
        {
            var list = new List<KeyValuePair<int, int>>(sectorPath);
            list.Add(new KeyValuePair<int, int>(x, y));

            var extension = 0;
            if (type != "" && MapManager.Instance.SectorMap.ContainsKey(x) && MapManager.Instance.SectorMap[x].ContainsKey(y))
                extension = MapManager.Instance.SectorMap[x][y].GetBestRangeExtension(team, "Relay"); // if sector has a relay, extend range;
            return new AStarSectorNode(list, g + 1, r - 1 + extension, team, type);
        }
    }

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