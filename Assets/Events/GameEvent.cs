using UnityEngine;
using System.Collections;

public class GameEvent 
{
    protected GameEventStage _stage;
    protected int _remainingTurns;

    public GameEventStage Stage { get { return _stage; } }

    public GameEvent(int turns)
    {
        _remainingTurns = turns;
        _stage = GameEventStage.Begin;
    }

	// Use this for initialization
	public virtual void Progress () 
    {
        _remainingTurns--;
        _stage = (_remainingTurns == 0 ? GameEventStage.End : GameEventStage.Continue);
	}
}
