using UnityEngine;
using System.Collections.Generic;

public class RetreatEvent : GameEvent 
{
    private Squad _squad;
    private Squad _from;
    private Player _player;
    // turn parameter = turns until command begins. 
    // calculate travel turns - 1 turn per sector, swap out remaining turns when initial == 0
    public RetreatEvent(Player player, Squad squad, Squad from) : base(1)
    {
        _squad = squad;
        _from = from;
        _squad.Mission = this;
        _player = player;
        if (player == HumanPlayer.Instance)
            GUIManager.Instance.AddEvent("Squad retreated!");
    }

    // when travelling, travel between planets (x = 10x, y = 10y)
    // travel from one waypoint to the next

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

    public override void Update()
    {
    }

    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null && _squad.Mission == this)
            return true;
        return false;
    }
}
