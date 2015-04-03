using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Ship : ListableObject
{
    private const string LIST_PREFAB = "ShipListing";
    private const string CONSTRUCT_PREFAB = "Constructable";

    protected bool unlocked;
    protected Sprite icon;
    protected string name;
    protected float hull;
    protected float firepower;
    protected float speed;
    protected int capacity;
    protected float protection;

    protected float baseHull;
    protected float baseFirepower;
    protected float baseSpeed;
    protected int baseCapacity;

    protected int totalPopulation;
    protected int primitivePopulation;
    protected int industrialPopulation;
    protected int spaceAgePopulation;

    protected Dictionary<Resource, int> requiredResources;

    protected ShipProperties shipProperties;

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
    public float Protection 
    { 
        get { return protection; }
        set { protection = value; }
    }
    public int Population
    {
        get { return totalPopulation; }
        set { totalPopulation = value; }
    }
    public int PrimitivePopulation
    {
        get { return primitivePopulation; }
        set { primitivePopulation = value; }
    }
    public int IndustrialPopulation
    {
        get { return industrialPopulation; }
        set { industrialPopulation = value; }
    }
    public int SpaceAgePopulation
    {
        get { return spaceAgePopulation; }
        set { spaceAgePopulation = value; }
    }
    public bool Unlocked
    {
        get { return unlocked; }
        set { unlocked = value; }
    }

    public ShipProperties ShipProperties { get { return shipProperties; } }

    public Ship(Sprite icon, string name, float hull, float firepower, float speed, int capacity, ShipProperties shipProperties, 
        Dictionary<Resource, int> requiredResources)
    {
        this.name = name;
        this.hull = this.baseHull = hull;
        this.firepower = this.baseFirepower = firepower;
        this.speed = this.baseSpeed = speed;
        this.capacity = this.baseCapacity = capacity;
        this.shipProperties = shipProperties;
        this.icon = icon;
        this.requiredResources = requiredResources;
    }

    protected virtual bool CanConstruct(Dictionary<Resource, int> resources)
    {
        return unlocked && 
            resources[Resource.Stations] >= requiredResources[Resource.Stations] &&
            resources[Resource.Ore] >= requiredResources[Resource.Ore] &&
            resources[Resource.Oil] >= requiredResources[Resource.Oil] &&
            resources[Resource.Asterminium] >= requiredResources[Resource.Asterminium] &&
            resources[Resource.Forest] >= requiredResources[Resource.Forest];
    }

    public virtual Ship Copy()
    {
        var ship = new Ship(icon, name, baseHull, baseFirepower, baseSpeed, baseCapacity, shipProperties, requiredResources);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }

    GameObject ListableObject.CreateListEntry(string listName, int index, System.Object data)
    {
        var shipEntry = Resources.Load<GameObject>(LIST_PREFAB);
        var entry = GameObject.Instantiate(shipEntry) as GameObject;
        var icon = entry.transform.FindChild("Icon").GetComponent<Image>();
        icon.sprite = this.icon;
        entry.transform.FindChild("Name").GetComponent<Text>().text = name;
        entry.transform.FindChild("Population").GetComponent<Text>().text = totalPopulation + " / " + capacity;
        entry.GetComponent<CustomUIAdvanced>().data = listName + "|" + index.ToString();

        var transfer = (bool)data;

        if(transfer && (shipProperties & ShipProperties.Untransferable) > 0)
            entry.GetComponent<Button>().interactable = false;

        return entry;
    }

    GameObject ListableObject.CreateBuildListEntry(string listName, int index, System.Object data) 
    {
        var buildEntry = Resources.Load<GameObject>(CONSTRUCT_PREFAB);
        var entry = GameObject.Instantiate(buildEntry) as GameObject;
        entry.transform.FindChild("Name").GetComponent<Text>().text = name;
        entry.transform.FindChild("Icon").GetComponent<Image>().sprite = icon;
        entry.transform.FindChild("OilText").GetComponent<Text>().text = requiredResources[Resource.Oil].ToString();
        entry.transform.FindChild("OreText").GetComponent<Text>().text = requiredResources[Resource.Ore].ToString();
        entry.transform.FindChild("ForestText").GetComponent<Text>().text = requiredResources[Resource.Forest].ToString();
        entry.transform.FindChild("AsterminiumText").GetComponent<Text>().text = requiredResources[Resource.Asterminium].ToString();
        entry.GetComponent<CustomUIAdvanced>().data = listName + "|" + name;

        var resources = data as Dictionary<Resource, int>;
        if (CanConstruct(resources))
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
        go.transform.FindChild("PopulationText").GetComponent<Text>().text = totalPopulation.ToString();
        go.transform.FindChild("PrimitiveText").GetComponent<Text>().text = primitivePopulation.ToString();
        go.transform.FindChild("IndustrialText").GetComponent<Text>().text = industrialPopulation.ToString();
        go.transform.FindChild("SpaceAgeText").GetComponent<Text>().text = spaceAgePopulation.ToString();
    }
}
