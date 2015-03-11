using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CustomButton : Button
{
    public string data;

    public void ClickUpgrade()
    {
        var split = data.Split('|');
        Player.Instance.UpgradeResearch(split[0], split[1], split[2]);
    }
}
