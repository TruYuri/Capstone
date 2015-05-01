using UnityEngine;
using System.Reflection;
using System.Collections;

public class BattleEvent : GameEvent
{
    private Squad _squad1;
    private Squad _squad2;
    private GameEvent _m1;
    private GameEvent _m2;
    private BattleType _battleType;

    public BattleEvent(Squad squad1, Squad squad2) : base(1)
    {
        _squad1 = squad1;
        _squad2 = squad2;
        _battleType = BattleType.Space;
        GameManager.Instance.Paused = true;
        Progress();
    }

    public BattleEvent(int turns, Squad squad, Tile tile) : base(turns)
    {
        _squad1 = squad;
        _squad2 = tile.GetComponent<Squad>();
        _battleType = BattleType.Invasion;
        GameManager.Instance.Paused = true;
        if (squad.Team == HumanPlayer.Instance.Team)
        {
            GUIManager.Instance.AddEvent("Transmitting command to invade " + tile.Name + ".");

            if (turns <= 0)
                Progress();
        }
        else if (tile.Team == HumanPlayer.Instance.Team)
            Progress();
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0)
            return;

        _m1 = _squad1.Mission;
        _m2 = _squad2.Mission;

        _squad1.Mission = _squad2.Mission = this;

        GameManager.Instance.Paused = true;
        if (HumanPlayer.Instance.Team == _squad1.Team || HumanPlayer.Instance.Team == _squad2.Team) // if player involved
        {
            // note: unpause when the battle result screen is closed
            HumanPlayer.Instance.PrepareBattleConditions(_squad1, _squad2, _battleType);
        }
        else // else let any AI duke it out
        {
            var WC = GameManager.Instance.Players[_squad1.Team].PrepareBattleConditions(_squad1, _squad2, _battleType);
            GameManager.Instance.Players[_squad1.Team].Battle(WC, _battleType, _squad1, _squad2);
            GameManager.Instance.Paused = false;

            ReplaceMission(_squad1);
            ReplaceMission(_squad2);
        }
    }

    public void ReplaceMission(Squad fix)
    {
        if (fix == null || fix.gameObject == null)
            return;

        if (fix.Mission != null && fix.Mission.GetType() == typeof(RetreatEvent))
            return;

        fix.Mission = (fix == _squad1 ? _m1 : _m2);
    }

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
