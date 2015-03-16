using UnityEngine;
using System.Collections.Generic;

public class GUIManager : MonoBehaviour 
{
    private static GUIManager _instance;
    private Dictionary<string, CustomButton> _interface;

    public static GUIManager Instance { get { return _instance; } }

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
}
