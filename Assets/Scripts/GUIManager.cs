using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GUIManager : MonoBehaviour 
{
    private static GUIManager _instance;
    private Dictionary<string, CustomUI> _interface;
    private int _listIndex;
    private Squad _selectedSquad;

    private const string LIST_PREFAB = "ShipListing";

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

        if (Player.Instance.Team == squad.Team)
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
    }

    public void SquadSelected(Squad squad)
    {
        _listIndex = -1;

        var listEntry = Resources.Load<GameObject>(LIST_PREFAB);
        foreach (Transform child in _interface["MainShipList"].transform) 
        {
            GameObject.Destroy(child.gameObject);
        }

        int i = 0;
        foreach(var ship in squad.Ships)
        {
            var entry = Instantiate(listEntry) as GameObject;
            // icon
            entry.transform.FindChild("Name").GetComponent<Text>().text = ship.Name;
            // population icon will be static
            entry.transform.FindChild("Population").GetComponent<Text>().text = ship.Population + " / " + ship.Capacity;
            entry.GetComponent<CustomUI>().data = i.ToString();
            entry.transform.SetParent(_interface["MainShipList"].transform);
            i++;
        }

        _interface["Deploy"].gameObject.transform.FindChild("Text").GetComponent<Text>().text = "Deploy";
        SetUIElements(true, false, false);
        SetMainListControls(squad, null, null);
    }

    public void TileSelected(Tile tile)
    {
        var squad = tile.gameObject.GetComponent<Squad>();
        SquadSelected(squad);

        if (tile.Team == Player.Instance.Team)
        {
        }
        else // enemy
        {
        }

        if(tile.DeployedStructure != null && tile.Team == Player.Instance.Team)
        {
            _interface["Deploy"].gameObject.transform.FindChild("Text").GetComponent<Text>().text = "Undeploy";   
        }
        SetUIElements(true, false, true);
        SetMainListControls(squad, null, tile);
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
        if (tile.Team == Player.Instance.Team) // ask to confirm, cancel
        {

        }
        else // enemy - ask to confirm move order, cancel, etc
        {

        }
    }

    // note: "squad2" isn't necessarily hostile. Handle that stuff here.
    public void SquadToSquad(Squad playerSquad, Squad squad2)
    {
        if (squad2.Team == Player.Instance.Team) // ask to confirm movement, etc
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
