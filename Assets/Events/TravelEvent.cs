using UnityEngine;
using System.Collections.Generic;

public class TravelEvent : GameEvent 
{
    private Squad _squad;
    private Vector3 _destination;
    private float _velocity;
    private List<Sector> _destinationSectors;
    private List<Vector3> _turnDestinations;
    private int _travelTurns;
    private AudioSource _engine;
    private Player _player;

    // turn parameter = turns until command begins. 
    // calculate travel turns - 1 turn per sector, swap out remaining turns when initial == 0
    /// <summary>
    /// Constructor for a TravelEvent.
    /// </summary>
    /// <param name="turns"></param>
    /// <param name="player"></param>
    /// <param name="squad"></param>
    /// <param name="destinationSector"></param>
    /// <param name="destination"></param>
    /// <param name="velocity"></param>
    public TravelEvent(int turns, Player player, Squad squad, Sector destinationSector, Vector3 destination, float velocity) : base(turns)
    {
        _squad = squad;
        _destination = destination;
        _velocity = velocity;
        _squad.Mission = this;
        _destinationSectors = MapManager.Instance.AStarSearch(squad.Sector, destinationSector);
        _travelTurns = _destinationSectors.Count;
        _engine = _squad.GetComponent<AudioSource>();
        _player = player;

        if (player == HumanPlayer.Instance)
            GUIManager.Instance.AddEvent("Transmitting command to move squad.");
    }

    // when travelling, travel between planets (x = 10x, y = 10y)
    // travel from one waypoint to the next

    /// <summary>
    /// Advances the TravelEvent to the next stage.
    /// </summary>
    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0 && _travelTurns > 0) // waiting for command to reach the squad
            return;

        if(_remainingTurns <= 0 && _travelTurns > 0) // swap to travelling
        {
            _remainingTurns = _travelTurns;
            _travelTurns = 0;
            _stage = GameEventStage.Continue;

            if (_player == HumanPlayer.Instance)
                GUIManager.Instance.AddEvent("Command received, " + _squad.name + " travelling to destination.");
        }
        else if(_remainingTurns <= 0)
        {
            _squad.transform.position = _destination;
            _squad.Mission = null;

            if (_player == HumanPlayer.Instance)
                GUIManager.Instance.AddEvent(_squad.name + " has reached its destination.");

            return;
        }

        if (_turnDestinations != null && _turnDestinations.Count > 0)
            _squad.transform.position = _turnDestinations[_turnDestinations.Count - 1];

        _turnDestinations = new List<Vector3>();

        Sector cur = null;
        if (_destinationSectors.Count > 1)
        {
            cur = _destinationSectors[0];
            _destinationSectors.RemoveAt(0);
        }

        if (_destinationSectors.Count >= 1 && cur != null)
        {
            var next = _destinationSectors[0];

            // add final corner first
            var dir = (cur.transform.position - next.transform.position).normalized;
            _turnDestinations.Add((cur.transform.position + next.transform.position) / 2.0f + dir * 2.0f);

            // determine columns/rows to traverse to reach the corner - avoiding planets

            
        }
        else if (_destinationSectors.Count == 1)
        {
            // add final destination
            _turnDestinations.Add(_destination);

            // determine coumns/rows to traverse to reach the destination - avoiding planets
        }
    }

    /// <summary>
    /// Visual updates for the TravelEvent, moves the associated squad.
    /// </summary>
    public override void Update()
    {
        if (_travelTurns > 0 || _turnDestinations == null || _turnDestinations.Count == 0)
        {
            _engine.Stop();
            return;
        }

        if (!_engine.isPlaying)
            _engine.Play();
        _squad.transform.position = Vector3.MoveTowards(_squad.transform.position, _turnDestinations[0], _velocity * Time.deltaTime);

        var diff = _squad.transform.position - _turnDestinations[0];
        if (diff.magnitude < 0.1f)
            _turnDestinations.RemoveAt(0);
    }

    /// <summary>
    /// Asserts validity of the TravelEvent.
    /// </summary>
    /// <returns></returns>
    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null && _squad.Mission == this)
            return true;
        return false;
    }
}
