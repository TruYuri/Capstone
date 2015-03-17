using UnityEngine;
using System.Collections.Generic;

public class Squad : MonoBehaviour 
{
    private const string SQUAD_TAG = "Squad";
    private List<Ship> _ships;
    private Team _team;

    public Team Team
    {
        get { return _team; }
        set { _team = value; }
    }

    public int Size { get { return _ships.Count; } }

	// Use this for initialization
	void Start () 
    {
        if (_ships == null)
            _ships = new List<Ship>();
	}

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == SQUAD_TAG)
        {

        }
    }
	
	// Update is called once per frame
	void Update () 
    {
	}

    public void AddShip(Ship ship)
    {
        if (_ships == null)
            _ships = new List<Ship>();
        _ships.Add(ship);
    }

    public void RemoveShip(Ship ship)
    {
        _ships.Remove(ship);
    }
}
