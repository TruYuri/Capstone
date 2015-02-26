using UnityEngine;
using System.Collections;

public class GuardSatelliteResearch : MilitaryResearch {
	
	// Use this for initialization
	void Start () {
		
		unlocked = true;
		baseHull = 2;
		baseFirepower = 3;
		baseSpeed = 0;
		baseCapacity = 0;
		
		hull = baseHull;
		firepower = baseFirepower;
		speed = baseSpeed;
		capacity = baseCapacity;
		
		bonusArmor = .5;
		bonusAsterminiumPlating = .02;
		bonusPlasmas = 1;
		bonusTorpedoes = .02;
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
		//Display nothing or Redded out
	}
	override public void Plasmas()
	{
		if(currPlasmas < maxPoints)
		{
			currPlasmas++;
			FireCalc();
		}
	}
	override public void Torpedoes()
	{
		if(currTorpedoes < maxPoints)
		{
			currTorpedoes++;
			FireCalc();
		}
	}
	override public void Capacity()
	{
		//Display nothing or Redded out
	}
	private void FireCalc()
	{
		firepower = baseFirepower + (currPlasmas * bonusPlasmas) + (currTorpedoes * bonusTorpedoes);
	}
	
}

