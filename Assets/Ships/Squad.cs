using UnityEngine;
using System.Collections.Generic;

public class Squad : MonoBehaviour 
{
    private const string SQUAD_TAG = "Squad";
    private const string TILE_TAG = "Tile";
    private List<Ship> _ships;
    private Team _team;
    private float _shipPower;
    private float _troopPower;
    private bool _isControlled;
    private Tile _collidingTile;
    private List<Squad> _collidingSquads;

    public Team Team
    {
        get { return _team; }
        set { _team = value; }
    }

    public List<Ship> Ships { get { return _ships; } }
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
        if(_ships == null)
            _ships = new List<Ship>();
        if(_collidingSquads == null)
            _collidingSquads = new List<Squad>();
	}

    void OnCollisionEnter(Collision collision)
    {
        if (_ships == null)
            _ships = new List<Ship>();
        if (_collidingSquads == null)
            _collidingSquads = new List<Squad>();
        // the only colliders are squads, so we can simplify stuff

        var squad = collision.collider.GetComponent<Squad>();
        if (squad == null)
            return;

        bool enemy = Player.Instance.Team != squad.Team;
            
        switch(collision.collider.tag)
        {
            case TILE_TAG:
                _collidingTile = collision.collider.GetComponent<Tile>();
                if (enemy && _team == Player.Instance.Team)
                {
                    if(squad.Size > 0)
                        GameManager.Instance.AddEvent(new BattleEvent(this, squad));
                    else
                        GameManager.Instance.AddEvent(new BattleEvent(this, _collidingTile));
                }
                else if(_isControlled)
                {
                    GUIManager.Instance.SetMainListControls(this, squad, _collidingTile);
                }
                break;
            case SQUAD_TAG:
                _collidingSquads.Add(squad);
                if (enemy && _team == Player.Instance.Team)
                {
                    GameManager.Instance.AddEvent(new BattleEvent(this, squad));
                }
                else if(_isControlled)
                {
                    GUIManager.Instance.SetMainListControls(this, squad, null);
                }
                break;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // the only colliders are squads, so we can simplify stuff

        var squad = collision.collider.GetComponent<Squad>();
        if (squad == null)
            return;

        switch (collision.collider.tag)
        {
            case TILE_TAG:
                if (_isControlled)
                {
                    GUIManager.Instance.SetMainListControls(this, squad, _collidingTile);
                }
                break;
            case SQUAD_TAG:
                if (_isControlled)
                {
                    GUIManager.Instance.SetMainListControls(this, squad, null);
                }
                break;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // the only colliders are squads, so we can simplify stuff

        var squad = collision.collider.GetComponent<Squad>();
        if (squad == null)
            return;

        switch (collision.collider.tag)
        {
            case TILE_TAG:
                _collidingTile = null;
                if (_isControlled)
                {
                    GUIManager.Instance.SetMainListControls(this, squad, collision.collider.GetComponent<Tile>());
                }
                break;
            case SQUAD_TAG:
                _collidingSquads.Remove(squad);
                if (_isControlled)
                {
                    GUIManager.Instance.SetMainListControls(this, squad, null);
                }
                break;
        }
    }
	   
	// Update is called once per frame
	void Update () 
    {
	}

    private void RecalculatePower()
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
        if (_ships == null)
            _ships = new List<Ship>();
        _ships.Add(ship);
        RecalculatePower();
    }

    public void RemoveShip(Ship ship)
    {
        _ships.Remove(ship);
        RecalculatePower();
    }

    public Team Combat(Squad squad)
    {
        float winC = (_shipPower - squad.ShipPower) / 100.0f * 0.5f + 0.5f;
        float winP = (float)GameManager.Generator.NextDouble();

        var winner = (winP < winC ? this : squad);
        var loser = (winP < winC ? squad : this);

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

        if (Size == 0)
            Destroy(winner.gameObject);
        Destroy(loser.gameObject);
        return winner.Team;
    }

    public Team Combat(Tile tile) // planet combat
    {
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

            tile.DestroyBase();
            return _team;
        }

        if (Size == 0)
            Destroy(this.gameObject);
        return tile.Team;
    }

    public Tile Deploy(int shipIndex)
    {
        _collidingTile.Deploy(_ships[shipIndex] as Structure, _team);
        _ships.RemoveAt(shipIndex);
        return _collidingTile;
    }
}
