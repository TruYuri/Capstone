using UnityEngine;
using System.Collections;

public class FighterResearch : MilitaryResearch {

	// Use this for initialization
	void Start () {

		unlocked = true;
		baseHull = 1;
		baseFirepower = 1;
		baseSpeed = 5;
		baseCapacity = 0;

		hull = baseHull;
		firepower = baseFirepower;
		speed = baseSpeed;
		capacity = baseCapacity;

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
	{return capacity;}

	public double getEscapeDeath()
	{return escapeDeath;}

	override public void Armor()
	{
		if(currArmor < maxPoints)
		{
			currArmor++;
			hull = baseHull + (currArmor * bonusArmor);
		}
	}
	override public void AsterminiumPlating()
	{
		if(currAsterminiumPlating < maxPoints)
		{
			currAsterminiumPlating++;
			escapeDeath = (currAsterminiumPlating * bonusAsterminiumPlating);
		}
	}
	override public void Thrusters()
	{
		if(currThrusters < maxPoints)
		{
			currThrusters++;
			speed = baseSpeed + (currThrusters * bonusThrusters);
		}
	}
	override public void Plasmas()
	{
		if(currPlasmas < maxPoints)
		{
			currPlasmas++;
			firepower = baseFirepower + (currPlasmas * bonusPlasmas);
		}
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
