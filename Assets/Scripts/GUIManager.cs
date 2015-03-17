using UnityEngine;
using System.Collections.Generic;

public class GUIManager : MonoBehaviour 
{
    private static GUIManager _instance;
    private Dictionary<string, CustomButton> _interface;

    public static GUIManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<GUIManager>();
            return _instance;
        }
    }

    void Start()
    {
        _instance = this;

        if(_interface == null)
            _interface = new Dictionary<string, CustomButton>();
    }

    void Update()
    {

    }

    public void Register(string name, CustomButton btn)
    {
        if (_interface == null)
            _interface = new Dictionary<string, CustomButton>();

        if(!_interface.ContainsKey(name))
            _interface.Add(name, btn);
    }

    //
    // These functions update UI when the object is selected
    //
    public void SquadSelected(Squad squad)
    {
    }

    public void TileSelected(Tile tile)
    {

    }

    //
    // These functions update and display the menu when a command is given
    //

    // Notes: Tiles have squads, but they may be empty.
    // Tiles and squads should check for enemy/friendly before choosing how to make the new menu
    public void SquadToSpace(Squad squad, Vector3 coords)
    {

    }

    public void SquadToTile(Squad squad, Tile tile)
    {
    }

    // note: "squad2" isn't necessarily hostile. Handle that stuff here.
    public void SquadToSquad(Squad playerSquad, Squad squad2)
    {

    }
}
