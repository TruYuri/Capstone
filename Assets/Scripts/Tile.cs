using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
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

    private const string PLANET_NAME = "AssignedName";

    private string _name;
    private Bounds _bounds;
    private string _planetType;
    private int _population;
    private Inhabitance _planetInhabitance;
    private TileSize _tileSize;
    private Resource _resourceType;
    private int _resourceCount;
    private Team _team;
    private Structure _structure;
    private List<Ship> _defenses;
    private float _power;
    private Squad _defenseSquad;

    public string Name { get { return _name; } }
    public Bounds Bounds { get { return _bounds; } }
    public string PlanetType { get { return _planetType; } }
    public int Population { get { return _population; } }
    public Inhabitance PlanetInhabitanceLevel { get { return _planetInhabitance; } }
    public Resource ResourceType { get { return _resourceType; } }
    public int ResourceCount { get { return _resourceCount; } }
    public Team Team { get { return _team; } }
    public Structure DeployedStructure { get { return _structure; } }
    public float Power { get { return _power; } }
    public Squad Squad { get { return _defenseSquad; } }

	// Use this for initialization
	void Start () 
    {
        _bounds = new UnityEngine.Bounds(this.transform.position, new Vector3(10, 10, 10));

        // Determine tile type
        var mapManager = MapManager.Instance;
        var chance = (float)GameManager.Generator.NextDouble();
        foreach(var planet in mapManager.PlanetTypeSpawnTable)
        {
            if (chance <= planet.Value)
            {
                _planetType = planet.Key;
                break;
            }
        }

        if (mapManager.PlanetTextureTable[_planetType].Texture == null) // crappy way to check if it's empty space, but it works for now
            return;

        // Determine Size, Population, and Resource Amount
        chance = (float)GameManager.Generator.NextDouble();
        if (chance < float.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_SPAWN_DETAIL]))
        {
            _tileSize = TileSize.Small;

            var minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_POPULATION_MIN_DETAIL]);
            var maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_POPULATION_MAX_DETAIL]);
            _population = GameManager.Generator.Next(minimum, maximum + 1);

            minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_RESOURCE_MIN_DETAIL]);
            maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_RESOURCE_MAX_DETAIL]);
            _resourceCount = GameManager.Generator.Next(minimum, maximum + 1);
        }
        else
        {
            _tileSize = TileSize.Large;

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
        _resourceType = Resource.None;
        chance = (float)GameManager.Generator.NextDouble();
        foreach (var resource in mapManager.PlanetResourceSpawnTable[_planetType])
        {
            if (chance <= resource.Value)
            {
                _resourceType = resource.Key;
                break;
            }
        }

        var sector = this.transform.parent.GetComponent<Sector>();
        if(!sector.PlanetCounts.ContainsKey(_planetType))
            sector.PlanetCounts.Add(_planetType, 0);

        _name = MapManager.Instance.PlanetSpawnDetails[_planetType][PLANET_NAME]
            + "-"
            + Math.Abs(sector.HorizontalGridPosition).ToString() + Math.Abs(sector.VerticalGridPosition).ToString()
            + PlanetSuffix(_planetType, sector);

        if(mapManager.PlanetTextureTable[_planetType].Texture != null)
        {
            var system = GetComponent<ParticleSystem>();
            var renderer = system.GetComponent<Renderer>();

            if (_tileSize == TileSize.Small)
                system.startSize *= 0.5f;

            renderer.material.mainTexture = mapManager.PlanetTextureTable[_planetType].Texture;
            renderer.material.mainTextureOffset = mapManager.PlanetTextureTable[_planetType].TextureOffset;
            renderer.material.mainTextureScale = mapManager.PlanetTextureTable[_planetType].TextureScale;

            system.enableEmission = true;
            renderer.enabled = true;

            this.GetComponent<SphereCollider>().enabled = true;
            _defenseSquad = this.GetComponent<Squad>();
            _defenseSquad.enabled = true;

            if (_population > 0)
            {
                _team = Team.Indigineous;
                _defenseSquad.Team = Team.Indigineous;
            }

            // debug
            _team = Team.Union;
            _defenseSquad.Team = Team.Union;
        }
	}

    private string PlanetSuffix(string type, Sector sector)
    {
        int diff = sector.PlanetCounts[type]++;
        string val = string.Empty;

        if(diff > 26)
        {
            var n = diff / 26;
            diff -= 26 * n;
        }

        val += (char)('a' + diff);

        if(sector.VerticalGridPosition >= 0 && sector.HorizontalGridPosition >= 0)
            val += "-q1";
        else if(sector.VerticalGridPosition < 0 && sector.HorizontalGridPosition >= 0)
            val += "-q2";
        else if(sector.VerticalGridPosition < 0 && sector.HorizontalGridPosition < 0)
            val += "-q3";
        else
            val += "-q4";

        return val;
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

    public void AddDefense(Ship defense)
    {
        _defenses.Add(defense);
        RecalculatePower();
    }

    public void DestroyBase()
    {
        _structure = null;
        _defenses.Clear();
        RecalculatePower();
    }

    public void Claim(Team team)
    {
        _team = team;
        _defenseSquad.Team = team;
    }

    public void Undeploy()
    {
        _defenseSquad.AddShip(_structure);
        _structure = null;
        RecalculatePower();
    }

    public void Deploy(Structure ship, Team team)
    {
        Claim(team);
        _structure = ship;
        RecalculatePower();
    }

    private void RecalculatePower()
    {
        _power = 0;
        if (_team == Team.Indigineous) // use indigineous
        {
            switch(_planetInhabitance)
            {
                case Inhabitance.Uninhabited:
                    break;
                case Inhabitance.Primitive:
                    _power = _population;
                    break;
                case Inhabitance.Industrial:
                    _power = _population * 1.5f;
                    break;
                case Inhabitance.SpaceAge:
                    _power = _population * 1.75f;
                    break;
            }
        }
        else // use deployed
        {
            if (_structure == null)
                return;

            float primitive = _structure.PrimitivePopulation;
            float industrial = _structure.IndustrialPopulation;
            float spaceAge = _structure.SpaceAgePopulation;

            _power = primitive + industrial * 1.5f + spaceAge * 1.75f + _structure.Defense;
        }
    }
}
