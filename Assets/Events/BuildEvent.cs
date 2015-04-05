using UnityEngine;
using System.Collections;

public class BuildEvent : GameEvent
{
    private Tile _tile;
    private Ship _ship;

    public BuildEvent(int turns, Tile tile, Ship ship)
        : base(turns)
    {
        _tile = tile;
        _ship = ship;
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns != 0)
            return;
        _tile.Squad.Ships.Add(_ship);
    }
}
