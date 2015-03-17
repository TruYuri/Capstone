using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    private static System.Random Generator = new System.Random();

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

    private Bounds _bounds;
    private string _planetType;
    private int _population;
    private Inhabitance _planetInhabitance;
    private TileSize _tileSize;
    private Resource _resourceType;
    private int _resourceCount;
    private Team _owner;
    private Structure _structure;

    public Bounds Bounds { get { return _bounds; } }
    public string PlanetType { get { return _planetType; } }
    public int Population { get { return _population; } }
    public Inhabitance PlanetInhabitanceLevel { get { return _planetInhabitance; } }
    public Resource ResourceType { get { return _resourceType; } }
    public int ResourceCount { get { return _resourceCount; } }
    public Team Team { get { return _owner; } }
    public Structure DeployedStructure { get { return _structure; } }

	// Use this for initialization
	void Start () 
    {
        _bounds = new UnityEngine.Bounds(this.transform.position, new Vector3(10, 10, 10));
        _owner = Team.None;

        // Determine tile type
        var mapManager = MapManager.Instance;
        var chance = (float)Generator.NextDouble();
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
        chance = (float)Generator.NextDouble();
        if (chance < float.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_SPAWN_DETAIL]))
        {
            _tileSize = TileSize.Small;

            var minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_POPULATION_MIN_DETAIL]);
            var maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_POPULATION_MAX_DETAIL]);
            _population = Generator.Next(minimum, maximum + 1);

            minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_RESOURCE_MIN_DETAIL]);
            maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_RESOURCE_MAX_DETAIL]);
            _resourceCount = Generator.Next(minimum, maximum + 1);
        }
        else
        {
            _tileSize = TileSize.Large;

            var minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_POPULATION_MIN_DETAIL]);
            var maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_POPULATION_MAX_DETAIL]);
            _population = Generator.Next(minimum, maximum + 1);

            minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_RESOURCE_MIN_DETAIL]);
            maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_RESOURCE_MAX_DETAIL]);
            _resourceCount = Generator.Next(minimum, maximum + 1);
        }

        // Determine Inhabitance
        chance = (float)Generator.NextDouble();
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
        chance = (float)Generator.NextDouble();
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

            if (_tileSize == TileSize.Small)
                system.startSize *= 0.5f;

            renderer.material.mainTexture = mapManager.PlanetTextureTable[_planetType].Texture;
            renderer.material.mainTextureOffset = mapManager.PlanetTextureTable[_planetType].TextureOffset;
            renderer.material.mainTextureScale = mapManager.PlanetTextureTable[_planetType].TextureScale;

            system.enableEmission = true;
            renderer.enabled = true;

            this.GetComponent<SphereCollider>().enabled = true;
            this.GetComponent<Squad>().enabled = true;
        }
	}

	// Update is called once per frame
	void Update () 
    {

        if (this.GetComponent<Renderer>().isVisible)
        {
            this.GetComponent<ParticleSystem>().enableEmission = true;
            this.GetComponent<Collider>().enabled = true;
        }
        else
        {
            this.GetComponent<ParticleSystem>().enableEmission = false;
            this.GetComponent<Collider>().enabled = false;
        }
	}
}
