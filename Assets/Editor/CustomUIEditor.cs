using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Unity Inspector override for the CustomUI class.
/// </summary>
[CustomEditor(typeof(CustomUI), true)]
public class UIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
