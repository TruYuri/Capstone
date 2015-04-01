using UnityEngine;
using System.Collections;

public class UndeployEvent : GameEvent
{
    private Tile _tile;

    public UndeployEvent(int turns, Tile tile)
        : base(turns)
    {
        _tile = tile;
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns != 0)
            return;

        _tile.Undeploy(false);
    }
}
