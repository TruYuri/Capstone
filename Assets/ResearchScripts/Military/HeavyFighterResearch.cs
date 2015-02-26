using UnityEngine;
using System.Collections;

public class HeavyFighterResearch : MilitaryResearch {
	
	// Use this for initialization
	void Start () {
		
		unlocked = true;
		baseHull = 3;
		baseFirepower = 3;
		baseSpeed = 2;
		baseCapacity = 10;
		
		hull = baseHull;
		firepower = baseFirepower;
		speed = baseSpeed;
		capacity = baseCapacity;
		
		bonusArmor = .5;
		bonusAsterminiumPlating = .02;
		bonusThrusters = .5;
		bonusPlasmas = .75;
		bonusTorpedoes = .02;
		bonusCapacity = 10;
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
		if(currCapacity < maxPoints)
		{
			currCapacity++;
			capacity = baseCapacity + (currCapacity * bonusCapacity);
		}
	}
	private void FireCalc()
	{
		firepower = baseFirepower + (currPlasmas * bonusPlasmas) + (currTorpedoes * bonusTorpedoes);
	}
	
}

