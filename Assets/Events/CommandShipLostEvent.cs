using UnityEngine;
using System.Collections;

public class CommandShipLostEvent : GameEvent 
{
    private Player _player;
    public CommandShipLostEvent(Squad squad, Player player) : base(5)
    {
        _player = player;

        if (squad != null)
            squad.Init(squad.Sector, "Squad");
    }

    public override void Progress()
    {
        _player.EndTurn();
        base.Progress();

        if (_remainingTurns > 0)
            return;

        _player.CreateNewCommandShip();
    }
}
