using UnityEngine;
using System.Collections;

public class BattleEvent : GameEvent
{
    private Squad _squad1;
    private Squad _squad2;
    private BattleType _battleType;

    public BattleEvent(Squad squad1, Squad squad2) : base(1)
    {
        _squad1 = squad1;
        _squad2 = squad2;
        _battleType = BattleType.Space;
    }

    public BattleEvent(Squad squad, Tile tile) : base(1)
    {
        _squad1 = squad;
        _squad2 = tile.GetComponent<Squad>();
        _battleType = BattleType.Invasion;
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
            HumanPlayer.Instance.PrepareBattleConditions(_squad1, _squad2, _battleType);
        }
        else // else let any AI duke it out
        {
            var WC = GameManager.Instance.Players[_squad1.Team].PrepareBattleConditions(_squad1, _squad2, _battleType);
            GameManager.Instance.Players[_squad1.Team].Battle(WC, _battleType, _squad1, _squad2);
        }
    }

    public override bool AssertValid()
    {
        if(_squad1 != null && _squad2 != null &&
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
