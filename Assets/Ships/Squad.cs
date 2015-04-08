using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Squad : MonoBehaviour, ListableObject
{
    private const string SQUAD_LIST_PREFAB = "SquadListing";
    private const string SQUAD_COUNT_PREFAB = "ShipCountListing";
    private const string SECTOR_TAG = "Sector";
    private const string SQUAD_TAG = "Squad";
    private const string TILE_TAG = "Tile";

    private List<Ship> _ships = new List<Ship>();
    private Team _team;
    private Tile _currentTile;
    private bool _inTileRange;
    private bool _permanentSquad;
    private Sector _currentSector;
    private List<Squad> _collidingSquads = new List<Squad>();
    private string _name = "Squad";
    private bool _onMission;

    public Team Team
    {
        get { return _team; }
        set { _team = value; }
    }
    public bool OnMission 
    { 
        get { return _onMission; }
        set { _onMission = value; }
    }

    public List<Ship> Ships { get { return _ships; } }
    public List<Squad> Colliders { get { return _collidingSquads; } }
    public Tile Tile { get { return _currentTile; } }
    public Sector Sector { get { return _currentSector; } }

	// Use this for initialization
	void Start () 
    {
	}

    public void Init(Sector sector, string name)
    {
        var tile = this.GetComponent<Tile>();
        if (tile != null)
        {
            _currentTile = tile;
            _inTileRange = _permanentSquad = true;
        }

        _currentSector = sector;
        _name = name;
    }

    // Update is called once per frame
    void Update()
    {
        if(!_permanentSquad && !GameManager.Instance.Paused)
            CheckSectorTile();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance.Paused)
            return;

        // the only colliders are squads, so we can simplify stuff
        var squad = collision.collider.GetComponent<Squad>();
        if (squad == null)
            return;
            
        switch(collision.collider.tag)
        {
            case TILE_TAG:
            case SQUAD_TAG:
                if (!_collidingSquads.Contains(squad))
                    _collidingSquads.Add(squad);

                if (_team != squad.Team)
                    GameManager.Instance.Players[_team].CreateBattleEvent(this, squad);
                break;
        }
    }

    void OnCollisionStay(Collision collision)
    {
    }

    void OnCollisionExit(Collision collision)
    {
        if (GameManager.Instance.Paused)
            return;

        // the only colliders are squads, so we can simplify stuff

        var squad = collision.collider.GetComponent<Squad>();
        if (squad == null)
            return;

        switch (collision.collider.tag)
        {
            case SQUAD_TAG:
                _collidingSquads.Remove(squad);
                break;
        }
    }

    private void CheckSector(Sector sector)
    {
        if (_currentSector == null || (sector.transform.position - this.transform.position).sqrMagnitude
            < (_currentSector.transform.position - this.transform.position).sqrMagnitude)
        {
            if (_currentSector != null)
                _currentSector.GetComponent<Renderer>().material.color = Color.white;
            _currentSector = sector;
            CheckSectorTile();
            _currentSector.GetComponent<Renderer>().material.color = Color.green;
            _currentSector.GenerateNewSectors();
            // GameManager.Instance.Players[this.Team].EndTurn();
        }
    }

    private void CheckSectorTile()
    {
        // raycast down to find sector
        var hits = Physics.RaycastAll(new Ray(this.transform.position + Vector3.up * 50f, Vector3.down));
        foreach(var col in hits)
        {
            var sector = col.collider.GetComponent<Sector>();
            if (sector != null && _currentSector != sector)
            {
                _currentSector = col.collider.gameObject.GetComponent<Sector>();
                _currentSector.GenerateNewSectors();
            }
        }

        if (_currentSector == null)
            return;

        var tile = _currentSector.GetTileAtPosition(transform.position);

        if (tile != _currentTile && _currentTile != null)
        {
            _collidingSquads.Remove(_currentTile.Squad);
            _currentTile.Squad.Colliders.Remove(this);
        }

        var wasInRange = _inTileRange;

        _currentTile = tile;
        if (_currentTile == null)
            return;

        if (_currentTile.IsInRange(this))
        {
            _inTileRange = true;

            if (!_collidingSquads.Contains(_currentTile.Squad) && _currentTile.Squad.Team == _team)
            {
                _collidingSquads.Add(_currentTile.Squad);
                _currentTile.Squad.Colliders.Add(this);
            }
            else if (_currentTile.Squad.Team != _team && _currentTile.Squad.Ships.Count > 0 && !wasInRange)
            {
                GameManager.Instance.Players[_team].CreateBattleEvent(this, _currentTile.Squad);
            }
        }
        else
        {
            _collidingSquads.Remove(_currentTile.Squad);
            _currentTile.Squad.Colliders.Remove(this);
            _inTileRange = false;
            HumanPlayer.Instance.ReloadGameplayUI();
        }

        if(_inTileRange != wasInRange && HumanPlayer.Instance.Squad == this)
            HumanPlayer.Instance.ReloadGameplayUI();
    }

    public float CalculateTroopPower()
    {
        float primitive = 0, industrial = 0, spaceAge = 0;

        foreach (var ship in _ships)
        {
            primitive += ship.PrimitivePopulation;
            industrial += ship.IndustrialPopulation;
            spaceAge += ship.SpaceAgePopulation;
        }

        return primitive + industrial * 1.5f + spaceAge * 1.75f;
    }

    public float CalculateShipPower()
    {
        float hull = 0, firepower = 0, speed = 0;

        foreach(var ship in _ships)
        {
            hull += ship.Hull;
            firepower += ship.Firepower;
            speed += ship.Speed;
        }

        return firepower * 2.0f + speed * 1.5f + hull;
    }

    public float GenerateWinChance(Squad enemy)
    {
        var power = CalculateShipPower();
        var enemyPower = enemy.CalculateShipPower();

        return (power - enemyPower) / 100.0f * 0.5f + 0.5f;
    }

    public float GenerateWinChance(Tile enemy)
    {
        var power = CalculateTroopPower();
        var enemyPower = enemy.CalculateDefensivePower();

        return (power - enemyPower) / 100.0f * 0.5f + 0.5f;
    }

    public KeyValuePair<Team, Dictionary<string, int>> Combat(Squad enemy, float winChance)
    {
        float winP = (float)GameManager.Generator.NextDouble();

        var winner = (winP < winChance ? this : enemy);
        var lost = new KeyValuePair<Team, Dictionary<string, int>>(winner.Team, new Dictionary<string,int>());

        if (this == winner) enemy.Ships.Clear();
        else _ships.Clear();

        float hull = 0;
        foreach (var ship in _ships)
        {
            hull += ship.Hull;
        }

        float damage = Mathf.Floor(hull * (1.0f - winChance) * ((float)GameManager.Generator.NextDouble() * (1.25f - 0.75f) + 0.75f));

        int prim = 0, ind = 0, space = 0;
        while(damage > 0.0f && _ships.Count > 0)
        {
            var randPos = GameManager.Generator.Next(0, _ships.Count);
            var ship = _ships[randPos];

            if (ship.Hull <= damage)
            {
                damage -= ship.Hull;
                float saveChance = (float)GameManager.Generator.NextDouble();

                if (saveChance >= _ships[randPos].Protection)  // add speedy = safer thing here
                {
                    if (!lost.Value.ContainsKey(ship.Name))
                        lost.Value.Add(ship.Name, 0);
                    _ships.Remove(ship);
                    lost.Value[ship.Name]++;
                    prim += ship.PrimitivePopulation;
                    ind += ship.IndustrialPopulation;
                    space += ship.SpaceAgePopulation;
                }
            }
            else
                damage = 0.0f;
        }

        if (prim > 0)
            lost.Value.Add("Primitive", prim);
        if (ind > 0)
            lost.Value.Add("Industrial", ind);
        if (space > 0)
            lost.Value.Add("Space Age", space);

        return lost;
    }

    public KeyValuePair<Team, Dictionary<string, int>> Combat(Tile enemy, float winChance) // planet combat
    {
        float winP = (float)GameManager.Generator.NextDouble();
        var winner = winP < winChance;
        var lost = new KeyValuePair<Team, Dictionary<string, int>>(winner ? _team : enemy.Team, new Dictionary<string,int>());
        lost.Value.Add("Primitive", 0);
        lost.Value.Add("Industrial", 0);
        lost.Value.Add("Space Age", 0);

        if (winner) // remove random soldiers from random ships in the fleet
        {
            enemy.Population = enemy.Population / 2; // squad won, so halve the planet population.

            int nTroops = 0;
            foreach (var ship in _ships)
            {
                nTroops += ship.PrimitivePopulation + ship.SpaceAgePopulation + ship.IndustrialPopulation;
            }

            float damage = Mathf.Floor(nTroops * (1.0f - winChance) * ((float)GameManager.Generator.NextDouble() * (1.25f - 0.75f) + 0.75f));

            while (damage >= 1.0f && nTroops > 0)
            {
                var randShip = GameManager.Generator.Next(0, _ships.Count);
                // somehow distribute probability around # troops and from which ships

                while (_ships[randShip].PrimitivePopulation + _ships[randShip].IndustrialPopulation + _ships[randShip].SpaceAgePopulation == 0)
                    randShip = GameManager.Generator.Next(0, _ships.Count);

                damage -= 1.0f;
                var randSoldier = GameManager.Generator.Next(0, _ships[randShip].PrimitivePopulation + 
                    _ships[randShip].IndustrialPopulation + _ships[randShip].SpaceAgePopulation);
                float saveChance = (float)GameManager.Generator.NextDouble();
                if(randSoldier < _ships[randShip].PrimitivePopulation)
                {
                    if(saveChance >= 0.2f)
                    {
                        _ships[randShip].PrimitivePopulation--;
                        lost.Value["Primitive"]++;
                        nTroops--;
                    }
                }
                else if(randSoldier < _ships[randShip].IndustrialPopulation)
                {
                    if (saveChance >= 0.1f)
                    {
                        _ships[randShip].IndustrialPopulation--;
                        lost.Value["Industrial"]++;
                        nTroops--;
                    }
                }
                else
                {
                    _ships[randShip].SpaceAgePopulation--;
                    lost.Value["Space Age"]++;
                    nTroops--;
                }
            }
        }
        else // tile won.
        {
            this.Ships.Clear();
            // before changing local population, kill all the soldiers in the fleet that lost
            foreach (var ship in _ships)
            {
                ship.PrimitivePopulation = ship.SpaceAgePopulation = ship.IndustrialPopulation = 0;
            }

            int primPop = 0, indPop = 0, spacePop = 0;

            // tile won, kill off soldiers lost in battle
            if(enemy.Team == Team.Indigenous)
            {
                switch(enemy.PopulationType)
                {
                    case Inhabitance.Primitive:
                        primPop = enemy.Population;
                        break;
                    case Inhabitance.Industrial:
                        indPop = enemy.Population;
                        break;
                    case Inhabitance.SpaceAge:
                        spacePop = enemy.Population;
                        break;
                }
            }
            else if(enemy.Structure != null)
            {
                primPop = enemy.Structure.PrimitivePopulation;
                indPop = enemy.Structure.IndustrialPopulation;
                spacePop = enemy.Structure.SpaceAgePopulation;
            }

            int nTroops = primPop + indPop + spacePop;
            float damage = Mathf.Floor(nTroops * (1.0f - winChance) * ((float)GameManager.Generator.NextDouble() * (1.25f - 0.75f) + 0.75f));
            while (damage >= 1.0f && nTroops > 0)
            {
                var randSoldier = GameManager.Generator.Next(0, nTroops);
                damage -= 1.0f;
                float saveChance = (float)GameManager.Generator.NextDouble();
                if(randSoldier < primPop)
                {
                    if(saveChance >= 0.2f)
                    {
                        nTroops--;
                        primPop--;
                        lost.Value["Primitive"]++;
                    }
                }
                else if(randSoldier < indPop)
                {
                    if (saveChance >= 0.1f)
                    {
                        nTroops--;
                        indPop--;
                        lost.Value["Industrial"]++;
                    }
                }
                else
                {
                    nTroops--;
                    spacePop--;
                    lost.Value["Space Age"]++;
                }
            }

            if (enemy.Team == Team.Indigenous)
            {
                enemy.Population = nTroops;
                if (nTroops == 0)
                    enemy.Relinquish();
            }
            else
            {
                enemy.Structure.PrimitivePopulation = primPop;
                enemy.Structure.IndustrialPopulation = indPop;
                enemy.Structure.SpaceAgePopulation = spacePop;
            }
        }

        if (lost.Value["Primitive"] == 0)
            lost.Value.Remove("Primitive");
        if (lost.Value["Industrial"] == 0)
            lost.Value.Remove("Industrial");
        if (lost.Value["Space Age"] == 0)
            lost.Value.Remove("Space Age");

        return lost;
    }

    public Tile Deploy(Structure structure, Tile tile)
    {
        _currentTile = tile;
        if (_currentTile != null) // planetary deploy
            _currentTile.Deploy(structure, ShipProperties.GroundStructure, _team);
        else // space deploy
        {
            _currentTile = _currentSector.CreateTileAtPosition(structure.Name, transform.position);
            _currentTile.Deploy(structure, ShipProperties.SpaceStructure, _team);
            _currentSector.RegisterSpaceStructure(_team, structure);
        }

        _ships.Remove(structure);
        return _currentTile;
    }

    public static void CleanSquadsFromList(Player player, List<Squad> squads)
    {
        var emptySquads = new List<Squad>();
        foreach (var squad in squads)
        {
            if ((squad == null || squad.gameObject == null || (squad.Ships.Count == 0 && squad.GetComponent<Tile>() == null)) && squad.Team == player.Team)
                emptySquads.Add(squad);
        }

        foreach (var squad in emptySquads)
        {
            squads.Remove(squad);
            player.DeleteSquad(squad);
        }
    }

    GameObject ListableObject.CreateListEntry(string listName, int index, System.Object data)
    {
        var squadEntry = Resources.Load<GameObject>(SQUAD_LIST_PREFAB);
        var entry = Instantiate(squadEntry) as GameObject;
        var tile = this.GetComponent<Tile>();

        var name = _name;

        if (this == HumanPlayer.Instance.Squad && listName == "AltSquadList")
            name = "(Self) " + name;

        entry.transform.FindChild("Text").GetComponent<Text>().text = name;
        entry.GetComponent<CustomUIAdvanced>().data = listName + "|" + index.ToString();

        return entry;
    }

    GameObject ListableObject.CreateBuildListEntry(string listName, int index, System.Object data) { return null; }

    void ListableObject.PopulateBuildInfo(GameObject popUp, System.Object data)
    {
    }

    void ListableObject.PopulateGeneralInfo(GameObject popUp, System.Object data)
    {
        PopulateCountList(popUp.transform.FindChild("ShipCounts").FindChild("ShipCountsList").gameObject);
    }

    public Dictionary<string, int> CountShips()
    {
        var counts = new Dictionary<string, int>();
        foreach (var ship in _ships)
        {
            if (!counts.ContainsKey(ship.Name))
                counts.Add(ship.Name, 0);
            counts[ship.Name]++;
        }

        return counts;
    }

    public void PopulateCountList(GameObject list)
    {
        var squadEntry = Resources.Load<GameObject>(SQUAD_COUNT_PREFAB);
        var counts = CountShips();

        foreach (var count in counts)
        {
            var entry = Instantiate(squadEntry) as GameObject;
            entry.transform.FindChild("Name").GetComponent<Text>().text = count.Key;
            entry.transform.FindChild("Icon").GetComponent<Image>().sprite = HumanPlayer.Instance.GetShipDefinition(count.Key).Icon;
            entry.transform.FindChild("Count").FindChild("Number").GetComponent<Text>().text = count.Value.ToString();
            entry.transform.SetParent(list.transform);
        }
    }
}
