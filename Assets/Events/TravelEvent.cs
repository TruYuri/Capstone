using UnityEngine;
using System.Collections.Generic;

public class TravelEvent : GameEvent 
{
    private readonly Vector3 TOP_RIGHT_OFFSET = new Vector3(98.3f, 0.0f, 149.0f);
    private readonly Vector3 RIGHT_OFFSET = new Vector3(196.5f, 0, 0.0f);
    private readonly Vector3 BOTTOM_RIGHT_OFFSET = new Vector3(98.3f, 0.0f, -149.0f);
    private readonly Vector3 BOTTOM_LEFT_OFFSET = new Vector3(-98.3f, 0.0f, -149.0f);
    private readonly Vector3 LEFT_OFFSET = new Vector3(-196.5f, 0, 0);
    private readonly Vector3 TOP_LEFT_OFFSET = new Vector3(-98.3f, 0.0f, 149.0f);

    private Squad _squad;
    private Vector3 _destination;
    private float _velocity;
    private List<Vector3> _turnDestinations;

    public TravelEvent(int turns, Squad squad, Vector3 destination, float velocity) : base(turns)
    {
        _squad = squad;
        _destination = destination;
        _velocity = velocity;
        _squad.OnMission = true;
        // calculate turns = 
    }

    // when travelling, travel between planets (x = 10x, y = 10y)
    // travel from one waypoint to the next

    public override void Progress()
    {
        base.Progress();

        if(_remainingTurns == 0)
        {
            _squad.transform.position = _destination;
            _squad.OnMission = false;
            return;
        }
        
        // force to last waypoint, if applicable

        // calculate new waypoints
        _turnDestinations = new List<Vector3>();
        _turnDestinations.Add(_destination);

        var diff = _squad.transform.position - _destination;
        if (diff.magnitude < 0.1f)
        {
            _turnDestinations.RemoveAt(0);
            _squad.OnMission = false;
        }
    }

    public override void Update()
    {
        if (_turnDestinations.Count == 0)
            return;

        var dir = _turnDestinations[0] - _squad.transform.position;
        dir.Normalize();
        _squad.transform.position += dir * _velocity * Time.deltaTime;

        var diff = _squad.transform.position - _turnDestinations[0];
        if (diff.magnitude < 0.1f)
            _turnDestinations.RemoveAt(0);
    }
}
