using UnityEngine;
using System.Collections;

public class DeployEvent : GameEvent
{
    private Structure _structure;
    private Squad _squad;
    private Tile _tile;

    public DeployEvent(int turns, Structure ship, Squad squad, Tile tile) 
        : base(turns)
    {
        _structure = ship;
        _squad = squad;
        _tile = tile;
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns != 0)
            return;

        _squad.Deploy(_structure, _tile);
        GameManager.Instance.Players[_squad.Team].CleanSquad(_squad);
    }
}
