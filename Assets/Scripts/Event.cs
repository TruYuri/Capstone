using UnityEngine;
using System.Collections;

public class GameEvent 
{
    private GameEventType _type;
    private bool _started;

    public bool Started { get { return _started; } }

    public GameEvent(GameEventType type)
    {
        _type = type;
    }

	// Use this for initialization
	public virtual void Begin () 
    {
        _started = true;
	}
}
