using UnityEngine;
using System.Collections;

public class UndeployEvent : GameEvent
{
    private Tile _tile;

    public Tile Tile { get { return _tile; } }

    public UndeployEvent(Tile tile, int turns)
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
        if (_tile == HumanPlayer.Instance.Tile)
            HumanPlayer.Instance.ReloadGameplayUI();
    }
}
