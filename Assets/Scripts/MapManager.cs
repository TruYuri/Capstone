using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct TextureAtlasDetails
{
    public Texture2D Texture;
    public Vector2 TextureOffset;
    public Vector2 TextureScale;

    public TextureAtlasDetails(Texture2D texture, Vector2 offset, Vector2 scale)
    {
        Texture = texture;
        TextureOffset = offset;
        TextureScale = scale;
    }
}

public class MapManager : MonoBehaviour
{
    private readonly Vector3 TOP_RIGHT_OFFSET = new Vector3(98.3f, 0.0f, 149.0f);
    private readonly Vector3 RIGHT_OFFSET = new Vector3(196.5f, 0, 0.0f);
    private readonly Vector3 BOTTOM_RIGHT_OFFSET = new Vector3(98.3f, 0.0f, -149.0f);
    private readonly Vector3 BOTTOM_LEFT_OFFSET = new Vector3(-98.3f, 0.0f, -149.0f);
    private readonly Vector3 LEFT_OFFSET = new Vector3(-196.5f, 0, 0);
    private readonly Vector3 TOP_LEFT_OFFSET = new Vector3(-98.3f, 0.0f, 149.0f);
    private const string SECTOR_PREFAB = "Sector";
    private const string INI_PATH = "/Resources/Planets.ini";
    private const string MATERIALS_PATH = "PlanetTextures/";
    private const string PLANET_SECTION_HEADER = "[PlanetSpawnRates]";
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
    private Object _sectorPrefab;
    private Dictionary<string, float> _planetTypeSpawnTable;
    private Dictionary<string, TextureAtlasDetails> _planetTextureTable;
    private Dictionary<string, Dictionary<Inhabitance, float>> _planetInhabitanceSpawnTable;
    private Dictionary<string, Dictionary<Resource, float>> _planetResourceSpawnTable;
    private Dictionary<string, Dictionary<string, string>> _planetSpawnDetails;
    private Texture2D _textureAtlas;
    private Rect[] _atlasEntries;
    private Dictionary<int, Dictionary<int, GameObject>> _sectorMap;

