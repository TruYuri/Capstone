using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour, ListableObject
{
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

    private float _radius;
    private float _clickRadius;
    private string _name;
    private string _planetType;
    private int _population;
    private Inhabitance _planetInhabitance;
    private Resource _resourceType;
    private int _resourceCount;
    private Team _team;
    private Structure _structure;
    private Squad _squad;

    public string Name { get { return _name; } }
    public Team Team { get { return _team; } }
    public Structure Structure { get { return _structure; } }
    public Squad Squad { get { return _squad; } }
    public float Radius { get { return _radius; } }
    public int Population 
    { 
        get { return _population; }
        set { _population = value; }
    }
    public Inhabitance PopulationType {  get { return _planetInhabitance; } }

	// Use this for initialization
    public void Init(string type, string name, Sector sector)
    {
        _name = name;
        _planetType = type;
        _squad = this.GetComponent<Squad>();
        _squad.Init();
        this.transform.SetParent(sector.transform);
    }

	void Start () 
    {
        // Determine tile type
        var mapManager = MapManager.Instance;
        // Determine Size, Population, and Resource Amount
        var chance = (float)GameManager.Generator.NextDouble();
        var small = true;
        if (chance < float.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_SPAWN_DETAIL]))
        {
            small = true;

            var minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_POPULATION_MIN_DETAIL]);
            var maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_POPULATION_MAX_DETAIL]);
            _population = GameManager.Generator.Next(minimum, maximum + 1);

            minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_RESOURCE_MIN_DETAIL]);
            maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_RESOURCE_MAX_DETAIL]);
            _resourceCount = GameManager.Generator.Next(minimum, maximum + 1);
        }
        else
        {
            small = false;

            var minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_POPULATION_MIN_DETAIL]);
            var maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_POPULATION_MAX_DETAIL]);
            _population = GameManager.Generator.Next(minimum, maximum + 1);

            minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_RESOURCE_MIN_DETAIL]);
            maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_RESOURCE_MAX_DETAIL]);
            _resourceCount = GameManager.Generator.Next(minimum, maximum + 1);
        }

        // Determine Inhabitance
        chance = (float)GameManager.Generator.NextDouble();
        foreach(var inhabit in mapManager.PlanetInhabitanceSpawnTable[_planetType])
        {
            if (chance <= inhabit.Value)
            {
                _planetInhabitance = inhabit.Key;
                break;
            }
        }

        // Determine Resource Type
        _resourceType = Resource.NoResource;
        chance = (float)GameManager.Generator.NextDouble();
        foreach (var resource in mapManager.PlanetResourceSpawnTable[_planetType])
        {
            if (chance <= resource.Value)
            {
                _resourceType = resource.Key;
                break;
            }
        }

        if(mapManager.PlanetTextureTable[_planetType].Texture != null)
        {
            var system = GetComponent<ParticleSystem>();
            var renderer = system.GetComponent<Renderer>();

            _radius = 2.0f;
            _clickRadius = 1.5f;
            if (small)
            {
                system.startSize *= 0.5f;
                _radius = 1.0f;
                _clickRadius = 1.0f;
            }

            renderer.material.mainTexture = mapManager.PlanetTextureTable[_planetType].Texture;
            renderer.material.mainTextureOffset = mapManager.PlanetTextureTable[_planetType].TextureOffset;
            renderer.material.mainTextureScale = mapManager.PlanetTextureTable[_planetType].TextureScale;

            system.enableEmission = true;
            renderer.enabled = true;

            if (_population > 0)
            {
                _team = Team.Indigineous;
                _squad.Team = Team.Indigineous;

                // generate random defenses if space age
            }

            // debug
            _team = Team.Union;
            _squad.Team = Team.Union;
        }
	}

	// Update is called once per frame
	void Update () 
    {

        if (this.GetComponent<Renderer>().isVisible)
        {
            this.GetComponent<ParticleSystem>().enableEmission = true;
        }
        else
        {
            this.GetComponent<ParticleSystem>().enableEmission = false;
        }
	}

    public void Relinquish()
    {
        GameManager.Instance.Players[_team].Squads.Remove(_squad);
        _team = Team.Uninhabited;
    }

    public void Claim(Team team)
    {
        _team = team;
        GameManager.Instance.Players[team].Squads.Add(_squad);
        _squad.Team = _team;
    }

    public string Undeploy(bool destroy)
    {
        if (!destroy && _structure != null)
            _squad.Ships.Add(_structure);
        _structure.Undeploy(this);
        _structure = null;
        return _planetType;
    }

    public void Deploy(Structure ship, ShipProperties structureType, Team team)
    {
        Claim(team);
        _structure = ship;
        _structure.Deploy(this);
    }

    public float CalculateDefensivePower()
    {
        var power = 0f;
        if (_team == Team.Indigineous) // use indigineous
        {
            switch(_planetInhabitance)
            {
                case Inhabitance.Uninhabited:
                    break;
                case Inhabitance.Primitive:
                    power = _population;
                    break;
                case Inhabitance.Industrial:
                    power = _population * 1.5f;
                    break;
                case Inhabitance.SpaceAge:
                    power = _population * 1.75f;
                    break;
            }
        }
        else if (_structure == null)
        {
            var primitive = _structure.PrimitivePopulation;
            var industrial = _structure.IndustrialPopulation;
            var spaceAge = _structure.SpaceAgePopulation;

            power = primitive + industrial * 1.5f + spaceAge * 1.75f + _structure.Defense;
        }

        return power;
    }

    public void PopulateInfoPanel(GameObject panel)
    {
        var tileRenderer = this.GetComponent<ParticleSystem>().GetComponent<Renderer>();
        var uiRenderer = panel.transform.FindChild("PlanetIcon").GetComponent<RawImage>();
        uiRenderer.texture = tileRenderer.material.mainTexture;
        uiRenderer.uvRect = new Rect(tileRenderer.material.mainTextureOffset.x,
                                     tileRenderer.material.mainTextureOffset.y,
                                     tileRenderer.material.mainTextureScale.x,
                                     tileRenderer.material.mainTextureScale.y);
        panel.transform.FindChild("PlanetName").GetComponent<Text>().text = _name;
        panel.transform.FindChild("TeamName").GetComponent<Text>().text = _team.ToString();
        panel.transform.FindChild("TeamIcon").GetComponent<Image>().sprite = GUIManager.Instance.Icons[_team.ToString()];

        if (_resourceType != Resource.NoResource)
        {
            var name = panel.transform.FindChild("ResourceName");
            var icon = panel.transform.FindChild("ResourceIcon");
            name.gameObject.SetActive(true);
            icon.gameObject.SetActive(true);
            name.GetComponent<Text>().text = _resourceType.ToString() + "\n" + _resourceCount.ToString();
            icon.GetComponent<Image>().sprite = GUIManager.Instance.Icons[_resourceType.ToString()];
        }
        else
        {
            panel.transform.FindChild("ResourceName").gameObject.SetActive(false);
            panel.transform.FindChild("ResourceIcon").gameObject.SetActive(false);
        }

        var total = panel.transform.FindChild("TotalPopulation").GetComponent<Text>();
        var primitive = panel.transform.FindChild("PrimitivePopulation").GetComponent<Text>();
        var industrial = panel.transform.FindChild("IndustrialPopulation").GetComponent<Text>();
        var spaceAge = panel.transform.FindChild("SpaceAgePopulation").GetComponent<Text>();
        var zero = Convert.ToString(0);

        if(_team == Team.Indigineous)
        {
            total.text = _population.ToString();

            switch(_planetInhabitance)
            {
                case Inhabitance.Primitive:
                    primitive.text = _population.ToString();
                    industrial.text = zero;
                    spaceAge.text = zero;
                    break;
                case Inhabitance.Industrial:
                    primitive.text = zero;
                    industrial.text = _population.ToString();
                    spaceAge.text = zero;
                    break;
                case Inhabitance.SpaceAge:
                    primitive.text = zero;
                    industrial.text = zero;
                    spaceAge.text = _population.ToString();
                    break;
            }
        }
        else if(_structure != null)
        {
            total.text = (_structure.PrimitivePopulation + _structure.IndustrialPopulation + _structure.SpaceAgePopulation).ToString();
            primitive.text = _structure.PrimitivePopulation.ToString();
            industrial.text = _structure.IndustrialPopulation.ToString();
            spaceAge.text = _structure.SpaceAgePopulation.ToString();
        }
        else
        {
            total.text = zero;
            primitive.text = zero;
            industrial.text = zero;
            spaceAge.text = zero;
        }
    }

    public bool IsInRange(Squad squad)
    {
        return (squad.transform.position - transform.position).sqrMagnitude <= (_radius * _radius);
    }

    public bool IsInClickRange(Vector3 click)
    {
        return (click - transform.position).sqrMagnitude <= (_clickRadius * _clickRadius);
    }

    // for info lists later
    GameObject ListableObject.CreateListEntry(string listName, int index, System.Object data) { return null; }
    GameObject ListableObject.CreateBuildListEntry(string listName, int index, System.Object data) { return null; }
    void ListableObject.PopulateBuildInfo(GameObject popUp, System.Object data) { }
    void ListableObject.PopulateGeneralInfo(GameObject popUp, System.Object data) { }
}
