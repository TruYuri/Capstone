using UnityEngine;
using System.Collections;

/// <summary>
/// Handles a warp request through two Warp Portals.
/// </summary>
public class WarpEvent : GameEvent
{
    private Tile _exitPortal;
    private Squad _squad;
    private Player _player;

    /// <summary>
    /// Constructor for the WarpEvent
    /// </summary>
    /// <param name="turns"></param>
    /// <param name="player"></param>
    /// <param name="squad"></param>
    /// <param name="exit"></param>
    public WarpEvent(int turns, Player player, Squad squad, Tile exit) 
        : base(turns)
    {
        _exitPortal = exit;
        _squad = squad;
        _squad.Mission = this;
        _player = player;
        if (_player == HumanPlayer.Instance)
            GUIManager.Instance.AddEvent("Transmitting command to warp " + _squad.name + ".");
    }

    /// <summary>
    /// Progresses until the Warp should actually occur.
    /// </summary>
    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0)
            return;

        _squad.Mission = null;
        var r = _exitPortal.Radius;
        var val = GameManager.Generator.Next(2);
        var offset = val == 0 ? new Vector3(r, 0, 0) : new Vector3(0, 0, r);
        _squad.transform.position = _exitPortal.transform.position + offset;

        if (_player == HumanPlayer.Instance)
        {
            GUIManager.Instance.AddEvent(_squad.name + " warped.");
            GUIManager.Instance.PlaySound("Warp");
        }
    }

    /// <summary>
    /// Ensures the warping is still valid.
    /// </summary>
    /// <returns></returns>
    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null && _exitPortal != null && _exitPortal.gameObject != null && _squad.Mission == this)
        {
            return true;
        }

        return false;
    }
}
