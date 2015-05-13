using UnityEngine;
using System.Collections;

/// <summary>
/// Interface to define how common UI objects interface via code.
/// </summary>
public interface ListableObject
{
    /// <summary>
    /// Generates a panel nested in a UI list.
    /// </summary>
    /// <param name="listName">The internal list name.</param>
    /// <param name="index">The list index.</param>
    /// <param name="data">Optional data.</param>
    /// <returns>A new list panel.</returns>
    GameObject CreateListEntry(string listName, int index, System.Object data);

    /// <summary>
    /// Generates a panel for the object in a construction list.
    /// </summary>
    /// <param name="listName">The internal list name.</param>
    /// <param name="index">The list index.</param>
    /// <param name="data">Optional data.</param>
    /// <returns>A new build list panel.</returns>
    GameObject CreateBuildListEntry(string listName, int index, System.Object data);

    /// <summary>
    /// Populates a popup window detailing this object's info to consider for construction.
    /// </summary>
    /// <param name="popUp">The popup panel.</param>
    /// <param name="data">Optional data.</param>
    void PopulateBuildInfo(GameObject popUp, System.Object data);

    /// <summary>
    /// Populates a popop window detailing this existing object.
    /// </summary>
    /// <param name="popUp">The popup panel.</param>
    /// <param name="data">Optional data.</param>
    void PopulateGeneralInfo(GameObject popUp, System.Object data);
}
