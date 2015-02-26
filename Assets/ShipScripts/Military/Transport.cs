using UnityEngine;
using System.Collections;

public class Transport : MilitaryShip {

	public int totalLoad;
	public int primitiveLoad;
	public int industrialLoad;
	public int spaceAgeLoad;

	// Use this for initialization
	void Start () {
		//create new Transport
		totalLoad = 0;
		primitiveLoad = 0;
		industrialLoad = 0;
		spaceAgeLoad = 0;
	}
	
	// Update is called once per frame
	void Update () {
		//Display Transport & calculate loads
	}
}
