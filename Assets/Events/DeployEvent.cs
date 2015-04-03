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

        _tile = _squad.Deploy(_structure, _tile);

        if(_squad.Ships.Count == 0 && _tile.Squad != _squad)
        {
            if (HumanPlayer.Instance.Squad == _squad)
                HumanPlayer.Instance.Control(_tile.gameObject);
            GameManager.Instance.Players[_squad.Team].DeleteSquad(_squad);
        }
    }
}
