using UnityEngine;
using System.Collections;

public class BattleEvent : GameEvent
{
    private Squad _squad1;
    private Squad _squad2;

    public BattleEvent(Squad squad1, Squad squad2) : base(1)
    {
        _squad1 = squad1;
        _squad2 = squad2;
    }

    public BattleEvent(Squad squad, Tile tile) : base(1)
    {
        _squad1 = squad;
        _squad2 = tile.GetComponent<Squad>();
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns != 0)
            return;

        if (HumanPlayer.Instance.Team == _squad1.Team || HumanPlayer.Instance.Team == _squad2.Team) // if player involved
        {
            // note: unpause when the battle result screen is closed
            GameManager.Instance.Paused = true;
            HumanPlayer.Instance.PrepareBattleConditions(_squad1, _squad2);
        }
        else // else let any AI duke it out
        {
            var WC = GameManager.Instance.Players[_squad1.Team].PrepareBattleConditions(_squad1, _squad2);
            GameManager.Instance.Players[_squad1.Team].Battle(WC, _squad1, _squad2);
        }
    }
}
