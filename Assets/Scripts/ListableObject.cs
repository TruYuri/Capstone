using UnityEngine;
using System.Collections;

public interface ListableObject
{
    GameObject CreateListEntry(string listName, int index, System.Object data);
    GameObject CreateBuildListEntry(string listName, int index, System.Object data);
    void PopulateBuildInfo(GameObject popUp, System.Object data);
    void PopulateGeneralInfo(GameObject popUp, System.Object data);
}
