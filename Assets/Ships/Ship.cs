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

    protected ShipType shipType;

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

    public ShipType ShipType { get { return shipType; } }

    public Ship(Sprite icon, string name, float hull, float firepower, float speed, int capacity, ShipType shipType, 
        Dictionary<Resource, int> requiredResources)
    {
        this.name = name;
        this.hull = this.baseHull = hull;
        this.firepower = this.baseFirepower = firepower;
        this.speed = this.baseSpeed = speed;
        this.capacity = this.baseCapacity = capacity;
        this.shipType = shipType;
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
        var ship = new Ship(icon, name, baseHull, baseFirepower, baseSpeed, baseCapacity, shipType, requiredResources);
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
        entry.GetComponent<CustomUI>().data = listName + "|" + index.ToString();

        var transfer = (bool)data;

        if(transfer)
            switch(shipType)
            {
                case ShipType.CommandShip:
                case ShipType.Defense:
                    entry.GetComponent<Button>().interactable = false;
                    break;
            }

        return entry;
    }

    GameObject ListableObject.CreateBuildListEntry(string listName, int index, System.Object data) 
    {
        var buildEntry = Resources.Load<GameObject>(CONSTRUCT_PREFAB);
        var entry = GameObject.Instantiate(buildEntry) as GameObject;
        entry.transform.FindChild("Name").GetComponent<Text>().text = name;
        entry.transform.FindChild("Icon").GetComponent<Image>().sprite = icon;
        entry.transform.FindChild("HullText").GetComponent<Text>().text = hull.ToString();
        entry.transform.FindChild("FirepowerText").GetComponent<Text>().text = firepower.ToString();
        entry.transform.FindChild("SpeedText").GetComponent<Text>().text = speed.ToString();
        entry.transform.FindChild("CapacityText").GetComponent<Text>().text = capacity.ToString();
        entry.GetComponent<CustomUI>().data = name;

        var resources = data as Dictionary<Resource, int>;
        if (CanConstruct(resources))
            entry.GetComponent<Button>().interactable = true;

        return entry;
    }
}
