using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public Bounds Bounds { get; set; }
    public static System.Random Generator = new System.Random();

	// Use this for initialization
	void Start () 
    {
        Bounds = new UnityEngine.Bounds(this.transform.position, new Vector3(10, 10, 10));

        var positive = Generator.Next(0, 2) - 1;
        var height = positive * (Generator.Next() % 50);

        foreach(Transform child in this.gameObject.transform)
        {
            child.transform.position = new Vector3(child.transform.position.x, height, child.transform.position.z);
        }
	}

	// Update is called once per frame

	void Update () 
    {
	    
	}
}
