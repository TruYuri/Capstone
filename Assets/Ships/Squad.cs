using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
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
    private GameEvent _mission;

    public Team Team
    {
        get { return _team; }
        set { _team = value; }
    }
    public GameEvent Mission 
    { 
        get { return _mission; }
        set { _mission = value; }
    }

    public List<Ship> Ships { get { return _ships; } }
    public List<Squad> Colliders { get { return _collidingSquads; } }
    public Tile Tile { get { return _currentTile; } }
    public Sector Sector { get { return _currentSector; } }

	// Use this for initialization
	void Start () 
    {
	}

    public void Init(Team team, Sector sector, string name)
    {
        var tile = this.GetComponent<Tile>();
        if (tile != null)
        {
            _currentTile = tile;
            _inTileRange = _permanentSquad = true;
        }

        _team = team;
        _currentSector = sector;
        this.name = name;

        if(_currentTile == null)
        {
            var c = GameManager.Instance.PlayerColors[_team];
            this.GetComponent<Renderer>().material.SetColor("_Color", new Color(c.r, c.g, c.b, 0.5f));
            var icon = this.transform.FindChild("Icon").GetComponent<ParticleSystem>();
            icon.GetComponent<Renderer>().material.mainTexture = GUIManager.Instance.Icons[_team.ToString()].texture;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!_permanentSquad && !GameManager.Instance.Paused)
            CheckSectorTile();
        if (!_permanentSquad && _team != HumanPlayer.Instance.Team && !GameManager.Instance.Paused)
            ((AIPlayer)GameManager.Instance.Players[_team]).UpdateAI(this);
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

    private void CheckSectorTile()
    {
        // raycast down to find sector
        var hits = Physics.RaycastAll(new Ray(this.transform.position + Vector3.up * 50f, Vector3.down));
        foreach(var col in hits)
        {
            var sector = col.collider.GetComponent<Sector>();
            if (sector != null && _currentSector != sector)
            {
                var oldSector = _currentSector;
                _currentSector = col.collider.gameObject.GetComponent<Sector>();
                var human = HumanPlayer.Instance;
                if (_team == human.Team)
                {
                    if (human.CommandSquad == this && oldSector != null)
                        human.EndTurn();

                    _currentSector.GenerateNewSectors();

                    if(human.Squad == this)
                        human.Control(this.gameObject);
                }
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

        foreach (var t in GameManager.Instance.Players)
        {
            if (t.Key == HumanPlayer.Instance.Team || t.Key == _team)
                continue;
            ((AIPlayer)t.Value).SetSquadBehaviors(_currentSector, this);
        }

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
        }

        if(_inTileRange != wasInRange && HumanPlayer.Instance.Squad == this && !GameManager.Instance.Paused)
            HumanPlayer.Instance.ReloadGameplayUI();
    }

    public float CalculateTroopPower()
    {
        float primitive = 0, industrial = 0, spaceAge = 0;

        foreach (var ship in _ships)
        {
            primitive += ship.Population[Inhabitance.Primitive];
            industrial += ship.Population[Inhabitance.Industrial];
            spaceAge += ship.Population[Inhabitance.SpaceAge];
        }

        return (primitive + industrial * 1.5f + spaceAge * 1.75f) * (_team == Team.Kharkyr ? 1.15f : 1f);
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

        return (firepower * 2.0f + speed * 1.5f + hull) * (_team == Team.Kharkyr ? 1.15f : 1f);
    }

    public float GenerateWinChance(Squad enemy)
    {
        var power = CalculateShipPower();
        var enemyPower = enemy.CalculateShipPower();

        //return (power - enemyPower) / 100.0f * 0.5f + 0.5f;
		return power / (power + enemyPower);
    }

    public float GenerateWinChance(Tile enemy)
    {
        var power = CalculateTroopPower();
        var enemyPower = enemy.CalculateDefensivePower();

        //return (power - enemyPower) / 100.0f * 0.5f + 0.5f;
		return power / (power + enemyPower);
    }

    public KeyValuePair<Team, Dictionary<string, int>> Combat(Squad enemy, float winChance)
    {
        float winP = (float)GameManager.Generator.NextDouble();

        var winner = (winP < winChance ? this : enemy);
        var lost = new KeyValuePair<Team, Dictionary<string, int>>(winner.Team, new Dictionary<string,int>());

        if (this == winner) 
            GameManager.Instance.Players[enemy.Team].RemoveAllShips(enemy);
        else
            GameManager.Instance.Players[_team].RemoveAllShips(this);

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
                    lost.Value[ship.Name]++;
                    prim += ship.Population[Inhabitance.Primitive];
                    ind += ship.Population[Inhabitance.Industrial];
                    space += ship.Population[Inhabitance.SpaceAge];
                    GameManager.Instance.Players[winner.Team].RemoveShip(winner, _ships[randPos]);
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
            lost.Value.Add("SpaceAge", space);

        return lost;
    }

    public KeyValuePair<Team, Dictionary<string, int>> Combat(Tile enemy, float winChance) // planet combat
    {
        float winP = (float)GameManager.Generator.NextDouble();
        var winner = winP < winChance;
        var lost = new KeyValuePair<Team, Dictionary<string, int>>(winner ? _team : enemy.Team, new Dictionary<string,int>());
        var types = new List<Inhabitance>() { Inhabitance.Primitive, Inhabitance.Industrial, Inhabitance.SpaceAge };
        var populationLoss = new Dictionary<Inhabitance, int>();
        foreach(var type in types)
        {
            populationLoss.Add(type, 0);
        }

        if (winner) // remove random soldiers from random ships in the fleet
        {
            var p = enemy.Population;

            if (enemy.PopulationType != Inhabitance.Uninhabited)
            {
                GameManager.Instance.Players[enemy.Team].RemoveSoldiers(enemy, true, enemy.PopulationType, p);
                GameManager.Instance.Players[_team].AddSoldiers(enemy, enemy.PopulationType, p / 2);
            }

            int nTroops = 0;
            foreach (var ship in _ships)
            {
                nTroops += ship.CountPopulation();
            }

            float damage = Mathf.Floor(nTroops * (1.0f - winChance) * ((float)GameManager.Generator.NextDouble() * (1.25f - 0.75f) + 0.75f));

            while (damage >= 1.0f && nTroops > 0)
            {
                var randShip = GameManager.Generator.Next(0, _ships.Count);

                while (_ships[randShip].CountPopulation() == 0)
                    randShip = GameManager.Generator.Next(0, _ships.Count);

                damage -= 1.0f;
                var randSoldier = GameManager.Generator.Next(0, _ships[randShip].CountPopulation());
                float saveChance = (float)GameManager.Generator.NextDouble();

                for(int i = 0; i < types.Count; i++)
                {
                    if (randSoldier < _ships[randShip].Population[types[i]] && saveChance >= 0.3f - i * 0.1f)
                    {
                        GameManager.Instance.Players[_team].RemoveSoldiers(_ships[randShip], types[i], 1);
                        populationLoss[types[i]]++;
                        nTroops--;
                        break;
                    }
                }
            }
        }
        else // tile won.
        {
            // before changing local population, kill all the soldiers in the fleet that lost
            foreach (var ship in _ships)
            {
                foreach(var type in types)
                {
                    GameManager.Instance.Players[_team].RemoveSoldiers(ship, type, ship.Population[type]);
                    ship.Population[type] = 0;
                }
            }

            // tile won, kill off soldiers lost in battle
            int nTroops = enemy.Population;
            foreach (var type in types)
            {
                if(enemy.Structure != null)
                    nTroops += enemy.Structure.Population[type];

                nTroops += populationLoss[type];
            }

            float damage = Mathf.Floor(nTroops * (1.0f - winChance) * ((float)GameManager.Generator.NextDouble() * (1.25f - 0.75f) + 0.75f));
            while (damage >= 1.0f && nTroops > 0)
            {
                var randSoldier = GameManager.Generator.Next(0, nTroops);
                damage -= 1.0f;
                float saveChance = (float)GameManager.Generator.NextDouble();

                for(int i = 0; i < types.Count; i++)
                {
                    if(randSoldier < populationLoss[types[i]] && saveChance >= 0.3f - i * 0.1f)
                    {
                        nTroops--;
                        populationLoss[types[i]]++;
                    }
                }
            }

            foreach (var type in types)
            {
                GameManager.Instance.Players[enemy.Team].RemoveSoldiers(enemy, true, type, populationLoss[type]);
            }
        }

        foreach(var type in types)
        {
            if (populationLoss[type] > 0)
                lost.Value.Add(type.ToString(), populationLoss[type]);
        }

        return lost;
    }

    public Tile Deploy(Structure structure, Tile tile)
    {
        _currentTile = tile;
        if (_currentTile != null) // planetary deploy
            _currentTile.Deploy(structure, ShipProperties.GroundStructure, _team);
        else // space deploy
        {
            _currentTile = _currentSector.CreateTileAtPosition(structure.Name, transform.position, _team);
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

        var name = this.name;

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
        PopulateCountList(popUp.transform.FindChild("ShipCounts").FindChild("ShipCountsList").gameObject, BattleType.Space | BattleType.Invasion);
    }

    private Dictionary<string, int> CountShips()
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

    public Dictionary<string, int> CountSoldiers()
    {
        var counts = new Dictionary<string, int>();

        foreach (var ship in _ships)
        {
            foreach (var i in ship.Population)
            {
                if (!counts.ContainsKey(i.Key.ToString()) && i.Value > 0)
                    counts.Add(i.Key.ToString(), i.Value);
                else if (counts.ContainsKey(i.Key.ToString()))
                    counts[i.Key.ToString()] += i.Value;
            }
        }

        return counts;
    }

    public void PopulateCountList(GameObject list, BattleType bType)
    {
        var squadEntry = Resources.Load<GameObject>(SQUAD_COUNT_PREFAB);

        if ((bType & BattleType.Space) != 0)
        {
            var counts = CountShips();

            foreach (var count in counts)
            {
                var entry = Instantiate(squadEntry) as GameObject;
                entry.transform.FindChild("Name").GetComponent<Text>().text = count.Key;
                entry.transform.FindChild("Icon").GetComponent<Image>().sprite = GUIManager.Instance.Icons[count.Key];
                entry.transform.FindChild("Count").FindChild("Number").GetComponent<Text>().text = count.Value.ToString();
                entry.transform.SetParent(list.transform);
            }
        }

        if ((bType & BattleType.Invasion) != 0)
        {
            var counts = CountSoldiers();

            foreach (var count in counts)
            {
                var entry = Instantiate(squadEntry) as GameObject;
                entry.transform.FindChild("Name").GetComponent<Text>().text = count.Key;
                entry.transform.FindChild("Icon").GetComponent<Image>().sprite = GUIManager.Instance.Icons[count.Key];
                entry.transform.FindChild("Count").FindChild("Number").GetComponent<Text>().text = count.Value.ToString();
                entry.transform.SetParent(list.transform);
            }
        }
    }
}
