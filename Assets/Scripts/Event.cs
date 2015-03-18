using UnityEngine;
using System.Collections;

public class GameEvent 
{
    protected GameEventType _type;
    protected GameEventStage _stage;
    protected int _remainingTurns;

    public GameEventType Type { get { return _type; } }
    public GameEventStage Stage { get { return _stage; } }

    public GameEvent(GameEventType type, int turns)
    {
        _type = type;
        _remainingTurns = turns;
        _stage = GameEventStage.Begin;
    }

	// Use this for initialization
	public virtual void Begin () 
    {
        _remainingTurns--;
        _stage = (_remainingTurns == 0 ? GameEventStage.End : GameEventStage.Continue);
	}
}
