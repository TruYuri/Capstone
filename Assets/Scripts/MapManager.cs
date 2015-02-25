using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private const string MATERIALS_PATH = "PlanetMaterials/";
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
    private List<GameObject> _sectors;
    private Dictionary<string, float> _planetTypeSpawnTable;
    private Dictionary<string, Material> _planetTextureTable;
    private Dictionary<string, Dictionary<Inhabitance, float>> _planetInhabitanceSpawnTable;
    private Dictionary<string, Dictionary<Resource, float>> _planetResourceSpawnTable;
    private Dictionary<string, Dictionary<string, string>> _planetSpawnDetails;

    public static MapManager Instance { get { return _instance; } }
    public Dictionary<string, float> PlanetTypeSpawnTable { get { return _planetTypeSpawnTable; } }
    public Dictionary<string, Material> PlanetTextureTable { get { return _planetTextureTable; } }
    public Dictionary<string, Dictionary<Inhabitance, float>> PlanetInhabitanceSpawnTable { get { return _planetInhabitanceSpawnTable; } }
    public Dictionary<string, Dictionary<Resource, float>> PlanetResourceSpawnTable { get { return _planetResourceSpawnTable; } }
    public Dictionary<string, Dictionary<string, string>> PlanetSpawnDetails { get { return _planetSpawnDetails; } }

	// Use this for initialization
	public void Start()
    {
        _sectors = new List<GameObject>();
        _sectorPrefab = Resources.Load<GameObject>(SECTOR_PREFAB);
        _planetTypeSpawnTable = new Dictionary<string, float>();
        _planetTextureTable = new Dictionary<string, Material>();
        _planetInhabitanceSpawnTable = new Dictionary<string, Dictionary<Inhabitance, float>>();
        _planetResourceSpawnTable = new Dictionary<string, Dictionary<Resource, float>>();
        _planetSpawnDetails = new Dictionary<string, Dictionary<string, string>>();

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

        foreach(var planet in spawnTables)
        {
            _planetInhabitanceSpawnTable.Add(planet.Key, new Dictionary<Inhabitance, float>());
            _planetResourceSpawnTable.Add(planet.Key, new Dictionary<Resource, float>());

            // cache texture
            var path = MATERIALS_PATH + spawnTables[planet.Key][PLANET_TEXTURE_DETAIL];
            _planetTextureTable.Add(planet.Key, Resources.Load<Material>(path));

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
            spawnTables[planet.Key].Remove(PLANET_TEXTURE_DETAIL);

            spawnTables[planet.Key].Remove(PLANET_UNINHABITED_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_PRIMITIVE_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_INDUSTRIAL_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_SPACEAGE_DETAIL);

            spawnTables[planet.Key].Remove(PLANET_FOREST_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_ORE_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_OIL_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_ASTERMINIUM_DETAIL);
        }

        // Store the remaining misc. data
        _planetSpawnDetails = spawnTables;

        parser.CloseINI();

        _sectors.Add(Instantiate(_sectorPrefab, Vector3.zero, Quaternion.identity) as GameObject);
        _instance = this;
	}

    public void GenerateNewSectors(Sector origin)
    {
        var position = Vector3.zero;

        // Generate any needed immediate neighbors and link them
        if (origin.TopRight == null)
        {
            position = origin.transform.position + TOP_RIGHT_OFFSET;

            origin.TopRight = Instantiate(_sectorPrefab, position, Quaternion.identity) as GameObject;
            origin.TopRight.GetComponent<Sector>().BottomLeft = origin.gameObject;

            _sectors.Add(origin.TopRight);
        }

        if (origin.Right == null)
        {
            position = origin.transform.position + RIGHT_OFFSET;

            origin.Right = Instantiate(_sectorPrefab, position, Quaternion.identity) as GameObject;
            origin.Right.GetComponent<Sector>().Left = origin.gameObject;

            _sectors.Add(origin.Right);
        }

        if (origin.BottomRight == null)
        {
            position = origin.transform.position + BOTTOM_RIGHT_OFFSET;

            origin.BottomRight = Instantiate(_sectorPrefab, position, Quaternion.identity) as GameObject;
            origin.BottomRight.GetComponent<Sector>().TopLeft = origin.gameObject;

            _sectors.Add(origin.BottomRight);
        }

        if (origin.BottomLeft == null)
        {
            position = origin.transform.position + BOTTOM_LEFT_OFFSET;

            origin.BottomLeft = Instantiate(_sectorPrefab, position, Quaternion.identity) as GameObject;
            origin.BottomLeft.GetComponent<Sector>().TopRight = origin.gameObject;

            _sectors.Add(origin.BottomLeft);
        }

        if (origin.Left == null)
        {
            position = origin.transform.position + LEFT_OFFSET;

            origin.Left = Instantiate(_sectorPrefab, position, Quaternion.identity) as GameObject;
            origin.Left.GetComponent<Sector>().Right = origin.gameObject;

            _sectors.Add(origin.Left);
        }

        if (origin.TopLeft == null)
        {
            position = origin.transform.position + TOP_LEFT_OFFSET;

            origin.TopLeft = Instantiate(_sectorPrefab, position, Quaternion.identity) as GameObject;
            origin.TopLeft.GetComponent<Sector>().BottomRight = origin.gameObject;

            _sectors.Add(origin.TopLeft);
        }

        ResolveLooseConnections();
    }

    private void ResolveLooseConnections()
    {
        // resolve broken links
        foreach (var sector in _sectors)
        {
            var component = sector.GetComponent<Sector>();

            if (component.TopRight == null)
                component.TopRight = FindSectorAtPosition(sector.transform.position + TOP_RIGHT_OFFSET);
            if (component.Right == null)
                component.Right = FindSectorAtPosition(sector.transform.position + RIGHT_OFFSET);
            if (component.BottomRight == null)
                component.BottomRight = FindSectorAtPosition(sector.transform.position + BOTTOM_RIGHT_OFFSET);
            if (component.BottomLeft == null)
                component.BottomLeft = FindSectorAtPosition(sector.transform.position + BOTTOM_LEFT_OFFSET);
            if (component.Left == null)
                component.Left = FindSectorAtPosition(sector.transform.position + LEFT_OFFSET);
            if (component.TopLeft == null)
                component.TopLeft = FindSectorAtPosition(sector.transform.position + TOP_LEFT_OFFSET);
        }
    }
	
    private GameObject FindSectorAtPosition(Vector3 position)
    {
        foreach(var sector in _sectors)
        {
            if(Vector3.Distance(sector.transform.position, position) <= 1.0f)
                return sector;
        }

        return null;
    }
}
