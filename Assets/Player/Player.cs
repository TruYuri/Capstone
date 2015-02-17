using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

	// Use this for initialization
	void Start () 
    {
	
	}

    void OnTriggerEnter(Collider other)
    {
        var render = other.GetComponent<MeshRenderer>();
        render.enabled = true;
    }

    void OnTriggerExit(Collider other)
    {
        var render = other.GetComponent<MeshRenderer>();
        render.enabled = false;
    }
	
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                float speed = 50.0f;
                transform.position = Vector3.MoveTowards(transform.position, hit.point, Time.deltaTime * speed);
            }
        }
	}
}
