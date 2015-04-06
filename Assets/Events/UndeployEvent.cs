using UnityEngine;
using System.Collections;

public class UndeployEvent : GameEvent
{
    private Tile _tile;
    private Team _team;
    private bool _destroy;

    public UndeployEvent(int turns, Team team, Tile tile, bool destroy)
        : base(turns)
    {
        _tile = tile;
        _destroy = destroy;
        _team = team;
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns != 0)
            return;

        var type = _tile.Undeploy(_destroy);

        if (MapManager.Instance.DeploySpawnTable.ContainsKey(type))
        {
            var squad = GameManager.Instance.Players[_tile.Team].CreateNewSquad(_tile.transform.position);
            foreach (var ship in _tile.Squad.Ships)
                squad.Ships.Add(ship);
            if (HumanPlayer.Instance.Squad == _tile.Squad)
                HumanPlayer.Instance.Control(squad.gameObject);
            GameManager.Instance.Players[_tile.Team].DeleteSquad(_tile.Squad);
        }
        else
            GameManager.Instance.Players[_tile.Team].Squads.Remove(_tile.Squad);
    }

    public override bool AssertValid()
    {
        if (_tile != null && _tile.Team == _team && _tile.Structure != null)
            return true;
        return false;
    }
}
