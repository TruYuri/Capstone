using UnityEngine;
using System.Collections;

public class UndeployEvent : GameEvent
{
    private Tile _tile;
    private bool _destroy;
    private Player _player;

    public UndeployEvent(int turns, Player player, Tile tile, bool destroy)
        : base(turns)
    {
        _tile = tile;
        _destroy = destroy;
        _player = player;
        if (_player == HumanPlayer.Instance)
            GUIManager.Instance.AddEvent("Undeploying " + _tile.Structure.Name + " at " + _tile.Name);
    }

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

        if (_player == HumanPlayer.Instance && _tile != null)
            GUIManager.Instance.AddEvent(name + " undeployed at " + _tile.Name + ".");
        else if (_player == HumanPlayer.Instance && _tile == null)
            GUIManager.Instance.AddEvent(name + " undeployed.");

        if (_player == HumanPlayer.Instance)
            HumanPlayer.Instance.ReloadGameplayUI();
    }

    public override bool AssertValid()
    {
        if (_destroy && _tile != null && _tile.Structure != null)
            return true;
        else if (_tile != null && _tile.Team == _player.Team && _tile.Structure != null)
            return true;
        return false;
    }
}
