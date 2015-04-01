using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GUIManager : MonoBehaviour 
{
    private static GUIManager _instance;
    private Dictionary<string, CustomUI> _interface;
    private Dictionary<string, Sprite> _icons;

    private Dictionary<string, int> _selectedIndices;

    // when done with GUIManager, add a ton more of these
    private const string UI_ICONS_PATH = "UI Icons/";

    public Dictionary<string, Sprite> Icons { get { return _icons; } }
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

        _selectedIndices = new Dictionary<string, int>()
        {
            { "MainShipList", -1 },
            { "Constructables", -1 },
            { "AltSquadList", -1 },
            { "AltShipList", -1 },
            { "SelectedShipList", -1 }
        };

        _icons = new Dictionary<string, Sprite>()
        {
            { "Oil", Resources.Load<Sprite>(UI_ICONS_PATH + "OilIcon") },
            { "Asterminium", Resources.Load<Sprite>(UI_ICONS_PATH + "AsterminiumIcon") },
            { "Ore", Resources.Load<Sprite>(UI_ICONS_PATH + "OreIcon") },
            { "Forest", Resources.Load<Sprite>(UI_ICONS_PATH + "ForestIcon") },
            { "NoResource", Resources.Load<Sprite>(UI_ICONS_PATH + "NoResourceIcon") },
            { "Indigineous", Resources.Load<Sprite>(UI_ICONS_PATH + "IndigineousIcon") },
            { "Uninhabited", Resources.Load<Sprite>(UI_ICONS_PATH + "UninhabitedIcon") },
            { "Plinthen", Resources.Load<Sprite>(UI_ICONS_PATH + "PlinthenIcon") },
            { "Union", Resources.Load<Sprite>(UI_ICONS_PATH + "UnionIcon") },
            { "Kharkyr", Resources.Load<Sprite>(UI_ICONS_PATH + "KharkyrIcon") }
        };
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
        var manage = _interface["Manage"].gameObject.GetComponent<Button>();
        var deploy = _interface["Deploy"].gameObject.GetComponent<Button>();

        if (HumanPlayer.Instance.Team == squad.Team)
        {
            manage.interactable = squad.Size > 0 || squad2 != null || (squad2 != null && tile.Team == squad.Team);
            deploy.interactable = (_selectedIndices["MainShipList"] != -1 && squad.Ships[_selectedIndices["MainShipList"]].ShipType == ShipType.Structure && tile != null && tile.Structure == null) || 
                (squad.GetComponent<Tile>() != null && squad.GetComponent<Tile>().Structure != null);
        }
        else
        {
            manage.interactable = false;
            deploy.interactable = false;
        }
    }

    public void SetUIElements(bool squad, bool battle, bool tile, bool manage)
    {
        _interface["SquadMenu"].gameObject.SetActive(squad);
        _interface["BattleMenu"].gameObject.SetActive(battle);
        _interface["PlanetInfo"].gameObject.SetActive(tile);
        _interface["ManageMenu"].gameObject.SetActive(manage);
    }

    public void ItemClicked(string data)
    {
        var split = data.Split('|');
        _selectedIndices[split[0]] = int.Parse(split[1]);

        switch(split[0])
        {
            case "MainShipList":
            case "AltShipList":
            case "SelectedShipList":
                break;
            case "AltSquadList":
                UpdateTransferInterface(false, true, false);
                break;
        }
    }

    public void PopulateManageLists()
    {
        GUIManager.Instance.SetUIElements(false, false, false, true);
        _selectedIndices["MainShipList"] = -1;
        ClearList("MainShipList");
        UpdateTransferInterface(true, true, true);
    }

    private void ClearList(string indexName)
    {
        foreach (Transform child in _interface[indexName].transform)
            GameObject.Destroy(child.gameObject);
    }

    private void PopulateList<T>(List<T> source, string indexName, ListingType type, System.Object data) where T : ListableObject
    {
        ClearList(indexName);

        for(var i = 0; i < source.Count; i++)
        {
            GameObject entry = null;
            switch (type)
            {
                case ListingType.Info:
                    entry = source[i].CreateListEntry(indexName, i, data);
                    break;
                case ListingType.Build:
                    entry = source[i].CreateBuildListEntry(indexName, i, data);
                    break;
            }

            if(entry != null)
                entry.transform.SetParent(_interface[indexName].transform);
        }

        AutoSelectIndex<T>(indexName, source);
    }

    private void AutoSelectIndex<T>(string name, List<T> list)
    {
        if (list.Count == 0)
            _selectedIndices[name] = -1;
        else if (_selectedIndices[name] > list.Count)
            _selectedIndices[name] = list.Count - 1;
        else if (_selectedIndices[name] < 0)
            _selectedIndices[name] = 0;
    }

    public void SquadSelected(Squad squad)
    {
        var keys = new List<string>(_selectedIndices.Keys);
        foreach (var index in keys)
            _selectedIndices[index] = -1;

        PopulateList<Ship>(squad.Ships, "MainShipList", ListingType.Info, false);

        _interface["DeployText"].GetComponent<Text>().text = "Deploy";
        _interface["Deploy"].GetComponent<CustomUI>().data = "Deploy";
        SetUIElements(true, false, false, false);
        SetMainListControls(squad, null, null);
    }

    public void TileSelected(Tile tile, int playerStations, Dictionary<string, Ship> defs)
    {
        var squad = tile.gameObject.GetComponent<Squad>();
        SquadSelected(squad);

        tile.PopulateInfoPanel(_interface["PlanetInfo"].gameObject);

        // move to structure
        var playerStructure = tile.Team == HumanPlayer.Instance.Team && tile.Structure != null;
        var deployText = playerStructure ? "Undeploy" : "Deploy";

        _interface["ConstBar"].gameObject.SetActive(playerStructure);
        _interface["Structure"].gameObject.SetActive(playerStructure);
        _interface["ConstructionList"].gameObject.SetActive(playerStructure);
        _interface["DeployText"].GetComponent<Text>().text = deployText;
        _interface["Deploy"].GetComponent<CustomUI>().data = deployText;

        if (playerStructure)
        {
            // populate structure info
            tile.Structure.Resources.Remove(Resource.Stations);
            tile.Structure.Resources.Add(Resource.Stations, playerStations);
            tile.Structure.PopulateStructurePanel(_interface["Structure"].gameObject);

            var buildList = new List<Ship>();
            foreach(var construct in tile.Structure.Constructables)
                buildList.Add(defs[construct]);
            PopulateList<Ship>(buildList, "Constructables", ListingType.Build, tile.Structure.Resources);
        }

        SetUIElements(true, false, true, false);
        SetMainListControls(squad, null, tile);
    }

    public void SetStructure(bool remove)
    {
        if (remove)
            HumanPlayer.Instance.Undeploy();
        else
            HumanPlayer.Instance.Deploy(_selectedIndices["MainShipList"]);
    }

    private void UpdateTransferInterface(bool squads, bool squadShips, bool ships)
    {
        if(squads)
            PopulateList<Squad>(HumanPlayer.Instance.Squad.Colliders, "AltSquadList", ListingType.Info, true);
        if (squadShips && _selectedIndices["AltSquadList"] > -1 && _selectedIndices["AltSquadList"] < HumanPlayer.Instance.Squad.Colliders.Count)
           PopulateList<Ship>(HumanPlayer.Instance.Squad.Colliders[_selectedIndices["AltSquadList"]].Ships, "AltShipList", ListingType.Info, true);
        if(ships)
            PopulateList<Ship>(HumanPlayer.Instance.Squad.Ships, "SelectedShipList", ListingType.Info, true);

        var right = _interface["Right"].GetComponent<Button>();
        var dright = _interface["DoubleRight"].GetComponent<Button>();
        var left = _interface["Left"].GetComponent<Button>();
        var dleft = _interface["DoubleLeft"].GetComponent<Button>();

        if (_selectedIndices["AltSquadList"] == -1)
            right.interactable = left.interactable = dright.interactable = dleft.interactable = false;
        else
        {
            right.interactable = _selectedIndices["AltShipList"] == -1 ? false : true;
            left.interactable = _selectedIndices["SelectedShipList"] == -1 ? false : true;
            dright.interactable = _selectedIndices["AltShipList"] == -1 ? false : true;
            dleft.interactable = _selectedIndices["SelectedShipList"] == -1 ? false : true;
        }
    }

    public Ship Transfer(Squad from, string fromName, Squad to)
    {
        var fromIndex = _selectedIndices[fromName];
        Ship ship = from.RemoveShip(fromIndex);

        switch (ship.ShipType)
        {
            case ShipType.CommandShip:
            case ShipType.Defense:
                return ship;
        }

        to.AddShip(ship);
        AutoSelectIndex<Ship>(fromName, from.Ships);
        return null;
    }

    public void TransferAll(Squad from, string fromName, Squad to)
    {
        var untransferables = new List<Ship>();
        while(from.Ships.Count > 0)
        {
            var ship = Transfer(from, fromName, to);
            if (ship != null)
                untransferables.Add(ship);
        }

        from.Ships.AddRange(untransferables);
    }

    public void TransferToControlledSquad()
    {
        var index = _selectedIndices["AltSquadList"];
        var squad = HumanPlayer.Instance.Squad.Colliders[index];
        var ship = Transfer(squad, "AltShipList", HumanPlayer.Instance.Squad);
        if (ship != null) squad.AddShip(ship, index);
        UpdateTransferInterface(false, true, true);
    }

    public void TransferToSelectedSquad()
    {
        var index = _selectedIndices["SelectedShipList"];
        var squad = HumanPlayer.Instance.Squad;
        var ship = Transfer(squad, "SelectedShipList", HumanPlayer.Instance.Squad.Colliders[_selectedIndices["AltSquadList"]]);
        if (ship != null) squad.AddShip(ship, index);
        UpdateTransferInterface(false, true, true);
    }

    public void TransferAllToControlledSquad()
    {
        TransferAll(HumanPlayer.Instance.Squad.Colliders[_selectedIndices["AltSquadList"]], "AltShipList", HumanPlayer.Instance.Squad);
        UpdateTransferInterface(false, true, true);
    }

    public void TransferAllToSelectedSquad()
    {
        TransferAll(HumanPlayer.Instance.Squad, "SelectedShipList", HumanPlayer.Instance.Squad.Colliders[_selectedIndices["AltSquadList"]]);
        UpdateTransferInterface(false, true, true);
    }

    public void NewSquad()
    {
        var squad = HumanPlayer.Instance.CreateNewSquad();
        var listing = (ListableObject)squad;

        listing.CreateListEntry("AltSquadList", HumanPlayer.Instance.Squad.Colliders.Count - 1, true).transform.SetParent(_interface["AltSquadList"].transform);
    }

    public void ExitManage()
    {
        HumanPlayer.Instance.CleanSquad(HumanPlayer.Instance.Squad);
        HumanPlayer.Instance.ReloadGameplayUI();
        ClearList("AltSquadList");
        ClearList("AltShipList");
        ClearList("SelectedShipList");
        AutoSelectIndex<Ship>("MainShipList", HumanPlayer.Instance.Squad.Ships);
    }

    public void Battle()
    {
        var winner = HumanPlayer.Instance.Battle();
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
            SetUIElements(false, true, false, false);
        }
    }

    public void SquadCollideTile(Squad playerSquad, Tile tile) // planet defenses assumed empty here
    {
        if(tile.Team == playerSquad.Team)
        {
            SetUIElements(true, false, false, false);
            SetMainListControls(playerSquad, tile.Squad, tile);
        }
        else
        {
            SetUIElements(false, true, false, false);
        }
    }
}
