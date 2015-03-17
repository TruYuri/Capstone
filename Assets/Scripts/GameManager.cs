using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
    public static System.Random Generator = new System.Random();
    private const string PLAYER_PREFAB = "Player";

    private static GameManager _instance;
    private GameObject _player;

    public static GameManager Instance 
    { 
        get 
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<GameManager>();
            return _instance; 
        }
    }

	// Use this for initialization
	void Start () 
    {
	    // create player
        var playerObj = Resources.Load<GameObject>(PLAYER_PREFAB);
        _player = Instantiate(playerObj) as GameObject;
        _instance = this;
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void EndTurn()
    {

    }
}
