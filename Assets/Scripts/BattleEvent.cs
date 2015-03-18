using UnityEngine;
using System.Collections;

public class BattleEvent : GameEvent
{
    private GameObject _playerSquad;
    private GameObject _enemySquad;

    public GameObject Player { get { return _playerSquad; } }
    public GameObject Enemy { get { return _enemySquad; } }

    public BattleEvent(Squad player, Squad enemy) : base(GameEventType.SquadBattle, 1)
    {
        _playerSquad = player.gameObject;
        _enemySquad = enemy.gameObject;
    }

    public BattleEvent(Squad player, Tile enemy) : base(GameEventType.PlanetBattle, 1)
    {
        _playerSquad = player.gameObject;
        _enemySquad = enemy.gameObject;
    }

    public override void Begin()
    {
        GameManager.Instance.Paused = true;
        //Player.Instance.Control(_playerSquad);

        if(_type == GameEventType.SquadBattle)
            GUIManager.Instance.SquadCollideSquad(_playerSquad.GetComponent<Squad>(), _enemySquad.GetComponent<Squad>());
        else
            GUIManager.Instance.SquadCollideTile(_playerSquad.GetComponent<Squad>(), _enemySquad.GetComponent<Tile>());
        base.Begin();
    }
}
