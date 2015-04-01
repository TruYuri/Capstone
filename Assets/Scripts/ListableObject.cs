using UnityEngine;
using System.Collections;

public interface ListableObject
{
    GameObject CreateListEntry(string listName, int index, System.Object data);
    GameObject CreateBuildListEntry(string listName, int index, System.Object data);
    GameObject CreatePopUpInfo(System.Object data);
}
