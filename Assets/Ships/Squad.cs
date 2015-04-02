using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Squad : MonoBehaviour, ListableObject
{
    private const string SQUAD_LIST_PREFAB = "SquadListing";
    private const string SECTOR_TAG = "SectorCollider";
    private const string SQUAD_TAG = "Squad";
    private const string TILE_TAG = "Tile";

    private List<Ship> _ships = new List<Ship>();
    private Team _team;
    private float _shipPower;
    private float _troopPower;
    private bool _isControlled;
    private Tile _currentTile;
    private bool _inTileRange;
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
    public int Size { get { return _ships.Count; } }
    public float ShipPower { get { return _shipPower; } }
    public float TroopPower { get { return _troopPower; } }
    public bool IsPlayerControlled 
    {
        get { return _isControlled; }
        set { _isControlled = value; }
    }

	// Use this for initialization
	void Start () 
    {
        var tile = this.GetComponent<Tile>();
        if (tile != null)
            _currentTile = tile;
	}

    // Update is called once per frame
    void Update()
    {
        CheckSectorTile(_currentSector);
    }

    private void CheckSectorTile(Sector sector)
    {
        if (sector == null)
            return;
        
        var tile = _currentSector.GetTileAtPosition(transform.position);

        if (tile != _currentTile)
        {
            if (_currentTile != null)
            {
                _collidingSquads.Remove(_currentTile.Squad);
                _currentTile.Squad.Colliders.Remove(this);
            }
            if(tile != null)
            {
                if ((transform.position - tile.transform.position).sqrMagnitude <= (tile.Radius * tile.Radius))
                {
                    _collidingSquads.Add(tile.Squad);
                    tile.Squad.Colliders.Add(this);
                    _inTileRange = true;
                }
                else
                    _inTileRange = false;
            }
            _currentTile = tile;
        }
        else if(_currentTile != null)
        {
            if ((transform.position - tile.transform.position).sqrMagnitude <= (tile.Radius * tile.Radius))
            {
                if (!_collidingSquads.Contains(tile.Squad))
                {
                    _collidingSquads.Add(tile.Squad);
                    _inTileRange = true;
                }
                if (!tile.Squad.Colliders.Contains(this))
                    tile.Squad.Colliders.Add(this);
            }
            else
            {
                _collidingSquads.Remove(tile.Squad);
                tile.Squad.Colliders.Remove(this);
                _inTileRange = false;
            }
        }
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

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == SECTOR_TAG)
            CheckSector(other.transform.parent.GetComponent<Sector>());
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == SECTOR_TAG)
            CheckSector(other.transform.parent.GetComponent<Sector>());
    }

    void OnTriggerExit(Collider other)
    {
    }

    private void CheckSector(Sector sector)
    {
        if (_currentSector == null || (sector.transform.position - this.transform.position).sqrMagnitude
            < (_currentSector.transform.position - this.transform.position).sqrMagnitude)
        {
            if (_currentSector != null)
                _currentSector.GetComponent<Renderer>().material.color = Color.white;
            _currentSector = sector;
            CheckSectorTile(_currentSector);
            _currentSector.GetComponent<Renderer>().material.color = Color.green;
            MapManager.Instance.GenerateNewSectors(_currentSector);
            // GameManager.Instance.Players[this.Team].EndTurn();
        }
    }

    public void CalculatePower()
    {
        float hull = 0, firepower = 0, speed = 0;
        float primitive = 0, industrial = 0, spaceAge = 0;

        foreach(var ship in _ships)
        {
            hull += ship.Hull;
            firepower += ship.Firepower;
            speed += ship.Speed;
            primitive += ship.PrimitivePopulation;
            industrial += ship.IndustrialPopulation;
            spaceAge += ship.SpaceAgePopulation;
        }

        _shipPower = firepower * 2.0f + speed * 1.5f + hull;
        _troopPower = primitive + industrial * 1.5f + spaceAge * 1.75f;
    }

    public void AddShip(Ship ship)
    {
        _ships.Add(ship);
    }

    public void AddShip(Ship ship, int index)
    {
        _ships.Insert(index, ship);
    }

    public Ship RemoveShip(int index)
    {
        var ship = _ships[index];
        _ships.Remove(ship);
        return ship;
    }

    public void RemoveShip(Ship ship)
    {
        _ships.Remove(ship);
    }

    public Team Combat(Squad squad)
    {
        CalculatePower();
        squad.CalculatePower();

        float winC = (_shipPower - squad.ShipPower) / 100.0f * 0.5f + 0.5f;
        float winP = (float)GameManager.Generator.NextDouble();

        var winner = (winP < winC ? this : squad);

        float hull = 0;
        foreach (var ship in _ships)
        {
            hull += ship.Hull;
        }

        float damage = Mathf.Floor(hull * (1.0f - winC) * ((float)GameManager.Generator.NextDouble() * (1.25f - 0.75f) + 0.75f));

        while(damage > 0.0f && Size > 0)
        {
            var randPos = GameManager.Generator.Next(0, Size);
            var ship = _ships[randPos];

            if (ship.Hull <= damage)
            {
                damage -= ship.Hull;
                float saveChance = (float)GameManager.Generator.NextDouble();

                if (saveChance >= _ships[randPos].Protection)  // add speedy = safer thing here
                    RemoveShip(ship);
            }
            else
                damage = 0.0f;
        }

        return winner.Team;
    }

    public Team Combat(Tile tile) // planet combat
    {
        CalculatePower();
        tile.CalculatePower();

        float winC = (_troopPower - tile.Power) / 100.0f * 0.5f + 0.5f;
        float winP = (float)GameManager.Generator.NextDouble();

        var winner = winP < winC;

        if (winner) // remove random soldiers from random ships in the fleet
        {
            int nTroops = 0;
            foreach (var ship in _ships)
            {
                nTroops += ship.Population;
            }

            float damage = Mathf.Floor(nTroops * (1.0f - winC) * ((float)GameManager.Generator.NextDouble() * (1.25f - 0.75f) + 0.75f));

            while (damage >= 1.0f && nTroops > 0)
            {
                var randShip = GameManager.Generator.Next(0, Size);
                // somehow distribute probability around # troops and from which ships

                while(_ships[randShip].Population == 0)
                    randShip = GameManager.Generator.Next(0, Size);

                damage -= 1.0f;
                var randSoldier = GameManager.Generator.Next(0, _ships[randShip].Population);
                float saveChance = (float)GameManager.Generator.NextDouble();
                if(randSoldier < _ships[randShip].PrimitivePopulation)
                {
                    if(saveChance >= 0.2f)
                    {
                        _ships[randShip].PrimitivePopulation--;
                        _ships[randShip].Population--;
                        nTroops--;
                    }
                }
                else if(randSoldier < _ships[randShip].IndustrialPopulation)
                {
                    if (saveChance >= 0.1f)
                    {
                        _ships[randShip].IndustrialPopulation--;
                        _ships[randShip].Population--;
                        nTroops--;
                    }
                }
                else
                {
                    _ships[randShip].SpaceAgePopulation--;
                    _ships[randShip].Population--;
                    nTroops--;
                }
            }

            return _team;
        }

        return tile.Team;
    }

    public void Deploy(Structure structure, Tile tile)
    {
        _currentTile.Deploy(structure, _team);
        _ships.Remove(structure);
    }

    GameObject ListableObject.CreateListEntry(string listName, int index, System.Object data)
    {
        var squadEntry = Resources.Load<GameObject>(SQUAD_LIST_PREFAB);
        var entry = Instantiate(squadEntry) as GameObject;
        var tile = this.GetComponent<Tile>();
        if (tile != null)
            entry.transform.FindChild("Text").GetComponent<Text>().text = tile.Name + " Defense";
        else
            entry.transform.FindChild("Text").GetComponent<Text>().text = _name;
        entry.GetComponent<CustomUI>().data = listName + "|" + index.ToString();

        return entry;
    }

    GameObject ListableObject.CreateBuildListEntry(string listName, int index, System.Object data) { return null; }

    GameObject ListableObject.CreatePopUpInfo(System.Object data)
    {
        return null;
    }

    public static void CleanSquadsFromList(Player player, List<Squad> squads)
    {
        var emptySquads = new List<Squad>();
        foreach(var squad in squads)
        {
            if ((squad == null || squad.gameObject == null || (squad.Size == 0 && squad.GetComponent<Tile>() == null)) && squad.Team == player.Team)
                emptySquads.Add(squad);
        }

        foreach (var squad in emptySquads)
        {
            squads.Remove(squad);
            if(squad != null && squad.gameObject != null)
                GameObject.DestroyImmediate(squad.gameObject);
            player.Squads.Remove(squad);
        }
    }
}
