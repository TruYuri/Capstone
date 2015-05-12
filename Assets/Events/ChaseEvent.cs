using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles one squad chasing another.
/// </summary>
public class ChaseEvent : GameEvent 
{
    private Squad _squad;
    private Squad _chase;
    private Tile _tTeather;
    private float _range;
    private float _velocity;
    private float _timeElapsedSpeedIncrease;

    /// <summary>
    /// Constructor for the chase event.
    /// </summary>
    /// <param name="squad"></param>
    /// <param name="chase"></param>
    /// <param name="homeTile"></param>
    /// <param name="range"></param>
    /// <param name="velocity"></param>
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

    /// <summary>
    /// Keeps the chase in constant progression until invalid.
    /// </summary>
    public override void Progress()
    {
        _remainingTurns++;
        base.Progress();
    }

    /// <summary>
    /// Physically moves the chasing squad closer to its target.
    /// </summary>
    public override void Update()
    {
        if (_tTeather != null && (_tTeather.transform.position - _chase.transform.position).sqrMagnitude < _range * _range 
            && _chase.Sector == _squad.Sector)
        {
            _timeElapsedSpeedIncrease += Time.deltaTime;
            _squad.transform.position = Vector3.MoveTowards(_squad.transform.position, _chase.transform.position, _velocity * _timeElapsedSpeedIncrease * Time.deltaTime);
        }
        else if(_tTeather == null && (_squad.transform.position - _chase.transform.position).sqrMagnitude < _range * _range 
            && _chase.Sector == _squad.Sector)
        {
            _timeElapsedSpeedIncrease += Time.deltaTime;
            _squad.transform.position = Vector3.MoveTowards(_squad.transform.position, _chase.transform.position, _velocity * _timeElapsedSpeedIncrease * Time.deltaTime);
        }
        else
            _squad.Mission = null;
    }

    /// <summary>
    /// Ensures the chase is still valid.
    /// </summary>
    /// <returns></returns>
    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null && _chase != null && _chase.gameObject != null && _squad.Mission == this)
            return true;
        if (_squad != null && _squad.gameObject != null)
            _squad.Mission = null;
        return false;
    }
}
