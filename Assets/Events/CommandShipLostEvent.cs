using UnityEngine;
using System.Linq;
using System.Collections;

/// <summary>
/// Handles when a player's command ship is lost.
/// </summary>
public class CommandShipLostEvent : GameEvent 
{
    private Player _player;
    private Vector3 _lost;

    /// <summary>
    /// Constructor for the CommandShipLostEvent
    /// </summary>
    /// <param name="lost"></param>
    /// <param name="squad"></param>
    /// <param name="player"></param>
    public CommandShipLostEvent(Vector3 lost, Squad squad, Player player) : base(5)
    {
        _player = player;
        _lost = lost;
        if (player == HumanPlayer.Instance)
            GUIManager.Instance.AddEvent("Command Ship lost!");
    }

    /// <summary>
    /// Forces the player to end its turn until the appropriate number have passed.
    /// </summary>
    public override void Progress()
    {
        _player.EndTurn();
        base.Progress();

        if (_remainingTurns > 0)
            return;

        _player.CreateNewCommandShip(_player.Tiles.OrderBy(t => (_lost - t.transform.position).sqrMagnitude).ToList()[0]);
    }
}
