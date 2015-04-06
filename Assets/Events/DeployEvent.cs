using UnityEngine;
using System.Collections;

public class DeployEvent : GameEvent
{
    private Structure _structure;
    private Squad _squad;
    private Tile _tile;

    public DeployEvent(int turns, Structure ship, Squad squad, Tile tile) 
        : base(turns)
    {
        _structure = ship;
        _squad = squad;
        _tile = tile;
        _squad.OnMission = true;
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns != 0)
            return;

        _tile = _squad.Deploy(_structure, _tile);
        _squad.OnMission = false;
        if(_squad.Ships.Count == 0 && _tile.Squad != _squad)
        {
            if (HumanPlayer.Instance.Squad == _squad)
                HumanPlayer.Instance.Control(_tile.gameObject);
            var team = _squad.Team;
            GameManager.Instance.Players[_squad.Team].CleanSquad(_squad);
            GameManager.Instance.Players[team].DeleteSquad(_squad);
        }
    }

    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null &&
            _tile.Team == _squad.Team && _squad.Ships.Contains(_structure) &&
            _tile.Structure == null)
            return true;
        return false;
    }
}
