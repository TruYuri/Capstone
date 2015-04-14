using System;
using System.Linq;
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

    private const string TILE_LISTING_PREFAB = "TileListing";

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
    private Dictionary<Team, bool> _diplomacy;

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
        _squad.Init(Team.Uninhabited, sector, _name);
        _diplomacy = new Dictionary<global::Team, bool>();
        transform.SetParent(sector.transform);
    }

	void Start () 
    {
        // Determine tile type
        var mapManager = MapManager.Instance;
        // Determine Size, Population, and Resource Amount

        // Determine Inhabitance
        var chance = (float)GameManager.Generator.NextDouble();
        foreach (var inhabit in mapManager.PlanetInhabitanceSpawnTable[_planetType])
        {
            if (chance <= inhabit.Value)
            {
                _planetInhabitance = inhabit.Key;
                break;
            }
        }

        chance = (float)GameManager.Generator.NextDouble();
        var small = true;
        int population = 0;
        if (chance < float.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_SPAWN_DETAIL]))
        {
            small = true;

            int minimum, maximum;
            if (_planetInhabitance != Inhabitance.Uninhabited)
            {
                minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_POPULATION_MIN_DETAIL]);
                maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_POPULATION_MAX_DETAIL]);
                population = GameManager.Generator.Next(minimum, maximum + 1);
            }

            minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_RESOURCE_MIN_DETAIL]);
            maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_RESOURCE_MAX_DETAIL]);
            _resourceCount = GameManager.Generator.Next(minimum, maximum + 1);
        }
        else
        {
            small = false;

            int minimum, maximum;
            if (_planetInhabitance != Inhabitance.Uninhabited)
            {
                minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_POPULATION_MIN_DETAIL]);
                maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_POPULATION_MAX_DETAIL]);
                population = GameManager.Generator.Next(minimum, maximum + 1);
            }

            minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_RESOURCE_MIN_DETAIL]);
            maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_RESOURCE_MAX_DETAIL]);
            _resourceCount = GameManager.Generator.Next(minimum, maximum + 1);
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

            if (population > 0)
            {
                _team = Team.Indigenous;
                _squad.Team = Team.Indigenous;

                if (!GameManager.Instance.Players.ContainsKey(_team))
                    GameManager.Instance.AddAIPlayer(_team);
                GameManager.Instance.Players[_team].AddSoldiers(this, _planetInhabitance, population);
                GameManager.Instance.Players[_team].ClaimTile(this);
                // generate random defenses if space age
            }
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

    public void Claim(Team team)
    {
        if (_team != Team.Uninhabited)
        {
            GameManager.Instance.Players[_team].RelinquishTile(this);

            if (_team == Team.Plinthen)
            {
                _resourceCount -= Mathf.CeilToInt(_resourceCount * 1.15f);
                _resourceCount = (_resourceCount < 0 ? 0 : _resourceCount);
            }
        }

        _team = team;

        if (_team == Team.Plinthen)
            _resourceCount += Mathf.CeilToInt(_resourceCount * 1.15f);

        GameManager.Instance.Players[_team].ClaimTile(this);
        _squad.Team = _team;

        var teams = _diplomacy.Keys.ToList();
        foreach(var t in teams)
        {
            _diplomacy[t] = false;
        }
    }

    public string Undeploy(bool destroyStructure)
    {
        if (_structure == null)
            return "";

        _squad.Ships.Add(_structure);
        _structure.Undeploy(this);

        if (destroyStructure)
            GameManager.Instance.Players[_team].RemoveShip(_squad, _structure);
        _structure = null;

        if(MapManager.Instance.DeploySpawnTable.ContainsKey(_planetType))
        {
            _squad.Sector.UnregisterSpaceStructure(_team, _structure);
            _squad.Sector.DeleteTile(this);
        }

        return _planetType;
    }

    public void Deploy(Structure ship, ShipProperties structureType, Team team)
    {
        if(_team != team)
            Claim(team);
        _structure = ship;
        _structure.Deploy(this);
    }

    public void GatherAndGrow()
    {
        _population += Mathf.CeilToInt(_population * 0.05f);

        if (_structure == null)
            return;

        var gathered = _structure.Gather(_resourceType, _resourceCount, _planetInhabitance, _population, _team);

        foreach (var resource in gathered)
        {
            switch (resource.Key)
            {
                case ResourceGatherType.None:
                case ResourceGatherType.Soldiers:
                    GameManager.Instance.Players[_team].RemoveSoldiers(this, false, PopulationType, resource.Value);
                    break;
                case ResourceGatherType.Research:
                    break;
                case ResourceGatherType.Natural:
                    _resourceCount -= resource.Value;
                    break;
            }
        }
    }

    public float CalculateDefensivePower()
    {
        var power = 0f;
        var bonuses = new Dictionary<Inhabitance, float>()
        {
            { Inhabitance.Primitive, 1f },
            { Inhabitance.Industrial, 1.5f },
            { Inhabitance.SpaceAge, 1.75f }
        };
        
        power += _population * bonuses[_planetInhabitance];
        if (_structure != null)
        {
            foreach(var bonus in bonuses)
                power += _structure.Population[bonus.Key] * bonus.Value;
        }

        return power * (_team == Team.Kharkyr ? 1.15f : 1f);
    }

    public void SetDiplomaticEffort(Team team)
    {
        if (!_diplomacy.ContainsKey(team))
            _diplomacy.Add(team, true);
        else
            _diplomacy[team] = true;
    }

    public void EndDiplomaticEffort(Team team)
    {
        _diplomacy[team] = false;
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

        if(_team == Team.Indigenous)
        {
            if(_diplomacy.ContainsKey(HumanPlayer.Instance.Team) && _diplomacy[HumanPlayer.Instance.Team])
            {
                panel.transform.FindChild("Diplomacy").gameObject.SetActive(false);
                panel.transform.FindChild("DiplomacyInProgress").gameObject.SetActive(true);
            }
            else
            {
                panel.transform.FindChild("Diplomacy").gameObject.SetActive(true);
                panel.transform.FindChild("DiplomacyInProgress").gameObject.SetActive(false);
            }
        }
        else
        {
            panel.transform.FindChild("Diplomacy").gameObject.SetActive(false);
            panel.transform.FindChild("DiplomacyInProgress").gameObject.SetActive(false);
        }

        if (_resourceType != Resource.NoResource)
        {
            var name = panel.transform.FindChild("ResourceName");
            var icon = panel.transform.FindChild("ResourceIcon");
            var amount = panel.transform.FindChild("ResourceCount");
            name.gameObject.SetActive(true);
            icon.gameObject.SetActive(true);
            name.GetComponent<Text>().text = _resourceType.ToString();
            amount.GetComponent<Text>().text = _resourceCount.ToString();
            icon.GetComponent<Image>().sprite = GUIManager.Instance.Icons[_resourceType.ToString()];
        }
        else
        {
            panel.transform.FindChild("ResourceName").gameObject.SetActive(false);
            panel.transform.FindChild("ResourceIcon").gameObject.SetActive(false);
        }

        var populations = new Dictionary<Inhabitance, int>() 
        {
            { Inhabitance.Primitive, 0 },
            { Inhabitance.Industrial, 0 },
            { Inhabitance.SpaceAge, 0 }
        };
        if(_structure != null)
            populations = new Dictionary<Inhabitance, int>(_structure.Population);
        if(_planetInhabitance != Inhabitance.Uninhabited)
            populations[_planetInhabitance] += _population;

        int total = 0;
        foreach(var pop in populations)
        {
            panel.transform.FindChild(pop.Key.ToString() + "Population").GetComponent<Text>().text = pop.Value.ToString();
            total += pop.Value;
        }

        panel.transform.FindChild("TotalPopulation").GetComponent<Text>().text = total.ToString();
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
    GameObject ListableObject.CreateListEntry(string listName, int index, System.Object data) 
    {
        var tileEntry = Resources.Load<GameObject>(TILE_LISTING_PREFAB);
        var entry = Instantiate(tileEntry) as GameObject;

        entry.transform.FindChild("Name").GetComponent<Text>().text = _name;
        var tileRenderer = this.GetComponent<ParticleSystem>().GetComponent<Renderer>();
        var uiRenderer = entry.transform.FindChild("Icon").GetComponent<RawImage>();
        uiRenderer.texture = tileRenderer.material.mainTexture;
        uiRenderer.uvRect = new Rect(tileRenderer.material.mainTextureOffset.x,
                                     tileRenderer.material.mainTextureOffset.y,
                                     tileRenderer.material.mainTextureScale.x,
                                     tileRenderer.material.mainTextureScale.y);
        entry.GetComponent<CustomUIAdvanced>().data = listName + "|" + index;

        return entry;
    }

    GameObject ListableObject.CreateBuildListEntry(string listName, int index, System.Object data) { return null; }
    void ListableObject.PopulateBuildInfo(GameObject popUp, System.Object data) { }
    void ListableObject.PopulateGeneralInfo(GameObject popUp, System.Object data) { }
}
