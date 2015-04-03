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
            {
                _instance = GameObject.FindObjectOfType<GUIManager>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;

        if (_interface == null)
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

    void Start()
    {
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
    public void SetSquadControls(Squad squad)
    {
        if (_interface["SquadMenu"].gameObject.activeInHierarchy == false)
            return;

        var manage = _interface["Manage"].gameObject.GetComponent<Button>();
        var deploy = _interface["Deploy"].gameObject.GetComponent<Button>();
        var type = deploy.GetComponent<CustomUI>().data == "Deploy";
        var tile = squad.Tile;

        AutoSelectIndex<Ship>("MainShipList", squad.Ships);
        var index = _selectedIndices["MainShipList"];

        // note: i fucking hate this bit
        var click = false;
        if (HumanPlayer.Instance.Team == squad.Team)
        {
            if(type && index != -1) // deploy
            {
                if (tile != null && (squad.Ships[index].ShipProperties & ShipProperties.GroundStructure) > 0 
                    && squad.IsInPlanetRange && tile.Team == squad.Team && tile.Structure == null) // existing planet
                    click = true;
                else if (tile == null && (squad.Ships[index].ShipProperties & ShipProperties.SpaceStructure) > 0)// empty space
                    click = true;
            }
            else if(tile != null && squad == tile.Squad && tile.Structure != null)// undeploy
                click = true;
        }

        deploy.interactable = click;
        manage.interactable = (squad.Ships.Count > 0 || squad.Colliders.Count > 0) && squad.Team == HumanPlayer.Instance.Team;
    }

    public void SetUIElements(bool squad, bool battle, bool win, bool lose, bool tile, bool manage)
    {
        _interface["SquadMenu"].gameObject.SetActive(squad);
        _interface["BattleMenu"].gameObject.SetActive(battle);
        _interface["BattleWon"].gameObject.SetActive(win);
        _interface["BattleLost"].gameObject.SetActive(lose);
        _interface["PlanetInfo"].gameObject.SetActive(tile);
        _interface["ManageMenu"].gameObject.SetActive(manage);
    }

    public void ItemClicked(string data)
    {
        var split = data.Split('|');
        switch(split[0])
        {
            case "MainShipList":
            case "AltShipList":
                _selectedIndices[split[0]] = int.Parse(split[1]);
                UpdateTransferInterface(false, true, false);
                break;
            case "SelectedShipList":
                _selectedIndices[split[0]] = int.Parse(split[1]);
                UpdateTransferInterface(false, false, true);
                break;
            case "AltSquadList":
                _selectedIndices[split[0]] = int.Parse(split[1]);
                UpdateTransferInterface(false, true, false);
                break;
            case "Constructables":
                HumanPlayer.Instance.CreateBuildEvent(split[1]);
                break;
        }
    }

    
    public void UIHighlighted(string data)
    {
        var split = data.Split('|');
        switch (split[0])
        {
            case "MainShipList":
            case "AltShipList":
                break;
            case "SelectedShipList":
                break;
            case "AltSquadList":
                break;
            case "Constructables":
                break;
        }
    }

    public void PopulateManageLists()
    {
        GUIManager.Instance.SetUIElements(false, false, false, false, false, true);
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
    }

    private void AutoSelectIndex<T>(string name, List<T> list)
    {
        if (list.Count == 0)
            _selectedIndices[name] = -1;
        else if (_selectedIndices[name] >= list.Count)
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
        SetUIElements(true, false, false, false, false, false);
        SetSquadControls(squad);
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

        SetUIElements(true, false, false, false, true, false);
        SetSquadControls(squad);
    }

    public void SetStructure(string data)
    {
        if (data == "Deploy")
            HumanPlayer.Instance.CreateDeployEvent(_selectedIndices["MainShipList"]);
        else
            HumanPlayer.Instance.CreateUndeployEvent(false);
    }

    private void UpdateTransferInterface(bool squads, bool squadShips, bool ships)
    {
        var squad = HumanPlayer.Instance.Squad;
        AutoSelectIndex<Squad>("AltSquadList", squad.Colliders);
        if (_selectedIndices["AltSquadList"] != -1)
            AutoSelectIndex<Ship>("AltShipList", squad.Colliders[_selectedIndices["AltSquadList"]].Ships);
        AutoSelectIndex<Ship>("SelectedShipList", squad.Ships);

        if(squads)
            PopulateList<Squad>(squad.Colliders, "AltSquadList", ListingType.Info, true);
        if (squadShips && _selectedIndices["AltSquadList"] > -1 && _selectedIndices["AltSquadList"] < squad.Colliders.Count)
            PopulateList<Ship>(squad.Colliders[_selectedIndices["AltSquadList"]].Ships, "AltShipList", ListingType.Info, true);
        if(ships)
            PopulateList<Ship>(squad.Ships, "SelectedShipList", ListingType.Info, true);

        var right = _interface["Right"].GetComponent<Button>();
        var dright = _interface["DoubleRight"].GetComponent<Button>();
        var left = _interface["Left"].GetComponent<Button>();
        var dleft = _interface["DoubleLeft"].GetComponent<Button>();

        var altsquad = _selectedIndices["AltSquadList"];
        var altships = _selectedIndices["AltShipList"];
        var selected = _selectedIndices["SelectedShipList"];

        if (altsquad == -1)
            right.interactable = left.interactable = dright.interactable = dleft.interactable = false;
        else
        {
            right.interactable = altships != -1 &&
                (HumanPlayer.Instance.Squad.Colliders[altsquad].Ships[altships].ShipProperties & ShipProperties.Untransferable) == 0 ? true : false;
            left.interactable = selected != -1  &&
                (HumanPlayer.Instance.Squad.Ships[selected].ShipProperties & ShipProperties.Untransferable) == 0 ? true : false;
            dright.interactable = altships == -1 ? false : true;
            dleft.interactable = selected == -1 ? false : true;
        }
    }

    public Ship Transfer(Squad from, string fromName, Squad to, string toName)
    {
        var fromIndex = _selectedIndices[fromName];
        Ship ship = from.RemoveShip(fromIndex);

        if((ship.ShipProperties & ShipProperties.Untransferable) > 0)
            return ship;

        to.AddShip(ship);
        return null;
    }

    public void TransferAll(Squad from, string fromName, Squad to, string toName)
    {
        var untransferables = new List<Ship>();
        while(from.Ships.Count > 0)
        {
            AutoSelectIndex<Ship>(fromName, from.Ships);
            var ship = Transfer(from, fromName, to, toName);
            if (ship != null)
                untransferables.Add(ship);
        }

        from.Ships.AddRange(untransferables);
    }

    public void TransferToControlledSquad()
    {
        var index = _selectedIndices["AltSquadList"];
        var squad = HumanPlayer.Instance.Squad.Colliders[index];
        var ship = Transfer(squad, "AltShipList", HumanPlayer.Instance.Squad, "SelectedShipList");
        if (ship != null) squad.AddShip(ship, index);
        UpdateTransferInterface(false, true, true);
    }

    public void TransferToSelectedSquad()
    {
        var index = _selectedIndices["SelectedShipList"];
        var squad = HumanPlayer.Instance.Squad;
        var ship = Transfer(squad, "SelectedShipList", HumanPlayer.Instance.Squad.Colliders[_selectedIndices["AltSquadList"]], "AltShipList");
        if (ship != null) squad.AddShip(ship, index);
        UpdateTransferInterface(false, true, true);
    }

    public void TransferAllToControlledSquad()
    {
        TransferAll(HumanPlayer.Instance.Squad.Colliders[_selectedIndices["AltSquadList"]], "AltShipList", HumanPlayer.Instance.Squad, "SelectedShipList");
        UpdateTransferInterface(false, true, true);
    }

    public void TransferAllToSelectedSquad()
    {
        TransferAll(HumanPlayer.Instance.Squad, "SelectedShipList", HumanPlayer.Instance.Squad.Colliders[_selectedIndices["AltSquadList"]], "AltShipList");
        UpdateTransferInterface(false, true, true);
    }

    public void NewSquad()
    {
        var squad = HumanPlayer.Instance.CreateNewSquad(HumanPlayer.Instance.Squad);
        var listing = (ListableObject)squad;

        listing.CreateListEntry("AltSquadList", HumanPlayer.Instance.Squad.Colliders.Count - 1, true).transform.SetParent(_interface["AltSquadList"].transform);
        UpdateTransferInterface(true, true, false);
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

    public void ConfigureBattleScreen(float WC, Squad squad1, Squad squad2)
    {
        var t1 = squad1.Team;

        Squad player = squad2;
        Squad enemy = squad1;

        if (t1 == HumanPlayer.Instance.Team)
        {
            player = squad1;
            enemy = squad2;
        }

        var pt = player.GetComponent<Tile>();
        var et = enemy.GetComponent<Tile>();

        _interface["BattlePercentage"].GetComponent<Text>().text = WC.ToString("P");

        // for detailed battle screen - current blows
        if(pt != null)
        {
            // player tile vs. enemy squad
        }
        else if(et != null)
        {
            // player squad vs. enemy tile
        }
        else // squad vs. squad
        {

        }

        SetUIElements(false, true, false, false, false, false);
    }

    public void Battle()
    {
        var winner = HumanPlayer.Instance.Battle(0f, null, null);

        if(winner.Team == HumanPlayer.Instance.Team)
        {
            SetUIElements(false, false, true, false, false, false);
        }
        else
        {
            SetUIElements(false, false, false, true, false, false);
        }
    }
}
