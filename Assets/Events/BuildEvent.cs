using UnityEngine;
using System.Collections;

public class BuildEvent : GameEvent
{
    private Tile _tile;
    private Ship _ship;
    private Team _team;
    private Structure _structure;

    public BuildEvent(int turns, Team team, Tile tile, Ship ship)
        : base(turns)
    {
        _tile = tile;
        _ship = ship;
        _structure = tile.Structure;
        _team = team;
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0)
            return;
        _tile.Squad.Ships.Add(_ship);
    }

    public override bool AssertValid()
    {
        if (_tile.Team == _team && _tile.Structure == _structure)
            return true;
        return false;
    }
}
