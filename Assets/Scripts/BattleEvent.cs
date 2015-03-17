using UnityEngine;
using System.Collections;

public class SquadBattleEvent : GameEvent
{
    private Squad _playerSquad;
    private Squad _enemySquad;

    public SquadBattleEvent(Squad player, Squad enemy) : base(GameEventType.Battle)
    {
        _playerSquad = player;
        _enemySquad = enemy;
    }

    public override void Begin()
    {
        base.Begin();

        GameManager.Instance.Paused = true;
        Player.Instance.Control(_playerSquad.gameObject);
        GUIManager.Instance.SquadCollideSquad(_playerSquad, _enemySquad);
    }
}
