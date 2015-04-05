using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GUIManager : MonoBehaviour 
{
    private const string INI_PATH = "/Resources/Ships.ini";
    private const string SHIP_SECTION_HEADER = "[Ships]";

    private static GUIManager _instance;
    private Dictionary<string, CustomUI> _interface;
    private Dictionary<string, Sprite> _icons;
    private Dictionary<string, int> _indices;
    private Dictionary<string, string> _descriptions; // duplicating that much text for each ship is terrible for memory.
    private float _popUpTimer;

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

    public void Init(Dictionary<string, string> descriptions)
    {
        _descriptions = descriptions;
    }

    void Awake()
    {
        _instance = this;

        if (_interface == null)
            _interface = new Dictionary<string, CustomUI>();

        _indices = new Dictionary<string, int>()
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

    public void Register(string name, CustomUI btn, bool disable)
    {
        if (_interface == null)
            _interface = new Dictionary<string, CustomUI>();

        if (!_interface.ContainsKey(name) && btn != null && btn.gameObject != null)
        {
            _interface.Add(name, btn);
            _interface[name].gameObject.SetActive(!disable);
        }
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
        var index = _indices["MainShipList"];

        // note: i fucking hate this bit
        var click = false;
        if (HumanPlayer.Instance.Team == squad.Team)
        {
            if(type && index != -1 && squad.OnMission == false) // deploy
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
        switch(split[0])
        {
            case "MainShipList":
            case "AltShipList":
                _indices[split[0]] = int.Parse(split[1]);
                UpdateTransferInterface(false, true, false, true);
                break;
            case "SelectedShipList":
                _indices[split[0]] = int.Parse(split[1]);
                UpdateTransferInterface(false, false, true, true);
                break;
            case "AltSquadList":
                _indices[split[0]] = int.Parse(split[1]);
                UpdateTransferInterface(false, true, false, true);
                break;
            case "Constructables":
                HumanPlayer.Instance.CreateBuildEvent(split[1]);
                break;
        }
    }
    
    public void UIHighlighted(string data)
    {
        _popUpTimer = 1.0f;
    }

    public void UIHighlightStay(string data)
    {
        var split = data.Split('|');
        if (_popUpTimer >= 0.0f)
        {
            _popUpTimer -= Time.deltaTime;

            if (_popUpTimer >= 0.0f)
                return;

            switch (split[0])
            {
                case "MainShipList":
                case "SelectedShipList":
                    var i = int.Parse(split[1]);
                    var ship1 = HumanPlayer.Instance.Squad.Ships[i];
                    (ship1 as ListableObject).PopulateGeneralInfo(
                        _interface["ShipInfo"].gameObject, _descriptions[ship1.Name]);
                    _interface["ShipInfo"].gameObject.SetActive(true);
                    break;
                case "AltShipList":
                    var j = int.Parse(split[1]);
                    var ship2 = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]].Ships[j];
                    (ship2 as ListableObject).PopulateGeneralInfo(
                        _interface["ShipInfo"].gameObject, _descriptions[ship2.Name]);
                    _interface["ShipInfo"].gameObject.SetActive(true);
                    break;
                case "AltSquadList":
                    break;
                case "Constructables":
                    (HumanPlayer.Instance.GetShipDefinition(split[1]) as ListableObject).PopulateBuildInfo(
                        _interface["ConstructionInfo"].gameObject, _descriptions[split[1]]);
                    _interface["ConstructionInfo"].gameObject.SetActive(true);
                    break;
            }
        }

        switch (split[0])
        {
            case "MainShipList":
            case "SelectedShipList":
            case "AltShipList":
                _interface["ShipInfo"].transform.position = Input.mousePosition;
                break;
            case "AltSquadList":
                break;
            case "Constructables":
                _interface["ConstructionInfo"].transform.position = Input.mousePosition;
                break;
        }
    }

    public void UIDehighlighted(string data)
    {
        var split = data.Split('|');
        switch (split[0])
        {
            case "MainShipList":
            case "SelectedShipList":
            case "AltShipList":
                _interface["ShipInfo"].gameObject.SetActive(false);
                break;
            case "AltSquadList":
                break;
            case "Constructables":
                _interface["ConstructionInfo"].gameObject.SetActive(false);
                break;
        }

        _popUpTimer = 1.0f;
    }

    public void PopulateManageLists()
    {
        GUIManager.Instance.SetUIElements(false, false, false, true);
        _indices["MainShipList"] = -1;
        ClearList("MainShipList");

        var squad = HumanPlayer.Instance.Squad;
        // temporarily add itself to colliders
        squad.Colliders.Add(HumanPlayer.Instance.Squad);

        // add deployed structures to the nearby tile if necessary
        if (squad.Tile != null && squad.Tile.Structure != null && squad.Tile.IsInRange(squad))
            squad.Tile.Squad.Ships.Add(squad.Tile.Structure);

        UpdateTransferInterface(true, true, true, true);
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
            _indices[name] = -1;
        else if (_indices[name] >= list.Count)
            _indices[name] = list.Count - 1;
        else if (_indices[name] < 0)
            _indices[name] = 0;
    }

    public void SquadSelected(Squad squad)
    {
        var keys = new List<string>(_indices.Keys);
        foreach (var index in keys)
            _indices[index] = -1;

        PopulateList<Ship>(squad.Ships, "MainShipList", ListingType.Info, false);

        _interface["DeployText"].GetComponent<Text>().text = "Deploy";
        _interface["Deploy"].GetComponent<CustomUI>().data = "Deploy";
        SetUIElements(true, false, false, false);
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

        SetUIElements(true, false, true, false);
        SetSquadControls(squad);
    }

    public void SetStructure(string data)
    {
        if (data == "Deploy")
            HumanPlayer.Instance.CreateDeployEvent(_indices["MainShipList"]);
        else
            HumanPlayer.Instance.CreateUndeployEvent(false);
    }

    private void UpdateTransferInterface(bool squads, bool squadShips, bool ships, bool soldiers)
    {
        UpdateTransferShipInterface(squads, squadShips, ships);
        if (soldiers)
            UpdateTransferSoldierInterface();
    }

    private void UpdateTransferShipInterface(bool squads, bool squadShips, bool ships)
    {
        var squad = HumanPlayer.Instance.Squad;
        AutoSelectIndex<Squad>("AltSquadList", squad.Colliders);
        if (_indices["AltSquadList"] != -1)
            AutoSelectIndex<Ship>("AltShipList", squad.Colliders[_indices["AltSquadList"]].Ships);
        AutoSelectIndex<Ship>("SelectedShipList", squad.Ships);

        if (squads)
            PopulateList<Squad>(squad.Colliders, "AltSquadList", ListingType.Info, true);
        if (squadShips && _indices["AltSquadList"] > -1 && _indices["AltSquadList"] < squad.Colliders.Count)
            PopulateList<Ship>(squad.Colliders[_indices["AltSquadList"]].Ships, "AltShipList", ListingType.Info, true);
        if (ships)
            PopulateList<Ship>(squad.Ships, "SelectedShipList", ListingType.Info, true);

        var right = _interface["Right"].GetComponent<Button>();
        var dright = _interface["DoubleRight"].GetComponent<Button>();
        var left = _interface["Left"].GetComponent<Button>();
        var dleft = _interface["DoubleLeft"].GetComponent<Button>();

        var altsquad = _indices["AltSquadList"];
        var altships = _indices["AltShipList"];
        var selected = _indices["SelectedShipList"];

        if (altsquad == -1 || squad == squad.Colliders[altsquad])
        {
            right.interactable = left.interactable = dright.interactable = dleft.interactable = false;
        }
        else
        {
            if (altships != -1)
            {
                var ship = HumanPlayer.Instance.Squad.Colliders[altsquad].Ships[altships];
                right.interactable = (ship.ShipProperties & ShipProperties.Untransferable) == 0 ? true : false;
                dright.interactable = true;
            }

            if (selected != -1)
            {
                var ship = HumanPlayer.Instance.Squad.Ships[selected];
                left.interactable = (ship.ShipProperties & ShipProperties.Untransferable) == 0 ? true : false;
                dleft.interactable = true;
            }
        }
    }

    private void UpdateTransferSoldierInterface()
    {
        var pright = _interface["PrimRight"].GetComponent<Button>();
        var iright = _interface["IndRight"].GetComponent<Button>();
        var sright = _interface["SpaceRight"].GetComponent<Button>();
        var pleft = _interface["PrimLeft"].GetComponent<Button>();
        var ileft = _interface["IndLeft"].GetComponent<Button>();
        var sleft = _interface["SpaceLeft"].GetComponent<Button>();

        var pdright = _interface["PrimRightAll"].GetComponent<Button>();
        var idright = _interface["IndRightAll"].GetComponent<Button>();
        var sdright = _interface["SpaceRightAll"].GetComponent<Button>();
        var pdleft = _interface["PrimLeftAll"].GetComponent<Button>();
        var idleft = _interface["IndLeftAll"].GetComponent<Button>();
        var sdleft = _interface["SpaceLeftAll"].GetComponent<Button>();

        var plCount = _interface["PrimLeftCount"].GetComponent<Text>();
        var ilCount = _interface["IndLeftCount"].GetComponent<Text>();
        var slCount = _interface["SpaceLeftCount"].GetComponent<Text>();
        var prCount = _interface["PrimRightCount"].GetComponent<Text>();
        var irCount = _interface["IndRightCount"].GetComponent<Text>();
        var srCount = _interface["SpaceRightCount"].GetComponent<Text>();

        var altsquad = _indices["AltSquadList"];
        var altships = _indices["AltShipList"];
        var selected = _indices["SelectedShipList"];

        if (selected != -1 && altships != -1) // manage soldiers!
        {
            var aship = HumanPlayer.Instance.Squad.Colliders[altsquad].Ships[altships];
            var sship = HumanPlayer.Instance.Squad.Ships[selected];
            var aShipTotal = aship.PrimitivePopulation + aship.IndustrialPopulation + aship.SpaceAgePopulation;
            var sShipTotal = sship.PrimitivePopulation + sship.IndustrialPopulation + sship.SpaceAgePopulation;
            var aShipFull = aShipTotal == aship.Capacity;
            var sShipFull = sShipTotal == sship.Capacity;
            var same = aship == sship;

            plCount.text = aship.PrimitivePopulation.ToString();
            ilCount.text = aship.IndustrialPopulation.ToString();
            slCount.text = aship.SpaceAgePopulation.ToString();
            prCount.text = sship.PrimitivePopulation.ToString();
            irCount.text = sship.IndustrialPopulation.ToString();
            srCount.text = sship.SpaceAgePopulation.ToString();

            pright.interactable = pdright.interactable = aship.PrimitivePopulation > 0 && !sShipFull && !same;
            iright.interactable = idright.interactable = aship.IndustrialPopulation > 0 && !sShipFull && !same;
            sright.interactable = sdright.interactable = aship.SpaceAgePopulation > 0 && !sShipFull && !same;
            pleft.interactable = pdleft.interactable = sship.PrimitivePopulation > 0 && !aShipFull && !same;
            ileft.interactable = idleft.interactable = sship.IndustrialPopulation > 0 && !aShipFull && !same;
            sleft.interactable = sdleft.interactable = sship.SpaceAgePopulation > 0 && !aShipFull && !same;
        }
        else
        {
            pdright.interactable = idright.interactable = sdright.interactable = pdleft.interactable = idleft.interactable = sdleft.interactable = false;
            pright.interactable = iright.interactable = sright.interactable = pleft.interactable = ileft.interactable = sleft.interactable = false;
            plCount.text = ilCount.text = slCount.text = prCount.text = irCount.text = srCount.text = Convert.ToString(0);

            pright.interactable = pdright.interactable = false;
            iright.interactable = idright.interactable = false;
            sright.interactable = sdright.interactable = false;
            pleft.interactable = pdleft.interactable = false;
            ileft.interactable = idleft.interactable = false;
            sleft.interactable = sdleft.interactable = false;
        }
    }

    public Ship Transfer(Squad from, string fromName, Squad to, string toName)
    {
        var fromIndex = _indices[fromName];
        var ship = from.Ships[fromIndex];
        from.Ships.RemoveAt(fromIndex);

        if((ship.ShipProperties & ShipProperties.Untransferable) > 0)
            return ship;

        to.Ships.Add(ship);
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
        var index = _indices["AltSquadList"];
        var squad = HumanPlayer.Instance.Squad.Colliders[index];
        var ship = Transfer(squad, "AltShipList", HumanPlayer.Instance.Squad, "SelectedShipList");
        if (ship != null) squad.Ships.Insert(index, ship);
        UpdateTransferInterface(false, true, true, true);
    }

    public void TransferToSelectedSquad()
    {
        var index = _indices["SelectedShipList"];
        var squad = HumanPlayer.Instance.Squad;
        var ship = Transfer(squad, "SelectedShipList", HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]], "AltShipList");
        if (ship != null) squad.Ships.Insert(index, ship);
        UpdateTransferInterface(false, true, true, true);
    }

    public void TransferAllToControlledSquad()
    {
        TransferAll(HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]], "AltShipList", HumanPlayer.Instance.Squad, "SelectedShipList");
        UpdateTransferInterface(false, true, true, true);
    }

    public void TransferAllToSelectedSquad()
    {
        TransferAll(HumanPlayer.Instance.Squad, "SelectedShipList", HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]], "AltShipList");
        UpdateTransferInterface(false, true, true, true);
    }

    public void NewSquad()
    {
        var squad = HumanPlayer.Instance.CreateNewSquad(HumanPlayer.Instance.Squad);
        var listing = (ListableObject)squad;

        listing.CreateListEntry("AltSquadList", HumanPlayer.Instance.Squad.Colliders.Count - 1, true).transform.SetParent(_interface["AltSquadList"].transform);
        UpdateTransferInterface(true, true, false, true);
    }

    private void TransferSoldier(string type, Ship from, Ship to)
    {
        switch(type)
        {
            case "Primitive":
                from.PrimitivePopulation--;
                to.PrimitivePopulation++;
                break;
            case "Industrial":
                from.IndustrialPopulation--;
                to.IndustrialPopulation++;
                break;
            case "SpaceAge":
                from.SpaceAgePopulation--;
                to.SpaceAgePopulation++;
                break;
        }
    }

    private void TransferAllSoldiers(string type, Ship from, Ship to)
    {
        switch (type)
        {
            case "Primitive":
                while (from.PrimitivePopulation > 0 && to.PrimitivePopulation + to.IndustrialPopulation + to.SpaceAgePopulation < to.Capacity)
                    TransferSoldier(type, from, to);
                break;
            case "Industrial":
                while (from.IndustrialPopulation > 0 && to.PrimitivePopulation + to.IndustrialPopulation + to.SpaceAgePopulation < to.Capacity)
                    TransferSoldier(type, from, to);
                break;
            case "SpaceAge":
                while (from.SpaceAgePopulation > 0 && to.PrimitivePopulation + to.IndustrialPopulation + to.SpaceAgePopulation < to.Capacity)
                    TransferSoldier(type, from, to);
                break;
        }
    }

    public void SoldierTransfer(string data)
    {
        var aship = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]].Ships[_indices["AltShipList"]];
        var sship = HumanPlayer.Instance.Squad.Ships[_indices["SelectedShipList"]];

        switch(data)
        {
            case "PrimRight":
                TransferSoldier("Primitive", aship, sship);
                break;
            case "IndRight":
                TransferSoldier("Industrial", aship, sship);
                break;
            case "SpaceRight":
                TransferSoldier("SpaceAge", aship, sship);
                break;
            case "PrimLeft":
                TransferSoldier("Primitive", sship, aship);
                break;
            case "IndLeft":
                TransferSoldier("Industrial", sship, aship);
                break;
            case "SpaceLeft":
                TransferSoldier("SpaceAge", sship, aship);
                break;
            case "PrimRightAll":
                TransferAllSoldiers("Primitive", aship, sship);
                break;
            case "IndRightAll":
                TransferAllSoldiers("Industrial", aship, sship);
                break;
            case "SpaceRightAll":
                TransferAllSoldiers("SpaceAge", aship, sship);
                break;
            case "PrimLeftAll":
                TransferAllSoldiers("Primitive", sship, aship);
                break;
            case "IndLeftAll":
                TransferAllSoldiers("Industrial", sship, aship);
                break;
            case "SpaceLeftAll":
                TransferAllSoldiers("SpaceAge", sship, aship);
                break;
        }

        UpdateTransferInterface(false, true, true, true);
    }

    public void ExitManage()
    {
        var squad = HumanPlayer.Instance.Squad;
        squad.Colliders.Remove(squad); // remove self
        if (squad.Tile != null && squad.Tile.IsInRange(squad)) // remove any deployed structures
            squad.Tile.Squad.Ships.Remove(squad.Tile.Structure);

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

        _interface["BattleChance"].GetComponent<Text>().text = WC.ToString("P");

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

        SetUIElements(false, true, false, false);
    }

    public void Battle()
    {
        var winner = HumanPlayer.Instance.Battle(0f, null, null);

        if(winner.Team == HumanPlayer.Instance.Team)
        {
            _interface["BattleWon"].gameObject.SetActive(true);
        }
        else
        {
            _interface["BattleLost"].gameObject.SetActive(true);
        }

        SetUIElements(false, false, false, false);
    }
}
