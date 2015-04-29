using UnityEngine;
using System.Collections;

public class CommandShipLostEvent : GameEvent 
{
    private Player _player;
    public CommandShipLostEvent(Squad squad, Player player) : base(5)
    {
        _player = player;

        if (player == HumanPlayer.Instance)
            GUIManager.Instance.AddEvent("Command Ship lost!");
    }

    public override void Progress()
    {
        _player.EndTurn();
        base.Progress();

        if (_remainingTurns > 0)
            return;

        _player.CreateNewCommandShip(_player.Tiles[0]);
    }
}
