using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GUIManager : MonoBehaviour 
{
    private static GUIManager _instance;
    private Dictionary<string, CustomUI> _interface;

    private int _listIndex; // find way to phase this out - list.selectedindex?

    private const string LIST_PREFAB = "ShipListing";
    private const string CONSTRUCT_PREFAB = "Constructable";

    public static GUIManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<GUIManager>();
            return _instance;
        }
    }
    public int ListIndex 
    {
        get { return _listIndex; }
        set { _listIndex = value; }
    }

    void Start()
    {
        _instance = this;

        if(_interface == null)
            _interface = new Dictionary<string, CustomUI>();
    }

    void Update()
    {

    }

    public void Register(string name, CustomUI btn)
    {
        if (_interface == null)
            _interface = new Dictionary<string, CustomUI>();

        if(!_interface.ContainsKey(name))
            _interface.Add(name, btn);
    }

    //
    // These functions update main UI when the object is selected
    //

    public void SetMainListControls(Squad squad, Squad squad2, Tile tile)
    {
        var merge = _interface["Merge"].gameObject.GetComponent<Button>();
        var split = _interface["Split"].gameObject.GetComponent<Button>();
        var deploy = _interface["Deploy"].gameObject.GetComponent<Button>();

        if (HumanPlayer.Instance.Team == squad.Team)
        {
            split.interactable = squad.Size > 0;
            merge.interactable = squad2 != null || (squad2 != null && tile.Team == squad.Team);
            deploy.interactable = (_listIndex != -1 && squad.Ships[_listIndex].ShipType == ShipType.Structure && tile != null && tile.DeployedStructure == null) || 
                (squad.GetComponent<Tile>() != null && squad.GetComponent<Tile>().DeployedStructure != null);
        }
        else
        {
            split.interactable = false;
            merge.interactable = false;
            deploy.interactable = false;
        }
    }

    public void SetUIElements(bool squad, bool battle, bool tile)
    {
        _interface["SquadMenu"].gameObject.SetActive(squad);
        _interface["BattleMenu"].gameObject.SetActive(battle);
        _interface["PlanetInfo"].gameObject.SetActive(tile);
    }

    public void SquadSelected(Squad squad)
    {
        _listIndex = -1;

        var listEntry = Resources.Load<GameObject>(LIST_PREFAB);
        foreach (Transform child in _interface["MainShipList"].transform) 
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < squad.Ships.Count; i++ )
        {
            var entry = Instantiate(listEntry) as GameObject;
            var icon = entry.transform.FindChild("Icon").GetComponent<Image>();
            icon.sprite = squad.Ships[i].Icon;
            entry.transform.FindChild("Name").GetComponent<Text>().text = squad.Ships[i].Name;
            entry.transform.FindChild("Population").GetComponent<Text>().text = squad.Ships[i].Population + " / " + squad.Ships[i].Capacity;
            entry.GetComponent<CustomUI>().data = i.ToString();
            entry.transform.SetParent(_interface["MainShipList"].transform);
        }

        _interface["DeployText"].GetComponent<Text>().text = "Deploy";
        SetUIElements(true, false, false);
        SetMainListControls(squad, null, null);
    }

    public void TileSelected(Tile tile, Dictionary<string, Ship> defs)
    {
        var squad = tile.gameObject.GetComponent<Squad>();
        SquadSelected(squad);

        // general planet stuff here
        var tileRenderer = tile.GetComponent<ParticleSystem>().GetComponent<Renderer>();
        var uiRenderer = _interface["PlanetIcon"].GetComponent<RawImage>();
        uiRenderer.texture = tileRenderer.material.mainTexture;
        uiRenderer.uvRect = new Rect(tileRenderer.material.mainTextureOffset.x,
                                     tileRenderer.material.mainTextureOffset.y,
                                     tileRenderer.material.mainTextureScale.x,
                                     tileRenderer.material.mainTextureScale.y);
        _interface["PlanetName"].GetComponent<Text>().text = tile.Name;
        // resource name
        _interface["TeamName"].GetComponent<Text>().text = tile.Team.ToString();
        // resource icon
        _interface["ResourceName"].GetComponent<Text>().text = tile.ResourceType.ToString() + "\n" + tile.ResourceCount.ToString();
        _interface["Population"].GetComponent<Text>().text = tile.Population.ToString();
        // population types

        if (tile.Team == HumanPlayer.Instance.Team && tile.DeployedStructure != null)
        {
            _interface["ConstBar"].gameObject.SetActive(true);
            _interface["Structure"].gameObject.SetActive(true);
            _interface["ConstructionList"].gameObject.SetActive(true);
            _interface["DeployText"].GetComponent<Text>().text = "Undeploy";

            // populate structure info
            _interface["StructureIcon"].GetComponent<Image>().sprite = tile.DeployedStructure.Icon;
            _interface["StructureName"].GetComponent<Text>().text = tile.DeployedStructure.Name;
            _interface["Capacity"].GetComponent<Text>().text = tile.DeployedStructure.Population.ToString()
                + " / " + tile.DeployedStructure.DeployedCapacity.ToString();
            _interface["Defense"].GetComponent<Text>().text = tile.DeployedStructure.Defense.ToString();
            _interface["GatherRate"].GetComponent<Text>().text = tile.DeployedStructure.GatherRate.ToString();

            var listEntry = Resources.Load<GameObject>(CONSTRUCT_PREFAB);
            foreach (Transform child in _interface["Constructables"].transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            // populate construction list
            foreach(var construct in tile.DeployedStructure.Constructables)
            {
                var entry = Instantiate(listEntry) as GameObject;
                entry.transform.FindChild("Name").GetComponent<Text>().text = defs[construct].Name;
                entry.transform.FindChild("Icon").GetComponent<Image>().sprite = defs[construct].Icon;
                entry.transform.FindChild("HullText").GetComponent<Text>().text = defs[construct].Hull.ToString();
                entry.transform.FindChild("FirepowerText").GetComponent<Text>().text = defs[construct].Firepower.ToString();
                entry.transform.FindChild("SpeedText").GetComponent<Text>().text = defs[construct].Speed.ToString();
                entry.transform.FindChild("CapacityText").GetComponent<Text>().text = defs[construct].Capacity.ToString();
                entry.GetComponent<CustomUI>().data = defs[construct].Name;
                entry.transform.SetParent(_interface["Constructables"].transform);

                if (defs[construct].Unlocked)
                    entry.GetComponent<Button>().interactable = true;
            }
        }
        else // enemy or no structure
        {
            _interface["ConstBar"].gameObject.SetActive(false);
            _interface["Structure"].gameObject.SetActive(false);
            _interface["ConstructionList"].gameObject.SetActive(false);
            _interface["DeployText"].GetComponent<Text>().text = "Undeploy";
        }

        SetUIElements(true, false, true);
        SetMainListControls(squad, null, tile);
    }

    public void UpdateSelectedPlanet(Tile tile, Dictionary<string, Ship> defs)
    {
        // probably won't need this if we just call SelectedTile and SelectedSquad again when the turn begins
        // update planet info itself
        // use CustomUI data to update from the appropriate research tree
    }

    //
    // These functions update and display the menu when a command is given
    //

    // Notes: Tiles have squads, but they may be empty.
    // Tiles and squads should check for enemy/friendly before choosing how to make the new menu
    public void SquadToSpace(Squad squad, Vector3 coords)
    {
        // ask to confirm/cancel movement
    }

    public void SquadToTile(Squad squad, Tile tile)
    {
        if (tile.Team == HumanPlayer.Instance.Team) // ask to confirm, cancel
        {

        }
        else // enemy - ask to confirm move order, cancel, etc
        {

        }
    }

    // note: "squad2" isn't necessarily hostile. Handle that stuff here.
    public void SquadToSquad(Squad playerSquad, Squad squad2)
    {
        if (squad2.Team == HumanPlayer.Instance.Team) // ask to confirm movement, etc
        {

        }
        else // enemy - ask to chase, ignore, cancel, etc
        {

        }
    }

    //
    // These functions update and enable UI when squads collide with the appropriate object
    //
    public void SquadCollideSquad(Squad playerSquad, Squad squad2)
    {
        if (playerSquad.Team == squad2.Team)
        {
            SetMainListControls(playerSquad, squad2, null);
        }
        else
        {
            SetUIElements(false, true, false);
        }
    }

    public void SquadCollideTile(Squad playerSquad, Tile tile) // planet defenses assumed empty here
    {
        if(tile.Team == playerSquad.Team)
        {
            SetUIElements(true, false, false);
            SetMainListControls(playerSquad, tile.Squad, tile);
        }
        else
        {
            SetUIElements(false, true, false);
        }
    }
}
