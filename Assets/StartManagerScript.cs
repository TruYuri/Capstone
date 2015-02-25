using UnityEngine;
using System.Collections;

public class StartManagerScript : MonoBehaviour {

	// Use this for initialization
	public void StartGame()
	{
		Application.LoadLevel("Map");
	}
}
