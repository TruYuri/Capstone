using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles a diplomacy request.
/// </summary>
public class DiplomacyEvent : GameEvent 
{
    private Tile _tile;
    private int _diplomacyTurns;
    private Player _player;

    /// <summary>
    /// Constructor for the DiplomacyEvent
    /// </summary>
    /// <param name="turns"></param>
    /// <param name="player"></param>
    /// <param name="tile"></param>
    public DiplomacyEvent(int turns, Player player, Tile tile) : base(turns)
    {
        _player = player;
        _tile = tile;
        _diplomacyTurns = 5;
        tile.SetDiplomaticEffort(player.Team);

        if (player == HumanPlayer.Instance)
            GUIManager.Instance.AddEvent("Transmitting command to begin diplomacy at " + tile.Name + ".");
    }

    /// <summary>
    /// Continues the diplomacy attempt until finished.
    /// </summary>
    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0 && _diplomacyTurns > 0) // waiting for command to reach the squad
            return;

        if (_remainingTurns <= 0 && _diplomacyTurns > 0) // swap to diplomacy
        {
            _remainingTurns = _diplomacyTurns;
            _diplomacyTurns = 0;
            _stage = GameEventStage.Continue;
            if (_player == HumanPlayer.Instance)
                GUIManager.Instance.AddEvent("Beginning diplomacy at " + _tile.Name + ".");
        }
        else if(_remainingTurns <= 0)
        {
            _player.DiplomaticEffort(_tile);
            return;
        }
    }

    /// <summary>
    /// Ensures the diplomacy is still worth pursuing.
    /// </summary>
    /// <returns></returns>
    public override bool AssertValid()
    {
        if (_tile.Team == Team.Indigenous)
            return true;
        return false;
    }
}
