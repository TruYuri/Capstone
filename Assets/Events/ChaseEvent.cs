using UnityEngine;
using System.Collections.Generic;

public class ChaseEvent : GameEvent 
{
    private Squad _squad;
    private Squad _chase;
    private Tile _tTeather;
    private float _range;
    private float _velocity;
    private float _timeElapsedSpeedIncrease;

    // turn parameter = turns until command begins. 
    // calculate travel turns - 1 turn per sector, swap out remaining turns when initial == 0
    public ChaseEvent(Squad squad, Squad chase, Tile homeTile, float range, float velocity) : base(1)
    {
        _squad = squad;
        _chase = chase;
        _squad.Mission = this;
        _velocity = velocity;
        _range = range;
        _tTeather = homeTile;
        _timeElapsedSpeedIncrease = 1f;
    }

    // when travelling, travel between planets (x = 10x, y = 10y)
    // travel from one waypoint to the next

    public override void Progress()
    {
        _remainingTurns++;
        base.Progress();
    }

    public override void Update()
    {
        if ((_tTeather.transform.position - _chase.transform.position).sqrMagnitude < _range * _range)
        {
            _timeElapsedSpeedIncrease += Time.deltaTime;
            _squad.transform.position = Vector3.MoveTowards(_squad.transform.position, _chase.transform.position, _velocity * _timeElapsedSpeedIncrease * Time.deltaTime);
        }
        else
            _squad.Mission = null;
    }

    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null && _chase != null && _chase.gameObject != null && _squad.Mission == this)
            return true;
        if (_squad != null && _squad.gameObject != null)
            _squad.Mission = null;
        return false;
    }
}
