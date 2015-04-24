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
        _squad.Mission = this;

        if (player == HumanPlayer.Instance)
        {
            if(tile != null)
                GUIManager.Instance.AddEvent("Transmitting command to deploy " + ship.Name + " at " + tile.Name + ".");
            else
                GUIManager.Instance.AddEvent("Transmitting command to deploy " + ship.Name + ".");
        }
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0)
            return;

        var orig = _tile;
        _tile = _squad.Deploy(_structure, _tile);
        _squad.Mission = null;
        if(_squad.Ships.Count == 0 && _tile.Squad != _squad)
        {
            if (HumanPlayer.Instance == _player && HumanPlayer.Instance.Squad == _squad)
                _player.Control(_tile.gameObject);
            _player.CleanSquad(_squad);
            _player.DeleteSquad(_squad);
        }
        
        if(HumanPlayer.Instance == _player)
        {
            if (orig != null)
                GUIManager.Instance.AddEvent(_structure.Name + " deployed at " + _tile.Name + ".");
            else
                GUIManager.Instance.AddEvent(_structure.Name + " deployed.");
       
            GUIManager.Instance.PlaySound("Deploy");
        }

        HumanPlayer.Instance.ReloadGameplayUI(); 
    }

    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null && _squad.Mission == this)
        {
            if (_tile != null && (_tile.Team == _squad.Team || _tile.Team == Team.Uninhabited) && _squad.Ships.Contains(_structure))
                return true;
            else if (_tile == null && _squad.Ships.Contains(_structure))
                return true;
        }
        return false;
    }
}
