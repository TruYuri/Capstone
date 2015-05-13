using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Definition and behavior for tiles/planets. Like squad, one of the most important in the game.
/// </summary>
public class Tile : MonoBehaviour, ListableObject
{
    private const string TILE_LISTING_PREFAB = "TileListing";
    private const string CIRCLE_PREFAB = "Circle";
    private const string SQUAD_COUNT_PREFAB = "ShipCountListing";

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
    private GameObject _circle;
    private float _defensiveRange;

    public string Name { get { return _name; } }
    public Team Team { get { return _team; } }
    public Structure Structure { get { return _structure; } }
    public string Type { get { return _planetType; } }
    public Squad Squad { get { return _squad; } }
    public float Radius { get { return _radius; } }
    public float DefensiveRange { get { return _defensiveRange; } }
    public int Population 
    { 
        get { return _population; }
        set { _population = value; }
    }
    public Inhabitance PopulationType {  get { return _planetInhabitance; } }
    public GameObject Ring { get { return _circle; } }

	/// <summary>
	/// Initializes this tile.
	/// </summary>
	/// <param name="sector">The parent sector.</param>
	/// <param name="type">The planet type.</param>
	/// <param name="name">The planet name.</param>
	/// <param name="pType">The population type.</param>
	/// <param name="p">The population count.</param>
	/// <param name="rType">The resource type.</param>
	/// <param name="rCount">The resource count.</param>
	/// <param name="size">The planet size.</param>
	/// <param name="team">The owning team.</param>
    public void Init(Sector sector, string type, string name, Inhabitance pType, int p, Resource rType, int rCount, TileSize size, Team team)
    {
        _squad = this.GetComponent<Squad>();
        _squad.Init(Team.Uninhabited, sector, name);
        _diplomacy = new Dictionary<global::Team, bool>();
        transform.SetParent(sector.transform);

        _planetType = type;
        _name = name;
        _planetInhabitance = pType;
        _resourceType = rType;
        _resourceCount = rCount;
        _team = team;
        _squad.Team = team;

        var mapManager = MapManager.Instance;
        var system = GetComponent<ParticleSystem>();
        var renderer = system.GetComponent<Renderer>();

        _radius = 5.0f;
        _clickRadius = 1.5f;
        system.startSize = 5.0f;
        _defensiveRange = 50f;
        if (size == TileSize.Small)
        {
            system.startSize *= 0.5f;
            _radius *= 0.5f;
            _clickRadius *= 0.5f;
        }

        renderer.material.mainTexture = mapManager.PlanetTextureTable[_planetType].Texture;
        renderer.material.mainTextureOffset = mapManager.PlanetTextureTable[_planetType].TextureOffset;
        renderer.material.mainTextureScale = mapManager.PlanetTextureTable[_planetType].TextureScale;

        system.enableEmission = true;
        renderer.enabled = true;

        if (_team != Team.Uninhabited)
        {
            var pl = GameManager.Instance.Players[_team];
            if(_planetInhabitance != Inhabitance.Uninhabited)
                pl.AddSoldiers(this, _planetInhabitance, p);
            
            // generate random defenses if space age or non-player team
            if((_team == Team.Indigenous && _planetInhabitance == Inhabitance.SpaceAge) || (_team != HumanPlayer.Instance.Team && _team != Team.Indigenous))
            {
                ((AIPlayer)pl).PopulateRandomSquad(_squad);

                var strs = pl.ShipDefinitions.Where(t => (t.Value.ShipProperties & ShipProperties.GroundStructure) != 0).ToList();
                var s = GameManager.Generator.Next(0, strs.Count);
                var sh = pl.AddShip(_squad, strs[s].Key);
                _squad.Deploy(sh as Structure, this);

                // populate structure
                // pl.AddSoldiers(_structure)

                if(_team != HumanPlayer.Instance.Team && _team != Team.Indigenous)
                {
                    var sq = pl.CreateNewSquad(_squad);
                    ((AIPlayer)pl).PopulateRandomSquad(sq);
                    ((AIPlayer)pl).RegisterDefensiveSquad(sector, this, sq);
                    // pl.CreateChaseEvent(sq, HumanPlayer.Instance.CommandSquad, sector, this, 50.0f, 25f);
                }
            }
        }

        GameManager.Instance.Players[_team].ClaimTile(this);

        var circle = Resources.Load(CIRCLE_PREFAB);
        _circle = GameObject.Instantiate(circle, this.transform.position, Quaternion.Euler(90f, 0, 0)) as GameObject;
        _circle.transform.localScale = new Vector3(_radius * 2 + 0.5f, _radius * 2 + 0.5f, _radius * 2 + 0.5f);
        _circle.transform.parent = this.transform.parent;
        _circle.GetComponent<Renderer>().material.SetColor("_Color", GameManager.Instance.PlayerColors[_team]);
    }

