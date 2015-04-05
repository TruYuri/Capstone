using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Squad : MonoBehaviour, ListableObject
{
    private const string SQUAD_LIST_PREFAB = "SquadListing";
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

    public Team Team
    {
        get { return _team; }
        set { _team = value; }
    }

    public List<Ship> Ships { get { return _ships; } }
    public List<Squad> Colliders { get { return _collidingSquads; } }
    public bool IsInPlanetRange { get { return _inTileRange; } }
    public Tile Tile { get { return _currentTile; } }
    public bool OnMission;

	// Use this for initialization
	void Start () 
    {
	}

    public void Init()
    {
        var tile = this.GetComponent<Tile>();
        if (tile != null)
        {
            _currentTile = tile;
            _inTileRange = _permanentSquad = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!_permanentSquad)
            CheckSectorTile();
    }

    void OnCollisionEnter(Collision collision)
    {
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

                if (_team != squad._team)
                {
                    GameManager.Instance.AddEvent(new BattleEvent(this, squad));
                }
                break;
        }
    }

    void OnCollisionStay(Collision collision)
    {
    }

    void OnCollisionExit(Collision collision)
    {
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

        _currentTile = tile;
        if (_currentTile == null)
            return;

        if (_currentTile.IsInRange(this))
        {
            if (!_collidingSquads.Contains(_currentTile.Squad))
            {
                _collidingSquads.Add(_currentTile.Squad);
                _currentTile.Squad.Colliders.Add(this);
                _inTileRange = true;
            }
        }
        else
        {
            _collidingSquads.Remove(_currentTile.Squad);
            _currentTile.Squad.Colliders.Remove(this);
            _inTileRange = false;
        }

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

    public Team Combat(Squad enemy, float winChance)
    {
        float winP = (float)GameManager.Generator.NextDouble();

        var winner = (winP < winChance ? this : enemy);

        float hull = 0;
        foreach (var ship in _ships)
        {
            hull += ship.Hull;
        }

        float damage = Mathf.Floor(hull * (1.0f - winChance) * ((float)GameManager.Generator.NextDouble() * (1.25f - 0.75f) + 0.75f));

        while(damage > 0.0f && _ships.Count > 0)
        {
            var randPos = GameManager.Generator.Next(0, _ships.Count);
            var ship = _ships[randPos];

            if (ship.Hull <= damage)
            {
                damage -= ship.Hull;
                float saveChance = (float)GameManager.Generator.NextDouble();

                if (saveChance >= _ships[randPos].Protection)  // add speedy = safer thing here
                    _ships.Remove(ship);
            }
            else
                damage = 0.0f;
        }

        return winner.Team;
    }

    public Team Combat(Tile enemy, float winChance) // planet combat
    {
        float winP = (float)GameManager.Generator.NextDouble();

        var winner = winP < winChance;

        if (winner) // remove random soldiers from random ships in the fleet
        {
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
                        nTroops--;
                    }
                }
                else if(randSoldier < _ships[randShip].IndustrialPopulation)
                {
                    if (saveChance >= 0.1f)
                    {
                        _ships[randShip].IndustrialPopulation--;
                        nTroops--;
                    }
                }
                else
                {
                    _ships[randShip].SpaceAgePopulation--;
                    nTroops--;
                }
            }

            return _team;
        }
        else
        {
            int primPop = 0, indPop = 0, spacePop = 0;

            // tile won
            if(enemy.Team == Team.Indigineous)
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
            else
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
                    }
                }
                else if(randSoldier < indPop)
                {
                    if (saveChance >= 0.1f)
                    {
                        nTroops--;
                        indPop--;
                    }
                }
                else
                {
                    nTroops--;
                    spacePop--;
                }
            }

            if (enemy.Team == Team.Indigineous)
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

        return enemy.Team;
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

        if (tile != null)
            name = tile.Name;

        if (this == HumanPlayer.Instance.Squad)
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
    }
}