    public static MapManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<MapManager>();
            return _instance;
        }
    }

    public Dictionary<string, float> PlanetTypeSpawnTable { get { return _planetTypeSpawnTable; } }
    public Dictionary<string, TextureAtlasDetails> PlanetTextureTable { get { return _planetTextureTable; } }
    public Dictionary<string, Dictionary<Inhabitance, float>> PlanetInhabitanceSpawnTable { get { return _planetInhabitanceSpawnTable; } }
    public Dictionary<string, Dictionary<Resource, float>> PlanetResourceSpawnTable { get { return _planetResourceSpawnTable; } }
    public Dictionary<string, Dictionary<string, string>> PlanetSpawnDetails { get { return _planetSpawnDetails; } }

	// Use this for initialization
	public void Start()
    {
        _sectorPrefab = Resources.Load<GameObject>(SECTOR_PREFAB);
        _planetTypeSpawnTable = new Dictionary<string, float>();
        _planetTextureTable = new Dictionary<string, TextureAtlasDetails>();
        _planetInhabitanceSpawnTable = new Dictionary<string, Dictionary<Inhabitance, float>>();
        _planetResourceSpawnTable = new Dictionary<string, Dictionary<Resource, float>>();
        _planetSpawnDetails = new Dictionary<string, Dictionary<string, string>>();
        _sectorMap = new Dictionary<int, Dictionary<int, GameObject>>();

        INIParser parser = new INIParser(Application.dataPath + INI_PATH);
        var spawnTables = parser.ParseINI();

        float runningTotal = 0.0f;

        // cache planet spawn probabilities
        foreach (var planet in spawnTables[PLANET_SECTION_HEADER])
        {
            // Generate upper limit in 0.0 - 1.0 spectrum for planet
            runningTotal += float.Parse(planet.Value);
            _planetTypeSpawnTable.Add('[' + planet.Key + ']', runningTotal);
        }
        spawnTables.Remove(PLANET_SECTION_HEADER);

        // using these to guarantee same order in the texture atlas. Dictionaries don't guarantee order.
        Texture2D[] textures = new Texture2D[spawnTables.Count];
        string[] planetNames = new string[spawnTables.Count];

        int planetCount = 0;
        foreach(var planet in spawnTables)
        {
            _planetInhabitanceSpawnTable.Add(planet.Key, new Dictionary<Inhabitance, float>());
            _planetResourceSpawnTable.Add(planet.Key, new Dictionary<Resource, float>());

            // load texture for atlasing
            var tex = Resources.Load<Texture2D>(MATERIALS_PATH + spawnTables[planet.Key][PLANET_TEXTURE_DETAIL]);
            textures[planetCount] = tex;
            planetNames[planetCount++] = planet.Key; 

            // cache per-planet Inhabitance probabilities
            _planetInhabitanceSpawnTable[planet.Key].Add(Inhabitance.Uninhabited, runningTotal = float.Parse(spawnTables[planet.Key][PLANET_UNINHABITED_DETAIL]));
            _planetInhabitanceSpawnTable[planet.Key].Add(Inhabitance.Primitive, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_PRIMITIVE_DETAIL]));
            _planetInhabitanceSpawnTable[planet.Key].Add(Inhabitance.Industrial, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_INDUSTRIAL_DETAIL]));
            _planetInhabitanceSpawnTable[planet.Key].Add(Inhabitance.SpaceAge, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_SPACEAGE_DETAIL]));

            // cache per-planet Resource probabilities
            _planetResourceSpawnTable[planet.Key].Add(Resource.Forest, runningTotal = float.Parse(spawnTables[planet.Key][PLANET_FOREST_DETAIL]));
            _planetResourceSpawnTable[planet.Key].Add(Resource.Ore, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_ORE_DETAIL]));
            _planetResourceSpawnTable[planet.Key].Add(Resource.Oil, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_OIL_DETAIL]));
            _planetResourceSpawnTable[planet.Key].Add(Resource.Asterminium, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_ASTERMINIUM_DETAIL]));

            // remove used data from the table
            spawnTables[planet.Key].Remove(PLANET_UNINHABITED_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_PRIMITIVE_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_INDUSTRIAL_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_SPACEAGE_DETAIL);

            spawnTables[planet.Key].Remove(PLANET_FOREST_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_ORE_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_OIL_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_ASTERMINIUM_DETAIL);
        }

        _textureAtlas = new Texture2D(spawnTables.Count * 256, 256);
        _atlasEntries = _textureAtlas.PackTextures(textures, 0);

        for (int i = 0; i < planetCount; i++)
        {
            _planetTextureTable.Add(planetNames[i], 
                new TextureAtlasDetails((_atlasEntries[i].width == 0 && _atlasEntries[i].height == 0 ? null : _textureAtlas), 
                                        new Vector2(_atlasEntries[i].x, _atlasEntries[i].y),
                                        new Vector2(_atlasEntries[i].width, _atlasEntries[i].height)));
            spawnTables[planetNames[i]].Remove(PLANET_TEXTURE_DETAIL);
        }

        // Store the remaining misc. data
        _planetSpawnDetails = spawnTables;

        parser.CloseINI();

        // class init should set grid spots to zero.
        var sector = Instantiate(_sectorPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        _sectorMap.Add(0, new Dictionary<int, GameObject>());
        _sectorMap[0].Add(0, sector);

        _instance = this;
	}

    public void GenerateNewSectors(Sector origin)
    {
        var position = origin.transform.position;
        var v = origin.VerticalGridPosition;
        var h = origin.HorizontalGridPosition;

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
            _sectorMap.Add(vertical, new Dictionary<int, GameObject>());

        if(!_sectorMap[vertical].ContainsKey(horizontal))
        {
            var sector = Instantiate(_sectorPrefab, position, Quaternion.identity) as GameObject;
            var component = sector.GetComponent<Sector>();
            component.VerticalGridPosition = vertical;
            component.HorizontalGridPosition = horizontal;
            _sectorMap[vertical].Add(horizontal, sector);
        }
    }
}
