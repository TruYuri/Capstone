using UnityEngine;
using System.Collections.Generic;

public class Squad : MonoBehaviour 
{
    private const string SQUAD_TAG = "Squad";
    private const string TILE_TAG = "Tile";
    private List<Ship> _ships;
    private Team _team;

    public Team Team
    {
        get { return _team; }
        set { _team = value; }
    }

    public List<Ship> Ships { get { return _ships; } }

    public int Size { get { return _ships.Count; } }

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
