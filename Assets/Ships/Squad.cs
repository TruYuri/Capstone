using UnityEngine;
using System.Collections.Generic;

public class Squad : MonoBehaviour 
{
    private const string SQUAD_TAG = "Squad";
    private const string TILE_TAG = "Tile";
    private List<Ship> _ships;
    private Team _team;
    private float _power;

    public Team Team
    {
        get { return _team; }
        set { _team = value; }
    }

    public List<Ship> Ships { get { return _ships; } }
    public int Size { get { return _ships.Count; } }
    public float Power { get { return _power; } }

	// Use this for initialization
	void Start () 
    {
        if (_ships == null)
            _ships = new List<Ship>();
	}

    void OnCollisionEnter(Collision collision)
    {
        switch(collision.collider.tag)
        {
            case TILE_TAG:
                GUIManager.Instance.SquadCollideSquad(this, collision.gameObject.GetComponent<Squad>(), true);
                break;
            case SQUAD_TAG:
                GUIManager.Instance.SquadCollideSquad(this, collision.gameObject.GetComponent<Squad>(), false);
                break;
        }
    }
	
    
	// Update is called once per frame
	void Update () 
    {
	}

    private void RecalculatePower()
    {
        float hull = 0;
        float firepower = 0;
        float speed = 0;

        foreach(var ship in _ships)
        {
            hull += ship.Hull;
            firepower += ship.Firepower;
            speed += ship.Speed;
        }

        _power = firepower * 2.0f + speed * 1.5f + hull;
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
        float winC = (_power - squad.Power) / 100.0f * 0.5f + 0.5f;
        float winP = (float)GameManager.Generator.NextDouble();

        var winner = (winP < winC ? this : squad);
        var loser = (winP < winC ? squad : this);

        float hull = 0;
        foreach (var ship in _ships)
        {
            hull += ship.Hull;
        }

        float damage = Mathf.Floor(hull * (1.0f - winC) * ((float)GameManager.Generator.NextDouble() * (1.25f - 0.75f) + 0.75f));

        while(damage > 0.0f)
        {
            var randPos = GameManager.Generator.Next(0, Size);
            var ship = _ships[randPos];

            if (ship.Hull <= damage)
            {
                damage -= ship.Hull;
                RemoveShip(ship);
            }
            else
                damage = 0.0f;
        }

        Destroy(loser.gameObject);
        return winner.Team;
    }

    public Team Combat(Tile tile) // planet combat
    {
        return Team.None;
    }
}
