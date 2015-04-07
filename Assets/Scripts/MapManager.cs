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

    private static MapManager _instance;
    private Dictionary<string, float> _planetTypeSpawnTable;
    private Dictionary<string, float> _deploySpawnTable;
    private Dictionary<string, TextureAtlasDetails> _planetTextureTable;
    private Dictionary<string, Dictionary<Inhabitance, float>> _planetInhabitanceSpawnTable;
    private Dictionary<string, Dictionary<Resource, float>> _planetResourceSpawnTable;
    private Dictionary<string, Dictionary<string, string>> _planetSpawnDetails;
    private Texture2D _textureAtlas;
    private Dictionary<int, Dictionary<int, Sector>> _sectorMap;

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

    public void GenerateNewSectors(Vector3 realPosition, Vector2 gridPosition)
    {
        var position = realPosition;
        var v = (int)gridPosition.x;
        var h = (int)gridPosition.y;

        if (Mathf.Abs(v) % 2 == 0) // even grid row
        {
            GenerateSector(position + TOP_RIGHT_OFFSET, v + 1, h);
            GenerateSector(position + RIGHT_OFFSET, v, h + 1);
            GenerateSector(position + BOTTOM_RIGHT_OFFSET, v - 1, h);
            GenerateSector(position + BOTTOM_LEFT_OFFSET, v - 1, h - 1);
            GenerateSector(position + LEFT_OFFSET, v, h - 1);
            GenerateSector(position + TOP_LEFT_OFFSET, v + 1, h - 1);
        }
        else // odd row
        {
            GenerateSector(position + TOP_RIGHT_OFFSET, v + 1, h + 1);
            GenerateSector(position + RIGHT_OFFSET, v, h + 1);
            GenerateSector(position + BOTTOM_RIGHT_OFFSET, v - 1, h + 1);
            GenerateSector(position + BOTTOM_LEFT_OFFSET, v - 1, h);
            GenerateSector(position + LEFT_OFFSET, v, h - 1);
            GenerateSector(position + TOP_LEFT_OFFSET, v + 1, h);
        }
    }

    private void GenerateSector(Vector3 position, int vertical, int horizontal)
    {
        if (!_sectorMap.ContainsKey(vertical))
            _sectorMap.Add(vertical, new Dictionary<int, Sector>());

        if (!_sectorMap[vertical].ContainsKey(horizontal))
        {
            var sectorPrefab = Resources.Load<UnityEngine.Object>(SECTOR_PREFAB);
            var sector = Instantiate(sectorPrefab, position, Quaternion.Euler(-90f, 0, 0)) as GameObject;
            var component = sector.GetComponent<Sector>();
            component.Init(new Vector2(vertical, horizontal));
            _sectorMap[vertical].Add(horizontal, component);
        }
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
                extension = MapManager.Instance.SectorMap[x][y].GetRangeExtension(team, "Relay"); // if sector has a relay, extend range;
            return new AStarSectorNode(list, g + 1, r - 1 + extension, team, type);
        }
    }

    private KeyValuePair<int, int> SectorGridToKVP(Sector sector)
    {
        return new KeyValuePair<int, int>((int)sector.GridPosition.x, (int)sector.GridPosition.y);
    }

    public List<KeyValuePair<int, int>> AStarSearch(Sector start, Sector goal, int startRange = 0, Team team = Team.Uninhabited, string type = "")
    {
        var fringe = new List<AStarSectorNode>();
        var fringeSet = new Dictionary<KeyValuePair<int, int>, AStarSectorNode>();
        var explored = new HashSet<KeyValuePair<int, int>>();

        var startPos = SectorGridToKVP(start);
        var endpos = SectorGridToKVP(goal);

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
                return cur.sectorPath;

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