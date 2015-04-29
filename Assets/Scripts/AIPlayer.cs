using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

class AIPlayer : Player
{
    public override void Init(Team team)
    {
        base.Init(team);
    }

    void Update()
    {
        if (GameManager.Instance.Paused || _turnEnded)
            return;
    }
}
