using UnityEngine;
using System.Collections;

public class DeployEvent : GameEvent
{
    private Structure _structure;
    private Squad _squad;
    private Tile _tile;

    public Structure Structure { get { return _structure; } }
    public Squad Squad { get { return _squad; } }
    public Tile Tile { get { return _tile; } }

    public DeployEvent(Structure ship, Squad squad, Tile tile, int turns) : base(turns)
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
        if (_squad == HumanPlayer.Instance.Squad)
            HumanPlayer.Instance.ReloadGameplayUI();
        GameManager.Instance.Players[_squad.Team].CleanSquad(_squad);
    }
}
