using UnityEngine;
using System.Collections;

public class DeployEvent : GameEvent
{
    private Player _player;
    private Structure _structure;
    private Squad _squad;
    private Tile _tile;

    public DeployEvent(int turns, Player player, Structure ship, Squad squad, Tile tile) 
        : base(turns)
    {
        _structure = ship;
        _squad = squad;
        _tile = tile;
        _player = player;
        _squad.OnMission = true;
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0)
            return;

        _tile = _squad.Deploy(_structure, _tile);
        _squad.OnMission = false;
        if(_squad.Ships.Count == 0 && _tile.Squad != _squad)
        {
            if (HumanPlayer.Instance == _player)
                _player.Control(_tile.gameObject);
            var team = _squad.Team;
            _player.CleanSquad(_squad);
            _player.DeleteSquad(_squad);
        }
    }

    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null)
        {
            if (_tile != null && (_tile.Team == _squad.Team || _tile.Team == Team.Uninhabited) && _squad.Ships.Contains(_structure))
                return true;
            else if (_tile == null && _squad.Ships.Contains(_structure))
                return true;
        }
        return false;
    }
}
