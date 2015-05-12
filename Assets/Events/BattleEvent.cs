using UnityEngine;
using System.Reflection;
using System.Collections;

/// <summary>
/// Handles a registered battle.
/// </summary>
public class BattleEvent : GameEvent
{
    private Squad _squad1;
    private Squad _squad2;
    private BattleType _battleType;

    /// <summary>
    /// Establishes a battle between two squads.
    /// </summary>
    /// <param name="squad1"></param>
    /// <param name="squad2"></param>
    public BattleEvent(Squad squad1, Squad squad2) : base(1)
    {
        _squad1 = squad1;
        _squad2 = squad2;
        _battleType = BattleType.Space;
        GameManager.Instance.Paused = true;
        Progress();
    }

    /// <summary>
    /// Establishes a battle between a squad and a tile.
    /// </summary>
    /// <param name="turns"></param>
    /// <param name="squad"></param>
    /// <param name="tile"></param>
    public BattleEvent(int turns, Squad squad, Tile tile) : base(turns)
    {
        _squad1 = squad;
        _squad2 = tile.GetComponent<Squad>();
        _battleType = BattleType.Invasion;
        GameManager.Instance.Paused = true;
        if (squad.Team == HumanPlayer.Instance.Team)
        {
            GUIManager.Instance.AddEvent("Transmitting command to invade " + tile.Name + ".");

            if (turns <= 1)
                Progress();
        }
        else if (tile.Team == HumanPlayer.Instance.Team)
            Progress();
    }

    /// <summary>
    /// Starts the battle. Pauses the game if the human player is involved, and instead enables the appropriate battle screens.
    /// </summary>
    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0)
            return;

        GameManager.Instance.Paused = true;
        if (HumanPlayer.Instance.Team == _squad1.Team || HumanPlayer.Instance.Team == _squad2.Team) // if player involved
        {
            // note: unpause when the battle result screen is closed
            HumanPlayer.Instance.PrepareBattleConditions(_squad1, _squad2, _battleType);
            _squad1.Mission = _squad2.Mission = this;
        }
        else // else let any AI duke it out
        {
            var WC = GameManager.Instance.Players[_squad1.Team].PrepareBattleConditions(_squad1, _squad2, _battleType);
            GameManager.Instance.Players[_squad1.Team].Battle(WC, _battleType, _squad1, _squad2);
            GameManager.Instance.Paused = false;
        }
    }

    /// <summary>
    /// Asserts the validity of the battle.
    /// </summary>
    /// <returns></returns>
    public override bool AssertValid()
    {
        if(_remainingTurns > 0 && _squad1 != null && _squad2 != null &&
            _squad1.gameObject != null && _squad2.gameObject != null)
        {
            var pt = _squad1.GetComponent<Tile>();
            var et = _squad2.GetComponent<Tile>();


            if (pt != null && pt.Team != _squad2.Team)
                return true;
            else if (et != null && et.Team != _squad1.Team)
                return true;

            if (_squad1.Team == _squad2.Team)
                return false;

            return true;
        }

        return false;
    }
}
