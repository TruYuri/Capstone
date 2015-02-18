using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public Bounds Bounds { get; set; }

	// Use this for initialization
	void Start () 
    {
        Bounds = new UnityEngine.Bounds(this.transform.position, new Vector3(10, 10, 10));
	}

	// Update is called once per frame

	void Update () 
    {
	
	}
}
