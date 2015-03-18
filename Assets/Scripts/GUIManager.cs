using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GUIManager : MonoBehaviour 
{
    private static GUIManager _instance;
    private Dictionary<string, CustomUI> _interface;

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

    private void SetUIElements(bool squad, bool battle, bool tile)
    {
        _interface["SquadMenu"].gameObject.SetActive(squad);
        _interface["BattleMenu"].gameObject.SetActive(battle);
    }

    public void SquadSelected(Squad squad)
    {
        var listEntry = Resources.Load<GameObject>(LIST_PREFAB);
        foreach (Transform child in _interface["MainShipList"].transform) 
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach(var ship in squad.Ships)
        {
            var entry = Instantiate(listEntry) as GameObject;
            // icon
            entry.transform.FindChild("Name").GetComponent<Text>().text = ship.Name;
            // population icon will be static
            entry.transform.FindChild("Population").GetComponent<Text>().text = ship.Population + " / " + ship.Capacity;
            entry.transform.parent = _interface["MainShipList"].transform;
        }

        if(squad.Team == Player.Instance.Team)
        {
            SetUIElements(true, false, false);
        }
        else // enemy
        {
            SetUIElements(true, false, false);
        }
    }

    public void TileSelected(Tile tile)
    {
        if (tile.Team == Player.Instance.Team)
        {
            SetUIElements(false, false, true);
        }
        else // enemy
        {
            SetUIElements(false, false, true);
        }
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
        if (squad2.Team == Player.Instance.Team) // ask to merge, exchange units, deploy (if tile), cancel
        {
        }
        else // enemy - show combat screen
        {
            SetUIElements(false, true, false);
        }
    }

    public void SquadCollideTile(Squad playerSquad, Tile tile) // planet defenses assumed empty here
    {
        if (tile.Team == Player.Instance.Team) // ask to merge, exchange units, deploy (if tile), cancel
        {
            _interface["Merge"].enabled = true;
        }
        else // enemy - show combat screen
        {
            SetUIElements(false, true, false);
        }
    }
}
