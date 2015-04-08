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
            var squad = GameManager.Instance.Players[_tile.Team].CreateNewSquad(_tile.transform.position, _tile.Squad.Sector);
            foreach (var ship in _tile.Squad.Ships)
                squad.Ships.Add(ship);
            if (HumanPlayer.Instance.Squad == _tile.Squad)
                HumanPlayer.Instance.Control(squad.gameObject);
            GameManager.Instance.Players[_team].Squads.Remove(_tile.Squad);
            GameManager.Instance.Players[_team].DeleteSquad(_tile.Squad);
            GameManager.Instance.Players[_team].Tiles.Remove(_tile);
        }
    }

    public override bool AssertValid()
    {
        if (_destroy && _tile != null && _tile.Structure != null)
            return true;
        else if (_tile != null && _tile.Team == _team && _tile.Structure != null)
            return true;
        return false;
    }
}