	/// <summary>
	/// Enables or disables the rendered if visible/not visible on screen.
	/// </summary>
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

    /// <summary>
    /// Claims this tile for a specified team.
    /// </summary>
    /// <param name="team">The new owning team.</param>
    public void Claim(Team team)
    {
        _circle.GetComponent<Renderer>().material.SetColor("_Color", GameManager.Instance.PlayerColors[team]);
        GameManager.Instance.Players[_team].RelinquishTile(this);

        if (_team == Team.Plinthen)
        {
            _resourceCount -= Mathf.CeilToInt(_resourceCount * 1.15f);
            _resourceCount = (_resourceCount < 0 ? 0 : _resourceCount);
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

        HumanPlayer.Instance.Control(HumanPlayer.Instance.Squad.gameObject);
    }

    /// <summary>
    /// Undeploy this tile's structure.
    /// </summary>
    /// <param name="destroyStructure">Indicate whether to destroy the structure after undeploying.</param>
    /// <returns>The planet type (for considering the undeploy type. i.e., undeploying a space structure requires extra work.)</returns>
    public string Undeploy(bool destroyStructure)
    {
        if (_structure == null)
            return "";

        _squad.Ships.Add(_structure);
        _structure.Undeploy(this);

        if (destroyStructure)
            GameManager.Instance.Players[_team].RemoveShip(_squad, _structure);

        if(MapManager.Instance.DeploySpawnTable.ContainsKey(_planetType))
        {
            _squad.Sector.UnregisterSpaceStructure(_team, _structure);
            _squad.Sector.DeleteTile(this);
        }

        _structure = null;

        return _planetType;
    }

    /// <summary>
    /// Deploy a specified ship to this tile.
    /// </summary>
    /// <param name="ship">The ship to deploy.</param>
    /// <param name="team">The team the ship belongs to.</param>
    public void Deploy(Structure ship, Team team)
    {
        if(_team != team)
            Claim(team);
        _structure = ship;
        _structure.Deploy(this);
    }

    /// <summary>
    /// Gather resources from this planet (if necessary) and grow the planet's population.
    /// </summary>
    public void GatherAndGrow()
    {
        if (_structure == null)
        {
            _population += Mathf.CeilToInt(_population * 0.05f);
            return;
        }
        else if(_planetInhabitance != Inhabitance.Uninhabited)
        {
            _population += Mathf.CeilToInt((_population + _structure.Population[_planetInhabitance]) * 0.05f);
        }

        var gathered = _structure.Gather(_resourceType, _resourceCount, _planetInhabitance, _population, _team);

        foreach (var resource in gathered)
        {
            switch (resource.Key)
            {
                case ResourceGatherType.None:
                case ResourceGatherType.Soldiers:
                case ResourceGatherType.Research:
                    break;
                case ResourceGatherType.Natural:
                    _resourceCount -= resource.Value;
                    break;
            }
        }
    }
    
    /// <summary>
    /// Calculate this planet's defensive power.
    /// </summary>
    /// <returns>The defensive power.</returns>
    public float CalculateDefensivePower()
    {
        var power = 0f;
        var bonuses = new Dictionary<Inhabitance, float>()
        {
            { Inhabitance.Primitive, 1f },
            { Inhabitance.Industrial, 1.5f },
            { Inhabitance.SpaceAge, 1.75f }
        };
        
        if(_planetInhabitance != Inhabitance.Uninhabited)
            power += _population * bonuses[_planetInhabitance];
        if (_structure != null)
        {
            foreach(var bonus in bonuses)
                power += _structure.Population[bonus.Key] * bonus.Value;
        }

        return power * (_team == Team.Kharkyr ? 1.15f : 1f);
    }

    /// <summary>
    /// Create a diplomatic effort at this planet.
    /// </summary>
    /// <param name="team">The team attempting diplomacy.</param>
    public void SetDiplomaticEffort(Team team)
    {
        if (!_diplomacy.ContainsKey(team))
            _diplomacy.Add(team, true);
        else
            _diplomacy[team] = true;
    }

    /// <summary>
    /// Remove a diplomatic effort at this planet.
    /// </summary>
    /// <param name="team">The team ending diplomacy.</param>
    public void EndDiplomaticEffort(Team team)
    {
        _diplomacy[team] = false;
    }

    /// <summary>
    /// Populate a UI panel detailing this planet's information.
    /// </summary>
    /// <param name="panel">The panel to populate.</param>
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

    /// <summary>
    /// Checks if a squad is within interaction distance.
    /// </summary>
    /// <param name="squad">The squad in consideration.</param>
    /// <returns>Bool indicating the squad is in range.</returns>
    public bool IsInRange(Squad squad)
    {
        return (squad.transform.position - transform.position).sqrMagnitude <= (_radius * _radius);
    }

    /// <summary>
    /// Checks if a raycasted point is close enough to be considered as a click.
    /// </summary>
    /// <param name="click">The raycast hit position on the parent sector.</param>
    /// <returns>Bool indicating a successful click.</returns>
    public bool IsInClickRange(Vector3 click)
    {
        return (click - transform.position).sqrMagnitude <= (_clickRadius * _clickRadius);
    }

    /// <summary>
    /// Populates a list with this planet's soldier counts, including the structure.
    /// </summary>
    /// <param name="list">The UI list to populate.</param>
    /// <param name="bType">The battle type to consider.</param>
    public void PopulateCountList(GameObject list, BattleType bType)
    {
        var squadEntry = Resources.Load<GameObject>(SQUAD_COUNT_PREFAB);

        var counts = new Dictionary<string, int>()
        {
            { _planetInhabitance.ToString(), _population }
        };

        if (_planetInhabitance == Inhabitance.Uninhabited)
            counts.Remove(_planetInhabitance.ToString());

        if(_structure != null)
            foreach (var i in _structure.Population)
            {
                if (!counts.ContainsKey(i.Key.ToString()) && i.Value > 0)
                    counts.Add(i.Key.ToString(), i.Value);
                else if (counts.ContainsKey(i.Key.ToString()))
                    counts[i.Key.ToString()] += i.Value;
            }

        foreach (var count in counts)
        {
            var entry = Instantiate(squadEntry) as GameObject;
            entry.transform.FindChild("Name").GetComponent<Text>().text = count.Key;
            entry.transform.FindChild("Icon").GetComponent<Image>().sprite = GUIManager.Instance.Icons[count.Key];
            entry.transform.FindChild("Count").FindChild("Number").GetComponent<Text>().text = count.Value.ToString();
            entry.transform.SetParent(list.transform);
        }
    }

    /// <summary>
    /// Creates a list entry with this planet's basic info.
    /// </summary>
    /// <param name="listName">The internal list name.</param>
    /// <param name="index">The index in the list.</param>
    /// <param name="data">Optional data.</param>
    /// <returns>The new list item.</returns>
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
