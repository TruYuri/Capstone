using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GUIManager : MonoBehaviour 
{
    private static GUIManager _instance;
    private Dictionary<string, CustomUI> _interface = new Dictionary<string, CustomUI>();
    private Dictionary<string, Sprite> _icons;
    private Dictionary<string, int> _indices;
    private Dictionary<string, string> _descriptions; // duplicating that much text for each ship is terrible for memory.
    private float _popUpTimer;
    private Dictionary<string, AudioSource> _audio;

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

    public void Init(Dictionary<string, string> descriptions, Dictionary<string, Sprite> extraIcons)
    {
        _descriptions = descriptions;
        _instance = this;
        _audio = new Dictionary<string, AudioSource>();

        _audio.Add("Click", GetComponents<AudioSource>().Where(s => s.clip.name == "sfx_button").ToList()[0]);
        _audio.Add("Warp", GetComponents<AudioSource>().Where(s => s.clip.name == "warp").ToList()[0]);
        _audio.Add("BuildStart", GetComponents<AudioSource>().Where(s => s.clip.name == "arc-welding").ToList()[0]);
        _audio.Add("SquadSelect", GetComponents<AudioSource>().Where(s => s.clip.name == "squad_select").ToList()[0]);
        _audio.Add("Battle", GetComponents<AudioSource>().Where(s => s.clip.name == "Lasers").ToList()[0]);
        _audio.Add("Deploy", GetComponents<AudioSource>().Where(s => s.clip.name == "Deploy").ToList()[0]);
        _audio.Add("Undeploy", GetComponents<AudioSource>().Where(s => s.clip.name == "hover-engine").ToList()[0]);
        _audio.Add("TileSelect", GetComponents<AudioSource>().Where(s => s.clip.name == "Select").ToList()[0]);
        _audio.Add("Command", GetComponents<AudioSource>().Where(s => s.clip.name == "Send Command").ToList()[0]);

        _indices = new Dictionary<string, int>()
        {
            { "MainShipList", -1 },
            { "Constructables", -1 },
            { "AltSquadList", -1 },
            { "AltShipList", -1 },
            { "SelectedShipList", -1 },
            { "SquadList", -1 },
            { "TileList", -1 },
            { "WarpList", -1 },
            { "Zoom", 0 }
        };

        _icons = new Dictionary<string, Sprite>()
        {
            { "Oil", Resources.Load<Sprite>(UI_ICONS_PATH + "OilIcon") },
            { "Asterminium", Resources.Load<Sprite>(UI_ICONS_PATH + "AsterminiumIcon") },
            { "Ore", Resources.Load<Sprite>(UI_ICONS_PATH + "OreIcon") },
            { "Forest", Resources.Load<Sprite>(UI_ICONS_PATH + "ForestIcon") },
            { "NoResource", Resources.Load<Sprite>(UI_ICONS_PATH + "NoResourceIcon") },
            { "Indigenous", Resources.Load<Sprite>(UI_ICONS_PATH + "IndigenousIcon") },
            { "Uninhabited", Resources.Load<Sprite>(UI_ICONS_PATH + "UninhabitedIcon") },
            { "Plinthen", Resources.Load<Sprite>(UI_ICONS_PATH + "PlinthenIcon") },
            { "Union", Resources.Load<Sprite>(UI_ICONS_PATH + "UnionIcon") },
            { "Kharkyr", Resources.Load<Sprite>(UI_ICONS_PATH + "KharkyrIcon") },
            { "Primitive", Resources.Load<Sprite>(UI_ICONS_PATH + "PrimitivePopulationIcon") },
            { "Industrial", Resources.Load<Sprite>(UI_ICONS_PATH + "IndustrialPopulationIcon") },
            { "SpaceAge", Resources.Load<Sprite>(UI_ICONS_PATH + "SpaceAgePopulationIcon") }
        };

        foreach (var icon in extraIcons)
            _icons.Add(icon.Key, icon.Value);
    }

    public void Register(string name, CustomUI btn, bool disable)
    {
        _interface.Add(name, btn);
        _interface[name].gameObject.SetActive(!disable);
    }

    void Start()
    {
        var cuis = this.gameObject.GetComponentsInChildren<CustomUI>();

        foreach (var c in cuis)
            c.Register();
    }

    public void PlaySound(string name)
    {
        _audio[name].Play();
    }

    public void SetScreen(string screen)
    {
        var player = HumanPlayer.Instance;
        switch (screen)
        {
            case "MainUI":
                _interface["Main"].gameObject.SetActive(true);
                _interface["ScientificResearch"].gameObject.SetActive(false);
                _interface["MilitaryResearch"].gameObject.SetActive(false);
                player.ReloadGameplayUI();
                break;
            case "Scientific":
                _interface["Main"].gameObject.SetActive(false);
                _interface["ScientificResearch"].gameObject.SetActive(true);
                _interface["MilitaryResearch"].gameObject.SetActive(false);
                player.DisplayResources(_interface["SciResources"].gameObject);
                player.DisplayResearch("Scientific", "Command Ship", _interface["Command Ship"].gameObject);
                player.DisplayResearch("Scientific", "Efficiency", _interface["Efficiency"].gameObject);
                player.DisplayResearch("Scientific", "Complex", _interface["Complex"].gameObject);
                player.DisplayResearch("Scientific", "Relay", _interface["Relay"].gameObject);
                player.DisplayResearch("Scientific", "Warp Portal", _interface["Warp Portal"].gameObject);
                player.DisplayResources(_interface["SciResources"].gameObject);
                break;
            case "Military":
                _interface["Main"].gameObject.SetActive(false);
                _interface["ScientificResearch"].gameObject.SetActive(false);
                _interface["MilitaryResearch"].gameObject.SetActive(true);
                player.DisplayResources(_interface["MilResources"].gameObject);
                player.DisplayResearch("Military", "Fighter", _interface["Fighter"].gameObject);
                player.DisplayResearch("Military", "Transport", _interface["Transport"].gameObject);
                player.DisplayResearch("Military", "Guard Satellite", _interface["Guard Satellite"].gameObject);
                player.DisplayResearch("Military", "Heavy Fighter", _interface["Heavy Fighter"].gameObject);
                player.DisplayResearch("Military", "Behemoth", _interface["Behemoth"].gameObject);
                player.DisplayResources(_interface["MilResources"].gameObject);
                break;
        }
    }

    public void UpdateResearch(string data)
    {
        var split = data.Split('|');
        HumanPlayer.Instance.UpgradeResearch(split[0], split[1], split[2]);
        SetScreen(split[0]);
    }

    public void SetSquadControls(Squad squad)
    {
        if (_interface["SquadMenu"].gameObject.activeInHierarchy == false)
            return;

        var manage = _interface["Manage"].gameObject.GetComponent<Button>();
        var deploy = _interface["SquadAction"].gameObject.GetComponent<Button>();
        var type = deploy.GetComponent<CustomUI>().data;
        var tile = squad.Tile;

        AutoSelectIndex<Ship>("MainShipList", squad.Ships);
        var index = _indices["MainShipList"];

        // note: i fucking hate this bit
        var click = false;
        if (HumanPlayer.Instance.Team == squad.Team && HumanPlayer.Instance.ControlledIsWithinRange && HumanPlayer.Instance.Squad.Mission == null)
        {
            if (type == "Deploy" && index != -1) // deploy
            {
                if (tile != null && (squad.Ships[index].ShipProperties & ShipProperties.GroundStructure) > 0
                    && squad.Tile.IsInRange(squad) && (tile.Team == squad.Team || tile.Team == Team.Uninhabited) && tile.Structure == null) // existing planet
                    click = true;
                else if (tile == null && (squad.Ships[index].ShipProperties & ShipProperties.SpaceStructure) > 0 &&
                    squad.Sector.IsValidLocation(squad.transform.position))// empty space
                    click = true;
            }
            else if (type == "Undeploy" && tile != null && squad == tile.Squad && tile.Structure != null)// undeploy
                click = true;
            else if (type == "Invade" && squad.CalculateTroopPower() > 0f)
                click = true;
            else if (type == "Warp" && tile.Team == squad.Team)
                click = true;

            deploy.interactable = click;
            manage.interactable = squad.Ships.Count > 0 || squad.Colliders.Count > 0;
        }
        else
        {
            deploy.interactable = false;
            manage.interactable = false;
        }
    }

    public void SetSquadList(bool set)
    {
        _interface["SquadListControl"].gameObject.SetActive(set);

        if (set)
            ReloadSquadList();
    }

    public void SetTileList(bool set)
    {
        _interface["TileListControl"].gameObject.SetActive(set);

        if (set)
            ReloadTileList();
    }

    private void ReloadSquadList()
    {
        PopulateList<Squad>(HumanPlayer.Instance.Squads, "SquadList", ListingType.Info, null);
    }

    private void ReloadTileList()
    {
        PopulateList<Tile>(HumanPlayer.Instance.Tiles, "TileList", ListingType.Info, null);
    }

    public void SetUIElements(bool squad, bool events, bool battle, bool tile, bool manage, bool lists, bool minimap)
    {
        _interface["SquadMenu"].gameObject.SetActive(squad);
        _interface["EventControl"].gameObject.SetActive(events);
        _interface["BattleMenu"].gameObject.SetActive(battle);
        _interface["PlanetInfo"].gameObject.SetActive(tile);
        _interface["ManageMenu"].gameObject.SetActive(manage);
        _interface["MenuControl"].gameObject.SetActive(lists);

        if (lists)
            HumanPlayer.Instance.DisplayResources(_interface["MainResources"].gameObject);

        _interface["MapInfo"].gameObject.SetActive(minimap);
    }

    public void ItemClicked(string data)
    {
        var split = data.Split('|');
        switch(split[0])
        {
            case "MainShipList":
                _indices[split[0]] = int.Parse(split[1]);
                break;
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
                PlaySound("BuildStart");
                break;
            case "SquadList":
                _indices[split[0]] = int.Parse(split[1]);
                HumanPlayer.Instance.Control(HumanPlayer.Instance.Squads[_indices[split[0]]].gameObject);
                GUIManager.Instance.PlaySound("SquadSelect");
                break;
            case "TileList":
                _indices[split[0]] = int.Parse(split[1]);
                HumanPlayer.Instance.Control(HumanPlayer.Instance.Tiles[_indices[split[0]]].gameObject);
                GUIManager.Instance.PlaySound("TileSelect");
                break;
            case "WarpList":
                _indices[split[0]] = int.Parse(split[1]);
                SetWarpList(true);
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
                case "WarpList":
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
                case "Constructables":
                    (HumanPlayer.Instance.ShipDefinitions[split[1]] as ListableObject).PopulateBuildInfo(
                        _interface["ConstructionInfo"].gameObject, _descriptions[split[1]]);
                    _interface["ConstructionInfo"].gameObject.SetActive(true);
                    break;
                case "AltSquadList":
                    var k = int.Parse(split[1]);
                    ClearList("SquadInfoList");
                    (HumanPlayer.Instance.Squad.Colliders[k] as ListableObject).PopulateGeneralInfo(
                        _interface["SquadInfo"].gameObject, null);
                    _interface["SquadInfo"].gameObject.SetActive(true);
                    break;
                case "SquadList":
                    var l = int.Parse(split[1]);
                    ClearList("SquadInfoList");
                    (HumanPlayer.Instance.Squads[l] as ListableObject).PopulateGeneralInfo(
                        _interface["SquadInfo"].gameObject, null);
                    _interface["SquadInfo"].gameObject.SetActive(true);
                    break;
                case "TileList":
                    break;
                case "Scientific":
                    HumanPlayer.Instance.PopulateResearchPanel(split[0], split[1], split[2], _interface["ScientificResearchInfo"].gameObject);
                    _interface["ScientificResearchInfo"].gameObject.SetActive(true);
                    break;
                case "Military":
                    HumanPlayer.Instance.PopulateResearchPanel(split[0], split[1], split[2], _interface["MilitaryResearchInfo"].gameObject);
                    _interface["MilitaryResearchInfo"].gameObject.SetActive(true);
                    break;
                case "MilHelp":
                    _interface["MilitaryUnlockInfo"].gameObject.SetActive(true);
                    break;
                case "SciHelp":
                    _interface["ScientificUnlockInfo"].gameObject.SetActive(true);
                    break;
            }
        }

        switch (split[0])
        {
            case "MainShipList":
            case "SelectedShipList":
            case "AltShipList":
            case "WarpList":
                _interface["ShipInfo"].transform.position = Input.mousePosition;
                break;
            case "Constructables":
                _interface["ConstructionInfo"].transform.position = Input.mousePosition;
                break;
            case "AltSquadList":
            case "SquadList":
                _interface["SquadInfo"].transform.position = Input.mousePosition;
                break;
            case "TileList":
                break;
            case "Scientific":
                 _interface["ScientificResearchInfo"].transform.position = Input.mousePosition;
                break;
            case "Military":
                _interface["MilitaryResearchInfo"].transform.position = Input.mousePosition;
                break;
            case "MilHelp":
                _interface["MilitaryUnlockInfo"].transform.position = Input.mousePosition;
                break;
            case "SciHelp":
                _interface["ScientificUnlockInfo"].transform.position = Input.mousePosition;
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
            case "WarpList":
                _interface["ShipInfo"].gameObject.SetActive(false);
                break;
            case "Constructables":
                _interface["ConstructionInfo"].gameObject.SetActive(false);
                break;
            case "AltSquadList":
            case "SquadList":
                _interface["SquadInfo"].gameObject.SetActive(false);
                break;
            case "TileList":
                break;
            case "Scientific":
                _interface["ScientificResearchInfo"].gameObject.SetActive(false);
                break;
            case "Military":
                _interface["MilitaryResearchInfo"].gameObject.SetActive(false);
                break;
            case "MilHelp":
                _interface["MilitaryUnlockInfo"].gameObject.SetActive(false);
                break;
            case "SciHelp":
                _interface["ScientificUnlockInfo"].gameObject.SetActive(false);
                break;
        }

        _popUpTimer = 1.0f;
    }

    public void PopulateManageLists()
    {
        SetUIElements(false, false, false, false, true, false, false);
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
            if(index != "Zoom")
                _indices[index] = -1;

        PopulateList<Ship>(squad.Ships, "MainShipList", ListingType.Info, false);
        var text = _interface["SquadActionText"].GetComponent<Text>();
        var data = _interface["SquadAction"].GetComponent<CustomUI>();

        text.text = data.data = "Deploy";

        if (squad.Tile != null && squad.Tile.IsInRange(squad))
        {
            if(squad.Tile.Team != squad.Team && squad.Tile.Team != Team.Uninhabited)
                text.text = data.data = "Invade";
            else if(squad.Tile.Team == squad.Team && squad.Tile.Structure != null && squad.Tile.Structure.Name == "Warp Portal")
                text.text = data.data = "Warp";
        }

        _interface["SquadTeamIcon"].GetComponent<Image>().sprite = _icons[squad.Team.ToString()];
        _interface["SquadTeamName"].GetComponent<Text>().text = squad.name;
        SetUIElements(true, true, false, false, false, true, true);
        SetSquadControls(squad);

        ReloadSquadList();
        ReloadTileList();
    }

    public void TileSelected(Tile tile, Dictionary<string, Ship> defs)
    {
        var squad = tile.gameObject.GetComponent<Squad>();
        SquadSelected(squad);

        tile.PopulateInfoPanel(_interface["PlanetInfo"].gameObject);

        // move to structure
        var structure = tile.Structure != null;
        var deployText = structure ? "Undeploy" : "Deploy";

        _interface["Structure"].gameObject.SetActive(structure);
        _interface["SquadActionText"].GetComponent<Text>().text = deployText;
        _interface["SquadAction"].GetComponent<CustomUI>().data = deployText;

        if (structure)
        {
            // populate structure info
            tile.Structure.Resources.Remove(Resource.Stations);
            tile.Structure.PopulateStructurePanel(_interface["Structure"].gameObject);

            if (tile.Structure.Constructables.Count > 0 && tile.Team == HumanPlayer.Instance.Team)
            {
                _interface["ConstructionList"].gameObject.SetActive(true);
                _interface["ConstBar"].gameObject.SetActive(true);

                var buildList = new List<Ship>();
                foreach (var construct in tile.Structure.Constructables)
                    buildList.Add(defs[construct]);
                PopulateList<Ship>(buildList, "Constructables", ListingType.Build, tile.Structure.Resources);
            }
            else
            {
                _interface["ConstructionList"].gameObject.SetActive(false);
                _interface["ConstBar"].gameObject.SetActive(false);
            }
        }

        SetUIElements(true, true, false, true, false, true, false);
        SetSquadControls(squad);
    }

    public void SquadAction(string data)
    {
        switch(data)
        {
            case "Deploy":
                HumanPlayer.Instance.CreateDeployEvent(_indices["MainShipList"]);
                break;
            case "Undeploy":
                HumanPlayer.Instance.CreateUndeployEvent(false);
                break;
            case "Invade":
                HumanPlayer.Instance.CreateBattleEvent(HumanPlayer.Instance.Squad, HumanPlayer.Instance.Squad.Tile);
                break;
            case "Warp":
                SetWarpList(true);
                break;
        }
    }

    public void Warp()
    {
        var sq = HumanPlayer.Instance.Squad;
        var warpList = MapManager.Instance.FindPortals(HumanPlayer.Instance.Team, sq.Tile.Structure, sq.Sector);
        warpList.Remove(sq.Tile.Structure);
        var portals = warpList.Keys.ToList();

        HumanPlayer.Instance.CreateWarpEvent(portals[_indices["WarpList"]].Tile, sq);

        GUIManager.Instance.SetWarpList(false);
        HumanPlayer.Instance.ReloadGameplayUI();
    }

    public void SetWarpList(bool enable)
    {
        _interface["WarpScreen"].gameObject.SetActive(enable);

        if (!enable)
            return;

        ClearList("WarpList");
        var sq = HumanPlayer.Instance.Squad;
        var warpList = MapManager.Instance.FindPortals(HumanPlayer.Instance.Team, sq.Tile.Structure, sq.Sector);
        warpList.Remove(sq.Tile.Structure);
        var portals = warpList.Keys.ToList();
        PopulateList<Structure>(portals, "WarpList", ListingType.Info, null);
        var i = _indices["WarpList"];

        var colors = new Dictionary<Sector, Color>()
        {
            { sq.Sector, Color.black },
        };

        if (i != -1 && warpList[portals[i]] == sq.Sector)
            colors[sq.Sector] = Color.red;
        else if (i != -1)
            colors.Add(warpList[portals[i]], Color.magenta);

        var image = _interface["WarpMap"].GetComponent<RawImage>();
        var texture = MapManager.Instance.GenerateMap(HumanPlayer.Instance.Squad.Sector, colors);    

        image.texture = texture;

        /*
        var minw = Math.Min(texture.width, 390);
        var miny = Math.Min(texture.height, 390);

        var left = center.x - (minw / 2 / (float)texture.width);
        if(left < 0f)
            left = 0f;
        var top = center.y - (miny / 2 / (float)texture.height);
        if (top < 0f)
            top = 0f;

        _interface["WarpButton"].GetComponent<Button>().interactable = i != -1;
        image.uvRect = new Rect(left, top,
            minw / (float)texture.width, miny / (float)texture.height);
         * */
    }

    private void UpdateTransferInterface(bool squads, bool squadShips, bool ships, bool other)
    {
        UpdateTransferShipInterface(squads, squadShips, ships);
        if (other && _interface["SoldierManager"].gameObject.activeInHierarchy)
            UpdateTransferItemInterface("Soldiers");
        else if (other && _interface["ResourceManager"].gameObject.activeInHierarchy)
            UpdateTransferItemInterface("Resources");
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
        var dright = _interface["RightAll"].GetComponent<Button>();
        var left = _interface["Left"].GetComponent<Button>();
        var dleft = _interface["LeftAll"].GetComponent<Button>();

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

    public void SwapManager(string data)
    {
        if(data == "Resources")
        {
            _interface["ResourceManager"].gameObject.SetActive(true);
            _interface["SoldierManager"].gameObject.SetActive(false);
        }
        else if(data == "Soldiers")
        {
            _interface["ResourceManager"].gameObject.SetActive(false);
            _interface["SoldierManager"].gameObject.SetActive(true);
        }

        UpdateTransferInterface(false, false, false, true);
    }

    private void UpdateTransferItemInterface(string type)
    {
        var rtypes = new List<Resource>() { Resource.Ore, Resource.Oil, Resource.Forest, Resource.Asterminium };
        var stypes = new List<Inhabitance>() { Inhabitance.Primitive, Inhabitance.Industrial, Inhabitance.SpaceAge };

        var altsquad = _indices["AltSquadList"];
        var altships = _indices["AltShipList"];
        var selected = _indices["SelectedShipList"];

        var aship = altsquad != -1 && altships != -1 ? HumanPlayer.Instance.Squad.Colliders[altsquad].Ships[altships] : null;
        var sship = selected != -1 ? HumanPlayer.Instance.Squad.Ships[selected] : null;

        if(type == "Soldiers")
        {
            var aCounts = aship != null ? aship.Population : null;
            var sCounts = sship != null ? sship.Population : null;
            var aCapacity = aship != null ? aship.Capacity : 0;
            var sCapacity = sship != null ? sship.Capacity : 0;
            UpdateTransferItemInterface<Inhabitance>(stypes, sCounts, sCapacity, aCounts, aCapacity, aship == sship );
        }
        else if(type == "Resources")
        {
            var aCounts = aship != null ? aship.Resources : null;
            var sCounts = sship != null ? sship.Resources : null;
            var aCapacity = aship != null ? aship.ResourceCapacity : 0;
            var sCapacity = sship != null ? sship.ResourceCapacity : 0;
            UpdateTransferItemInterface<Resource>(rtypes, sCounts, sCapacity, aCounts, aCapacity, aship == sship);
        }
    }

    private void UpdateTransferItemInterface<T>(List<T> types, Dictionary<T, int> sCounts, int sCapacity, Dictionary<T, int> aCounts, int aCapacity, bool same)
    {
        foreach (var type in types)
        {
            var str = type.ToString();

            if (sCounts != null)
            {
                _interface[str + "RightCount"].GetComponent<Text>().text = sCounts[type].ToString();
            }
            if (aCounts != null)
            {
                _interface[str + "LeftCount"].GetComponent<Text>().text = aCounts[type].ToString();
            }

            if (sCounts != null && aCounts != null)
            {
                var aShipFull = Count<T>(aCounts) == aCapacity;
                var sShipFull = Count<T>(sCounts) == sCapacity;

                _interface[str + "Right"].GetComponent<Button>().interactable =
                    _interface[str + "RightAll"].GetComponent<Button>().interactable = aCounts[type] > 0 && !sShipFull && !same;
                _interface[str + "Left"].GetComponent<Button>().interactable =
                    _interface[str + "LeftAll"].GetComponent<Button>().interactable = sCounts[type] > 0 && !aShipFull && !same;
            }
            else
            {
                _interface[str + "Right"].GetComponent<Button>().interactable =
                    _interface[str + "RightAll"].GetComponent<Button>().interactable =
                    _interface[str + "Left"].GetComponent<Button>().interactable =
                    _interface[str + "LeftAll"].GetComponent<Button>().interactable = false;
            }
        }
    }

    private int Count<T>(Dictionary<T, int> items)
    {
        int i = 0;
        foreach (var item in items)
            i += item.Value;
        return i;
    }

    public void NewSquad()
    {
        var squad = HumanPlayer.Instance.CreateNewSquad(HumanPlayer.Instance.Squad);
        var listing = (ListableObject)squad;

        listing.CreateListEntry("AltSquadList", HumanPlayer.Instance.Squad.Colliders.Count - 1, true).transform.SetParent(_interface["AltSquadList"].transform);
        UpdateTransferInterface(true, true, false, true);
    }

    public Ship TransferShip(Squad from, string fromName, Squad to, string toName)
    {
        var fromIndex = _indices[fromName];
        var ship = from.Ships[fromIndex];
        from.Ships.RemoveAt(fromIndex);

        if((ship.ShipProperties & ShipProperties.Untransferable) > 0)
            return ship;

        to.Ships.Add(ship);
        return null;
    }

    public void TransferAllShips(Squad from, string fromName, Squad to, string toName)
    {
        var untransferables = new List<Ship>();
        while(from.Ships.Count > 0)
        {
            AutoSelectIndex<Ship>(fromName, from.Ships);
            var ship = TransferShip(from, fromName, to, toName);
            if (ship != null)
                untransferables.Add(ship);
        }

        from.Ships.AddRange(untransferables);
    }

    public void ShipTransfer(string data)
    {
        var newString = Regex.Replace(data, "([a-z])([A-Z])", "$1 $2");
        var s = newString.Split(' ').ToList();

        var fromIndex = "AltShipList";
        var from = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]];
        var toIndex = "SelectedShipList";
        var to = HumanPlayer.Instance.Squad;
        if (s[0] == "Left")
        {
            toIndex = "AltShipList";
            to = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]];
            fromIndex = "SelectedShipList";
            from = HumanPlayer.Instance.Squad;
        }

        if(s.Count == 2 && s[1] == "All")
        {
            TransferAllShips(from, fromIndex, to, toIndex);
        }
        else
        {
            var ship = TransferShip(from, fromIndex, to, toIndex);
            if (ship != null) from.Ships.Insert(_indices[fromIndex], ship);
        }

        UpdateTransferInterface(false, true, true, true);
    }

    public void SoldierTransfer(string data)
    {
        var newString = Regex.Replace(data, "([a-z])([A-Z])", "$1 $2");
        var s = newString.Split(' ').ToList();
        if (s.Count >= 3 && s[1] != "Right" && s[1] != "Left") 
            { s[0] += s[1]; s.RemoveAt(1); }
        var type = (Inhabitance)Enum.Parse(typeof(Inhabitance), s[0]);

        var from = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]].Ships[_indices["AltShipList"]];
        var to = HumanPlayer.Instance.Squad.Ships[_indices["SelectedShipList"]];
        if(s[1] == "Left")
        {
            to = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]].Ships[_indices["AltShipList"]];
            from = HumanPlayer.Instance.Squad.Ships[_indices["SelectedShipList"]];
        }

        if (s.Count == 3 && s[2] == "All")
        {
            // this can be done better
            while (from.Population[type] > 0 && to.CountPopulation() < to.Capacity)
            {
                from.Population[type]--;
                to.Population[type]++;
            }
        }
        else
        {
            from.Population[type]--;
            to.Population[type]++;
        }

        UpdateTransferInterface(false, true, true, true);
    }

    public void ResourceTransfer(string data)
    {
        var newString = Regex.Replace(data, "([a-z])([A-Z])", "$1 $2");
        var s = newString.Split(' ').ToList();
        var resource = (Resource)Enum.Parse(typeof(Resource), s[0]);
        
        // assume going right
        Ship from = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]].Ships[_indices["AltShipList"]];
        Ship to = HumanPlayer.Instance.Squad.Ships[_indices["SelectedShipList"]];
        if(s[1] == "Left")
        {
            to = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]].Ships[_indices["AltShipList"]];
            from = HumanPlayer.Instance.Squad.Ships[_indices["SelectedShipList"]];
        }

        if(s.Count == 3 && s[2] == "All")
        {
            while(from.Resources[resource] > 0 && to.CountResources() < to.ResourceCapacity)
            {
                if (from.IsDeployed == true)
                    HumanPlayer.Instance.RemoveResources(from, resource, 1);
                else
                    from.Resources[resource] -= 1;

                if (to.IsDeployed)
                    HumanPlayer.Instance.AddResources(to, resource, 1);
                else
                    to.Resources[resource] += 1;
            }
        }
        else
        {
            if (from.IsDeployed == true)
                HumanPlayer.Instance.RemoveResources(from, resource, 1);
            else
                from.Resources[resource] -= 1;

            if (to.IsDeployed)
                HumanPlayer.Instance.AddResources(to, resource, 1);
            else
                to.Resources[resource] += 1;
        }

        UpdateTransferInterface(false, true, true, true);
    }

    public void ExitManage()
    {
        var squad = HumanPlayer.Instance.Squad;
        squad.Colliders.Remove(squad); // remove self
        if (squad.Tile != null && squad.Tile.IsInRange(squad)) // remove any deployed structures
            squad.Tile.Squad.Ships.Remove(squad.Tile.Structure);

        ClearList("AltSquadList");
        ClearList("AltShipList");
        ClearList("SelectedShipList");
        HumanPlayer.Instance.CleanSquad(HumanPlayer.Instance.Squad);
        HumanPlayer.Instance.ReloadGameplayUI();
        AutoSelectIndex<Ship>("MainShipList", HumanPlayer.Instance.Squad.Ships);
    }

    public void ConfigureBattleScreen(float WC, Squad player, Squad enemy, BattleType battleType)
    {
        var pt = player.GetComponent<Tile>();
        var et = enemy.GetComponent<Tile>();

        _interface["BattleChance"].GetComponent<Text>().text = WC.ToString("P");
        _interface["PlayerBattleIcon"].GetComponent<Image>().sprite = _icons[player.Team.ToString()];
        _interface["EnemyBattleIcon"].GetComponent<Image>().sprite = _icons[enemy.Team.ToString()];

        ClearList("PlayerSquadInfoList");
        ClearList("EnemySquadInfoList");

        var ptext = _interface["BattleMenu"].transform.FindChild("PlayerShipText").GetComponent<Text>();
        var etext = _interface["BattleMenu"].transform.FindChild("EnemyShipText").GetComponent<Text>();

        if (battleType == BattleType.Invasion && pt != null)
            _interface["Retreat"].GetComponent<Button>().interactable = false;
        else
        {
            var unmoveable = player.Ships.Count(s => (s.ShipProperties & ShipProperties.Untransferable) != 0);

            if(unmoveable < player.Ships.Count)
                _interface["Retreat"].GetComponent<Button>().interactable = true;
            else
                _interface["Retreat"].GetComponent<Button>().interactable = false;
        }

        if (battleType == BattleType.Invasion)
        {
            ptext.text = "Your Soldiers";
            etext.text = "Their Soldiers";

            if (pt != null)
            {
                pt.PopulateCountList(_interface["PlayerSquadInfoList"].gameObject, battleType);
                enemy.PopulateCountList(_interface["EnemySquadInfoList"].gameObject, battleType);
            }
            else
            {
                et.PopulateCountList(_interface["EnemySquadInfoList"].gameObject, battleType);
                player.PopulateCountList(_interface["PlayerSquadInfoList"].gameObject, battleType);
            }
        }
        else
        {
            ptext.text = "Your Ships";
            etext.text = "Their Ships";

            player.PopulateCountList(_interface["PlayerSquadInfoList"].gameObject, battleType);
            enemy.PopulateCountList(_interface["EnemySquadInfoList"].gameObject, battleType);
        }

        SetUIElements(false, false, true, false, false, false, false);
    }

    public void Battle()
    {
        PlaySound("Battle");
        var winner = HumanPlayer.Instance.Battle(0f, BattleType.Invasion, null, null);

        if(winner.Key.Key == HumanPlayer.Instance.Team)
        {
            _interface["BattleWon"].gameObject.SetActive(true);
            ClearList("ShipsLostList");
            var squadEntry = Resources.Load<GameObject>("ShipCountListing");
            foreach(var lost in winner.Value)
            {
                var entry = Instantiate(squadEntry) as GameObject;
                entry.transform.FindChild("Name").GetComponent<Text>().text = lost.Key;

                entry.transform.FindChild("Icon").GetComponent<Image>().sprite = _icons[lost.Key];
                entry.transform.FindChild("Count").FindChild("Number").GetComponent<Text>().text = lost.Value.ToString();
                entry.transform.SetParent(_interface["ShipsLostList"].transform);
            }
        }
        else if(winner.Key.Key == Team.Uninhabited) // draw
        {

        }
        else
        {
            _interface["BattleLost"].gameObject.SetActive(true);

            if(winner.Key.Value == BattleType.Space)
            {
                _interface["ShipsText"].gameObject.SetActive(true);
                _interface["SoldiersText"].gameObject.SetActive(false);
            }
            else
            {
                _interface["ShipsText"].gameObject.SetActive(false);
                _interface["SoldiersText"].gameObject.SetActive(true);
            }
        }

        SetUIElements(false, false, false, false, false, false, false);
    }

    public void ContinueAfterBattle(bool win)
    {
        if (win)
        {
            _interface["BattleWon"].gameObject.SetActive(false);
        }
        else
        {
            _interface["BattleLost"].gameObject.SetActive(false);
        }
        HumanPlayer.Instance.EndBattleConditions(win);
    }


    public void Retreat()
    {
        HumanPlayer.Instance.CreateRetreatEvent();
        HumanPlayer.Instance.ReloadGameplayUI();
        GameManager.Instance.Paused = false;
    }

    public void SetZoom(string data, bool redraw)
    {
        var z = _indices["Zoom"];
        if (data != null)
        {
            if (data == "ZoomOut")
                z++;
            else
                z--;
        }

        var m = MapManager.Instance.Minimap;

        var maxw = 256 + 256 * z;
        var maxh = 192 + 192 * z;

        var maxedw = maxw > m.width;
        var maxedh = maxh > m.height;
        if(!maxedw && !maxedh)
            _indices["Zoom"] = Mathf.Clamp(z, 0, 5);

        var i = _interface["ZoomIn"].GetComponent<Button>();
        var o = _interface["ZoomOut"].GetComponent<Button>();

        if (z != 0 && z == _indices["Zoom"])
            i.interactable = true;
        else
            i.interactable = false;

        if (z != 4 && z == _indices["Zoom"])
            o.interactable = true;
        else
            o.interactable = false;

        if (redraw)
            HumanPlayer.Instance.Control(HumanPlayer.Instance.Squad.gameObject);
    }

    public void UpdateMinimap(Texture2D texture, Vector3 position, Sector sector)
    {
        var image = _interface["Minimap"].GetComponent<RawImage>();
        image.texture = texture;

        var zoom = _indices["Zoom"];

        var maxw = 256 + 256 * zoom;
        var maxh = 192 + 192 * zoom;

        var left = 0.5f - (maxw / 2 / (float)texture.width);
        if (left < 0f)
            left = 0f;
        var top = 0.5f - (maxh / 2 / (float)texture.height);
        if (top < 0f)
            top = 0f;

        image.uvRect = new Rect(left, top,
            maxw / (float)texture.width, maxh / (float)texture.height);
    }

    public void AddEvent(string e)
    {
        var l = _interface["EventList"];
        if (l.transform.childCount == 50)
            GameObject.Destroy(l.transform.GetChild(49));

        var eventEntry = Resources.Load<GameObject>("EventListing");
        var entry = Instantiate(eventEntry) as GameObject;
        entry.transform.FindChild("Text").GetComponent<Text>().text = e;
        entry.transform.SetParent(l.transform);
        entry.transform.SetSiblingIndex(0);
    }
}
