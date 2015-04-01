using UnityEngine;
using System.Collections;

public class BattleEvent : GameEvent
{
    private Squad _squad1;
    private Squad _squad2;

    public Squad Squad1 { get { return _squad1; } }
    public Squad Squad2 { get { return _squad2; } }

    public BattleEvent(Squad squad1, Squad squad2) : base(GameEventType.SquadBattle, 1)
    {
        _squad1 = squad1;
        _squad2 = squad2;
    }

    public BattleEvent(Squad squad, Tile tile) : base(GameEventType.PlanetBattle, 1)
    {
        _squad1 = squad;
        _squad2 = tile.GetComponent<Squad>();
    }

    public override void Begin()
    {
        GameManager.Instance.Paused = true;

        if (HumanPlayer.Instance.Team == _squad1.Team || HumanPlayer.Instance.Team == _squad2.Team)
        {
            if (_type == GameEventType.SquadBattle)
                GUIManager.Instance.SquadCollideSquad(_squad1, _squad2);
            else
                GUIManager.Instance.SquadCollideTile(_squad1, _squad2.GetComponent<Tile>());
        }
        base.Begin();
    }
}
