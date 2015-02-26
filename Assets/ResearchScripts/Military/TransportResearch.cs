using UnityEngine;
using System.Collections;

public class TransportResearch : MilitaryResearch {
	
	// Use this for initialization
	void Start () {
		
		unlocked = true;
		baseHull = 10;
		baseFirepower = 0;
		baseSpeed = 2;
		baseCapacity = 100;
		
		hull = baseHull;
		firepower = baseFirepower;
		speed = baseSpeed;
		capacity = baseCapacity;
		
		bonusArmor = 2;
		bonusAsterminiumPlating = .02;
		bonusThrusters = .5;
		bonusCapacity = 100;
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
		//Display nothing or Redded out
	}
	override public void Torpedoes()
	{
		//Display nothing or Redded out
	}
	override public void Capacity()
	{
		//Display nothing or Redded out
		if(currCapacity < maxPoints)
		{
			currCapacity++;
			capacity = baseCapacity + (currCapacity * bonusCapacity);
		}
	}
	
}
