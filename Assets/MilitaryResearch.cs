using UnityEngine;
using System.Collections;

public abstract class MilitaryResearch : MonoBehaviour {

	public bool unlocked;
	public double bonusArmor;
	public double bonusAsterminiumPlating;
	public double bonusThrusters;
	public double bonusPlasmas;
	public double bonusTorpedoes;
	public double bonusCapacity;
	public int currArmor = 0;
	public int currAsterminiumPlating = 0;
	public int currThrusters = 0;
	public int currPlasmas = 0;
	public int currTorpedoes = 0;
	public int currCapacity = 0;
	public int maxPoints = 10;
	public double hull;
	public double firepower;
	public double speed;
	public double capacty;
	public double escapeDeath = 0;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public abstract void Armor();
	public abstract void AsterminiumPlating();
	public abstract void Thrusters();
	public abstract void Plasmas();
	public abstract void Torpedoes();
	public abstract void Capacity();
}
