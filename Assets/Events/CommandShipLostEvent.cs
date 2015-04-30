using UnityEngine;
using System.Linq;
using System.Collections;

public class CommandShipLostEvent : GameEvent 
{
    private Player _player;
    private Vector3 _lost;

    public CommandShipLostEvent(Vector3 lost, Squad squad, Player player) : base(5)
    {
        _player = player;
        _lost = lost;
        if (player == HumanPlayer.Instance)
            GUIManager.Instance.AddEvent("Command Ship lost!");
    }

    public override void Progress()
    {
        _player.EndTurn();
        base.Progress();

        if (_remainingTurns > 0)
            return;

        _player.CreateNewCommandShip(_player.Tiles.OrderBy(t => (_lost - t.transform.position).sqrMagnitude).ToList()[0]);
    }
}
