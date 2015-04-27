using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Ship : ListableObject
{
    private const string LIST_PREFAB = "ShipListing";
    private const string CONSTRUCT_PREFAB = "Constructable";
    private const string PLATING = "Asterminium Plating";

    protected bool unlocked;
    protected string name;
    protected float hull;
    protected float firepower;
    protected float speed;
    protected int capacity;
    protected float protection;
    protected int resourceCapacity;
    protected int plating;

    protected Dictionary<Inhabitance, int> population;
    protected Dictionary<Resource, int> requiredResources;
    protected Dictionary<Resource, int> resources;

    protected ShipProperties shipProperties;

    protected bool isDeployed;

    public bool IsDeployed
    {
        get { return isDeployed; }
    }

    public string Name
    {
        get { return name; }
    }
    public float Hull 
    { 
        get { return hull; }
        set { hull = value; }
    }
    public float Firepower 
    { 
        get { return firepower; }
        set { firepower = value; }
    }
    public float Speed 
    { 
        get { return speed; }
        set { speed = value; }
    }
    public int Capacity 
    { 
        get { return capacity; }
        set { capacity = value; }
    }
    public int Plating
    {
        get { return plating; }
        set { plating = value; }
    }
    public int ResourceCapacity
    {
        get { return resourceCapacity; }
        set { resourceCapacity = value; }
    }
    public float Protection 
    { 
        get { return protection; }
        set { protection = value; }
    }
    public bool Unlocked
    {
        get { return unlocked; }
        set { unlocked = value; }
    }

    public Dictionary<Resource, int> Resources { get { return resources; } }
    public Dictionary<Resource, int> RequiredResources { get { return requiredResources; } }
    public Dictionary<Inhabitance, int> Population { get { return population; } }
    public ShipProperties ShipProperties { get { return shipProperties; } }

    public Ship(string name, float hull, float firepower, float speed, int capacity, int rCapacity, int plating, ShipProperties shipProperties)
    {
        this.name = name;
        this.hull = hull;
        this.firepower = firepower;
        this.speed = speed;
        this.capacity = capacity;
        this.resourceCapacity = rCapacity;
        this.shipProperties = shipProperties;
        this.plating = plating;
        this.population = new Dictionary<Inhabitance, int>()
        {
            { Inhabitance.Uninhabited, 0 },
            { Inhabitance.Primitive, 0 },
            { Inhabitance.Industrial, 0 },
            { Inhabitance.SpaceAge, 0 }
        };
        this.requiredResources = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, 0 },
            { Resource.Forest, 0 },
            { Resource.Oil, 0 },
            { Resource.Ore, 0 },
        };
        this.resources = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, 0 },
            { Resource.Forest, 0 },
            { Resource.Oil, 0 },
            { Resource.Ore, 0 },
        };
    }

    public KeyValuePair<bool, Dictionary<Resource, int>> CanConstruct(Dictionary<Resource, int> resources, int n, float r)
    {
        var rs = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt(requiredResources[Resource.Ore] * n * (1.0f - r)) },
            { Resource.Oil, Mathf.CeilToInt(requiredResources[Resource.Oil] * n * (1.0f - r)) },
            { Resource.Asterminium, Mathf.CeilToInt(requiredResources[Resource.Asterminium] * n * (1.0f - r)) },
            { Resource.Forest, Mathf.CeilToInt(requiredResources[Resource.Forest] * n * (1.0f - r)) }
        };

        if (resources == null)
            return new KeyValuePair<bool, Dictionary<Resource, int>>(false, rs);

        return new KeyValuePair<bool, Dictionary<Resource, int>>(
            resources[Resource.Ore] >= rs[Resource.Ore] &&
            resources[Resource.Oil] >= rs[Resource.Oil] &&
            resources[Resource.Asterminium] >= rs[Resource.Asterminium] &&
            resources[Resource.Forest] >= rs[Resource.Forest],
            rs);
    }

    public virtual void RecalculateResources()
    {
        requiredResources[Resource.Ore] = Mathf.CeilToInt(hull * 5);
        requiredResources[Resource.Oil] = Mathf.CeilToInt(hull / 2.0f + speed / 2.0f + firepower / 2.0f);
        requiredResources[Resource.Asterminium] = Mathf.CeilToInt(plating * hull);
        requiredResources[Resource.Forest] = capacity;
    }

    public int CountPopulation()
    {
        int i = 0;
        foreach(var pop in population)
            i += pop.Value;
        return i;
    }

    public int CountResources()
    {
        int i = 0;
        foreach (var resource in resources)
            i += resource.Value;
        return i;
    }

    public virtual Ship Copy()
    {
        var ship = new Ship(name, hull, firepower, speed, capacity, resourceCapacity, plating, shipProperties);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }

    GameObject ListableObject.CreateListEntry(string listName, int index, System.Object data)
    {
        var shipEntry = UnityEngine.Resources.Load<GameObject>(LIST_PREFAB);
        var entry = GameObject.Instantiate(shipEntry) as GameObject;
        var icon = entry.transform.FindChild("Icon").GetComponent<Image>();
        icon.sprite = GUIManager.Instance.Icons[name];
        entry.transform.FindChild("Name").GetComponent<Text>().text = name;
        entry.transform.FindChild("Population").GetComponent<Text>().text = CountPopulation().ToString() + "/" + capacity;
        entry.GetComponent<CustomUIAdvanced>().data = listName + "|" + index.ToString();

        return entry;
    }

    GameObject ListableObject.CreateBuildListEntry(string listName, int index, System.Object data) 
    {
        var buildEntry = UnityEngine.Resources.Load<GameObject>(CONSTRUCT_PREFAB);
        var entry = GameObject.Instantiate(buildEntry) as GameObject;
        entry.transform.FindChild("Name").GetComponent<Text>().text = name;
        entry.transform.FindChild("Icon").GetComponent<Image>().sprite = GUIManager.Instance.Icons[name];
        entry.transform.FindChild("OilText").GetComponent<Text>().text = requiredResources[Resource.Oil].ToString();
        entry.transform.FindChild("OreText").GetComponent<Text>().text = requiredResources[Resource.Ore].ToString();
        entry.transform.FindChild("ForestText").GetComponent<Text>().text = requiredResources[Resource.Forest].ToString();
        entry.transform.FindChild("AsterminiumText").GetComponent<Text>().text = requiredResources[Resource.Asterminium].ToString();
        entry.GetComponent<CustomUIAdvanced>().data = listName + "|" + name;

        var resources = data as Dictionary<Resource, int>;
        if (CanConstruct(resources, 1, 0f).Key && unlocked)
            entry.GetComponent<Button>().interactable = true;

        return entry;
    }

    void ListableObject.PopulateBuildInfo(GameObject popUp, System.Object data)
    {
        var go = popUp; // construction info only
        go.transform.FindChild("HullText").GetComponent<Text>().text = hull.ToString();
        go.transform.FindChild("FirepowerText").GetComponent<Text>().text = firepower.ToString();
        go.transform.FindChild("SpeedText").GetComponent<Text>().text = speed.ToString();
        go.transform.FindChild("CapacityText").GetComponent<Text>().text = capacity.ToString();
        go.transform.FindChild("Description").GetComponent<Text>().text = (string)data;
        go.transform.FindChild("ResourceCapacityText").GetComponent<Text>().text = resourceCapacity.ToString();
        go.transform.FindChild("DefenseIcon").gameObject.SetActive(false);
        go.transform.FindChild("DefenseText").gameObject.SetActive(false);
        go.transform.FindChild("DeployedCapacityIcon").gameObject.SetActive(false);
        go.transform.FindChild("DeployedCapacityText").gameObject.SetActive(false);
        go.transform.FindChild("GatherRateIcon").gameObject.SetActive(false);
        go.transform.FindChild("GatherRateText").gameObject.SetActive(false);
    }

    void ListableObject.PopulateGeneralInfo(GameObject popUp, System.Object data)
    {
        var go = popUp;
        go.transform.FindChild("HullText").GetComponent<Text>().text = hull.ToString();
        go.transform.FindChild("FirepowerText").GetComponent<Text>().text = firepower.ToString();
        go.transform.FindChild("SpeedText").GetComponent<Text>().text = speed.ToString();
        go.transform.FindChild("CapacityText").GetComponent<Text>().text = capacity.ToString();
        go.transform.FindChild("PopulationText").GetComponent<Text>().text = CountPopulation().ToString() + "/" + capacity.ToString();
        foreach(var pop in population)
            go.transform.FindChild(pop.Key.ToString() + "Text").GetComponent<Text>().text = pop.Value.ToString();
        go.transform.FindChild("ResourceCapacityText").GetComponent<Text>().text =
            (resources[Resource.Forest] + resources[Resource.Oil] + resources[Resource.Ore] + resources[Resource.Asterminium]).ToString() + "/" + resourceCapacity.ToString();
        go.transform.FindChild("OreText").GetComponent<Text>().text = resources[Resource.Ore].ToString();
        go.transform.FindChild("OilText").GetComponent<Text>().text = resources[Resource.Oil].ToString();
        go.transform.FindChild("ForestText").GetComponent<Text>().text = resources[Resource.Forest].ToString();
        go.transform.FindChild("AsterminiumText").GetComponent<Text>().text = resources[Resource.Asterminium].ToString();
    }
}
