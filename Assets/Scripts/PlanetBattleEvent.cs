using UnityEngine;
using System.Collections;

public class PlanetBattleEvent : GameEvent
{
    private Squad _playerSquad;
    private Tile _enemyPlanet;

    public PlanetBattleEvent(Squad player, Tile enemy)
        : base(GameEventType.Battle)
    {
        _playerSquad = player;
        _enemyPlanet = enemy;
    }

    public override void Begin()
    {
        base.Begin();

        GameManager.Instance.Paused = true;
        Player.Instance.Control(_playerSquad);
        GUIManager.Instance.SquadCollideTile(_playerSquad, _enemyPlanet);
    }
}
