using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    private static System.Random Generator = new System.Random();

    private Bounds _bounds;

    public Bounds Bounds { get { return _bounds; } }

	// Use this for initialization
	void Start () 
    {
        _bounds = new UnityEngine.Bounds(this.transform.position, new Vector3(10, 10, 10));
        var height = Generator.Next() % 50 - 25;
        this.GetComponent<ParticleSystem>().transform.position = new Vector3(this.transform.position.x, height, this.transform.position.z);

        
	}

	// Update is called once per frame
	void Update () 
    {
	}
}
