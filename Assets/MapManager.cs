using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    private static Object Sector;

    private readonly Vector3 TOP_RIGHT_OFFSET = new Vector3(98.3f, 0.0f, 149.0f);
    private readonly Vector3 RIGHT_OFFSET = new Vector3(196.5f, 0, 0.0f);
    private readonly Vector3 BOTTOM_RIGHT_OFFSET = new Vector3(98.3f, 0.0f, -149.0f);
    private readonly Vector3 BOTTOM_LEFT_OFFSET = new Vector3(-98.3f, 0.0f, -149.0f);
    private readonly Vector3 LEFT_OFFSET = new Vector3(-196.5f, 0, 0);
    private readonly Vector3 TOP_LEFT_OFFSET = new Vector3(-98.3f, 0.0f, 149.0f);
    private const string SECTOR_PREFAB = "Sector";
    private const string INI_PATH = "/Resources/Planets.ini";
    private const string PLANET_SECTION_HEADER = "[PlanetSpawnRates]";

    private static MapManager _instance;
    private List<GameObject> _sectors;
    private Dictionary<string, float> _planetSpawnTable;
    private Dictionary<string, Dictionary<string, string>> _planetSpawnDetails;

    public static MapManager Instance { get { return _instance; } }
    public Dictionary<string, float> PlanetSpawnTable { get { return _planetSpawnTable; } }
    public Dictionary<string, Dictionary<string, string>> PlanetSpawnDetails { get { return _planetSpawnDetails; } }

	// Use this for initialization
	public void Start()
    {
        _sectors = new List<GameObject>();
        Sector = Resources.Load<GameObject>(SECTOR_PREFAB);

        INIParser parser = new INIParser(Application.dataPath + INI_PATH);
        var spawnTables = parser.ParseINI();

        _planetSpawnTable = new Dictionary<string, float>();
        foreach (var planet in spawnTables[PLANET_SECTION_HEADER])
        {
            _planetSpawnTable.Add('[' + planet.Key + ']', float.Parse(planet.Value));
        }
        spawnTables.Remove(PLANET_SECTION_HEADER);
        _planetSpawnDetails = spawnTables;
        parser.CloseINI();

        _sectors.Add(Instantiate(Sector, Vector3.zero, Quaternion.identity) as GameObject);
        _instance = this;
	}

    public void GenerateNewSectors(Sector origin)
    {
        var position = Vector3.zero;

        // Generate any needed immediate neighbors and link them
        if (origin.TopRight == null)
        {
            position = origin.transform.position + TOP_RIGHT_OFFSET;

            origin.TopRight = Instantiate(Sector, position, Quaternion.identity) as GameObject;
            origin.TopRight.GetComponent<Sector>().BottomLeft = origin.gameObject;

            _sectors.Add(origin.TopRight);
        }

        if (origin.Right == null)
        {
            position = origin.transform.position + RIGHT_OFFSET;

            origin.Right = Instantiate(Sector, position, Quaternion.identity) as GameObject;
            origin.Right.GetComponent<Sector>().Left = origin.gameObject;

            _sectors.Add(origin.Right);
        }

        if (origin.BottomRight == null)
        {
            position = origin.transform.position + BOTTOM_RIGHT_OFFSET;

            origin.BottomRight = Instantiate(Sector, position, Quaternion.identity) as GameObject;
            origin.BottomRight.GetComponent<Sector>().TopLeft = origin.gameObject;

            _sectors.Add(origin.BottomRight);
        }

        if (origin.BottomLeft == null)
        {
            position = origin.transform.position + BOTTOM_LEFT_OFFSET;

            origin.BottomLeft = Instantiate(Sector, position, Quaternion.identity) as GameObject;
            origin.BottomLeft.GetComponent<Sector>().TopRight = origin.gameObject;

            _sectors.Add(origin.BottomLeft);
        }

        if (origin.Left == null)
        {
            position = origin.transform.position + LEFT_OFFSET;

            origin.Left = Instantiate(Sector, position, Quaternion.identity) as GameObject;
            origin.Left.GetComponent<Sector>().Right = origin.gameObject;

            _sectors.Add(origin.Left);
        }

        if (origin.TopLeft == null)
        {
            position = origin.transform.position + TOP_LEFT_OFFSET;

            origin.TopLeft = Instantiate(Sector, position, Quaternion.identity) as GameObject;
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
