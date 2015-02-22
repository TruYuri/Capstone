using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public Bounds Bounds { get; set; }
    public static System.Random Generator = new System.Random();
    private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () 
    {
        Bounds = new UnityEngine.Bounds(this.transform.position, new Vector3(10, 10, 10));

        var height = Generator.Next() % 50 - 25;

        this.GetComponent<SpriteRenderer>().transform.position = new Vector3(this.transform.position.x, height, this.transform.position.z);

        spriteRenderer = this.GetComponent<SpriteRenderer>();

        // generate with probabilities
        // if(need something generated)
	}

	// Update is called once per frame
	void Update () 
    {
        if (this.spriteRenderer.isVisible)
        {
            //renderer.enabled = true;
            transform.rotation = Camera.main.transform.rotation;
        }
        //else
            //renderer.enabled = false;
	}
}
