using UnityEngine;
using System.Collections;

public class FighterResearch : MilitaryResearch {

	// Use this for initialization
	void Start () {

		unlocked = true;
		hull = 1;
		firepower = 1;
		speed = 5;
		capacty = 0;
		escapeDeath = 0;
		bonusArmor = .25;
		bonusAsterminiumPlating = .02;
		bonusThrusters = 1;
		bonusPlasmas = .25;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public double getHull()
	{return hull;}

	public double getFirepower()
	{return firepower;}

	public double getSpeed()
	{return speed;}

	public double getCapacity()
	{return capacty;}

	public double getEscapeDeath()
	{return escapeDeath;}

	override public void Armor()
	{
		currArmor++;
		hull = hull + (currArmor * bonusArmor);
	}
	override public void AsterminiumPlating()
	{
		currAsterminiumPlating++;
		escapeDeath = escapeDeath + (currAsterminiumPlating * bonusAsterminiumPlating);
		
	}
	override public void Thrusters()
	{
		currThrusters++;
		speed = speed + (currThrusters * bonusThrusters);
	}
	override public void Plasmas()
	{
		currPlasmas++;
		firepower = firepower + (currPlasmas * bonusPlasmas);
	}
	override public void Torpedoes()
	{
		//Display nothing or Redded out
	}
	override public void Capacity()
	{
		//Display nothing or Redded out
	}

}
