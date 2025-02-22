﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Handles an undeploy request.
/// </summary>
public class UndeployEvent : GameEvent
{
    private Tile _tile;
    private bool _destroy;
    private Player _player;

    /// <summary>
    /// Constructor for UndeployEvent
    /// </summary>
    /// <param name="turns"></param>
    /// <param name="player"></param>
    /// <param name="tile"></param>
    /// <param name="destroy"></param>
    public UndeployEvent(int turns, Player player, Tile tile, bool destroy)
        : base(turns)
    {
        _tile = tile;
        _destroy = destroy;
        _player = player;
        if (_player == HumanPlayer.Instance)
        {
            if(_tile.Type != _tile.Structure.Name)
                GUIManager.Instance.AddEvent("Transmitting command to undeploy " + _tile.Structure.Name + " at " + _tile.Name);
            else
                GUIManager.Instance.AddEvent("Transmitting command to undeploy " + _tile.Structure.Name);
        }
    }

    /// <summary>
    /// Progresses turns until the undeploy should occur.
    /// </summary>
    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0)
            return;

        var name = _tile.Structure.Name;
        var type = _tile.Undeploy(_destroy);

        if (MapManager.Instance.DeploySpawnTable.ContainsKey(type))
        {
            var squad = GameManager.Instance.Players[_tile.Team].CreateNewSquad(_tile.transform.position, _tile.Squad.Sector);
            foreach (var ship in _tile.Squad.Ships)
                squad.Ships.Add(ship);
            if (HumanPlayer.Instance.Squad == _tile.Squad)
                HumanPlayer.Instance.Control(squad.gameObject);
            _player.Squads.Remove(_tile.Squad);
            _player.DeleteSquad(_tile.Squad);
            _player.Tiles.Remove(_tile);
        }

        if (_player == HumanPlayer.Instance)
        {
            GUIManager.Instance.PlaySound("Undeploy");
            if (_tile != null && _tile.Type != name)
                GUIManager.Instance.AddEvent(name + " undeployed at " + _tile.Name + ".");
            else
                GUIManager.Instance.AddEvent(name + " undeployed.");
        }

        HumanPlayer.Instance.ReloadGameplayUI();
    }

    /// <summary>
    /// Ensures the undeploy request is still valid.
    /// </summary>
    /// <returns></returns>
    public override bool AssertValid()
    {
        if (_destroy && _tile != null && _tile.Structure != null)
            return true;
        else if (_tile != null && _tile.Team == _player.Team && _tile.Structure != null)
            return true;
        return false;
    }
}
