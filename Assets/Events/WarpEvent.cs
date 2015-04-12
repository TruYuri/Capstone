using UnityEngine;
using System.Collections;

public class WarpEvent : GameEvent
{
    private Tile _exitPortal;
    private Squad _squad;

    public WarpEvent(int turns, Squad squad, Tile exit) 
        : base(turns)
    {
        _exitPortal = exit;
        _squad = squad;
        _squad.Mission = this;
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0)
            return;

        _squad.Mission = null;
        var r = _exitPortal.Radius;
        var val = GameManager.Generator.Next(2);
        var offset = val == 0 ? new Vector3(r, 0, 0) : new Vector3(0, 0, r);
        _squad.transform.position = _exitPortal.transform.position + offset;
    }

    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null && _exitPortal != null && _exitPortal.gameObject != null && _squad.Mission == this)
        {
            return true;
        }

        return false;
    }
}
