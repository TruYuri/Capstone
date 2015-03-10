using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
    private const string PLAYER_PREFAB = "Player";

    private static GameManager _instance;
    private GameObject _player;

    public static GameManager Instance { get { return _instance; } }

	// Use this for initialization
	void Start () 
    {
	    // create player
        var playerObj = Resources.Load<GameObject>(PLAYER_PREFAB);
        _player = Instantiate(playerObj) as GameObject;

        // create command ship - move this to automatic game functionality later?
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
