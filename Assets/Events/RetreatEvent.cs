using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles a retreat.
/// </summary>
public class RetreatEvent : GameEvent 
{
    private Squad _squad;
    private Squad _from;
    private Player _player;
    
    /// <summary>
    /// Constructor for RetreatEvent.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="squad"></param>
    /// <param name="from"></param>
    public RetreatEvent(Player player, Squad squad, Squad from) : base(1)
    {
        _squad = squad;
        _from = from;
        _squad.Mission = this;
        _player = player;
        if (player == HumanPlayer.Instance)
            GUIManager.Instance.AddEvent("Squad retreated!");
    }

    /// <summary>
    /// Finds the best location to retreat to, and proceeds with it.
    /// </summary>
    public override void Progress()
    {
        base.Progress();

        var closest = MapManager.Instance.FindNearestSector(_squad.Sector, _squad.transform.position);
        var dir = (closest.transform.position - _squad.Sector.transform.position).normalized;
        var pos = (closest.transform.position + _squad.Sector.transform.position) / 2.0f + dir * 2.0f;

        if(_squad.Tile != null && _squad.Tile.Squad == _squad)
        {
            var sq = _player.CreateNewSquad(pos, closest);

            List<Ship> moved = new List<Ship>();
            foreach(var ship in _squad.Ships)
            {
                if((ship.ShipProperties & ShipProperties.Untransferable) == 0)
                {
                    _player.AddShip(sq, ship);
                    moved.Add(ship);
                }
            }

            foreach (var ship in moved)
                _player.RemoveShip(_squad, ship);

            if(_squad.Ships.Count > 0) // restart battle
            {
                _player.CreateBattleEvent(_squad, _from);
            }
        }
        else
            _squad.transform.position = pos;
        _squad.Mission = null;
    }

    /// <summary>
    /// Ensures the retreat event is still valid.
    /// </summary>
    /// <returns></returns>
    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null && _squad.Mission == this)
            return true;
        return false;
    }
}
