using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for all events. Defines functions crucial to frequent and timed calls for proper event handling in a queue.
/// </summary>
public class GameEvent 
{
    protected GameEventStage _stage; // the event's current stage
    protected int _remainingTurns; // turns until this event is invalid

    public GameEventStage Stage { get { return _stage; } }
    
    /// <summary>
    /// Base constructor for any game event. 
    /// </summary>
    /// <param name="turns">The number of turns until this event expires.</param>
    public GameEvent(int turns)
    {
        _remainingTurns = turns;
        _stage = GameEventStage.Begin;
    }

	/// <summary>
	/// Called once per turn for each event. Advances it to the next stage as appropriate.
	/// </summary>
	public virtual void Progress () 
    {
        _remainingTurns--;
        _stage = (_remainingTurns <= 0 ? GameEventStage.End : GameEventStage.Continue);
	}

    /// <summary>
    /// Updates events continually. This is for, say, cosmetic stuff.
    /// </summary>
    public virtual void Update()
    {

    }

    /// <summary>
    /// Used by the event system to ensure this event is still valid, otherwise bugs will occur.
    /// </summary>
    /// <returns>Boolean indicating if this event is still valid.</returns>
    public virtual bool AssertValid()
    {
        return true;
    }
}
