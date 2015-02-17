using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    private static Vector3 TOP_RIGHT_OFFSET = new Vector3(140.0f, 0, -92.5f);
    private static Vector3 RIGHT_OFFSET = new Vector3(0.0f, 0, -185.0f);
    private static Vector3 BOTTOM_RIGHT_OFFSET = new Vector3(-140.0f, 0, -92.5f);
    private static Vector3 BOTTOM_LEFT_OFFSET = new Vector3(-140.0f, 0, 92.5f);
    private static Vector3 LEFT_OFFSET = new Vector3(0.0f, 0, 185.0f);
    private static Vector3 TOP_LEFT_OFFSET = new Vector3(140.0f, 0, 92.5f);

    private static MapManager instance;
    private List<GameObject> Sectors;

    public static MapManager Instance
    {
        get 
        {
            if (instance == null)
            {
                var obj = Instantiate(Resources.Load<GameObject>("MapManager"), Vector3.zero, Quaternion.identity) as GameObject;
                instance = obj.GetComponent<MapManager>();
            }

            return instance;
        }
    }

	// Use this for initialization
	public void Start()
    {
        Sectors = new List<GameObject>();
        var baseSector = Resources.Load<GameObject>("Sector");
        instance = this;

        Sectors.Add(Instantiate(baseSector, Vector3.zero, Quaternion.identity) as GameObject);
	}

    public void GenerateNewSectors(Sector origin)
    {
        var baseSector = Resources.Load<GameObject>("Sector");
        var position = Vector3.zero;
        var newSectors = new List<GameObject>();

        // Generate any needed immediate neighbors and link them
        if (origin.TopRight == null)
        {
            position = origin.transform.position + TOP_RIGHT_OFFSET;

            origin.TopRight = Instantiate(baseSector, position, Quaternion.identity) as GameObject;
            origin.TopRight.GetComponent<Sector>().BottomLeft = origin.gameObject;

            newSectors.Add(origin.TopRight);
            Sectors.Add(origin.TopRight);
        }

        if (origin.Right == null)
        {
            position = origin.transform.position + RIGHT_OFFSET;

            origin.Right = Instantiate(baseSector, position, Quaternion.identity) as GameObject;
            origin.Right.GetComponent<Sector>().Left = origin.gameObject;

            newSectors.Add(origin.Right);
            Sectors.Add(origin.Right);
        }

        if (origin.BottomRight == null)
        {
            position = origin.transform.position + BOTTOM_RIGHT_OFFSET;

            origin.BottomRight = Instantiate(baseSector, position, Quaternion.identity) as GameObject;
            origin.BottomRight.GetComponent<Sector>().TopLeft = origin.gameObject;

            newSectors.Add(origin.BottomRight);
            Sectors.Add(origin.BottomRight);
        }

        if (origin.BottomLeft == null)
        {
            position = origin.transform.position + BOTTOM_LEFT_OFFSET;

            origin.BottomLeft = Instantiate(baseSector, position, Quaternion.identity) as GameObject;
            origin.BottomLeft.GetComponent<Sector>().TopRight = origin.gameObject;

            newSectors.Add(origin.BottomLeft);
            Sectors.Add(origin.BottomLeft);
        }

        if (origin.Left == null)
        {
            position = origin.transform.position + LEFT_OFFSET;

            origin.Left = Instantiate(baseSector, position, Quaternion.identity) as GameObject;
            origin.Left.GetComponent<Sector>().Right = origin.gameObject;

            newSectors.Add(origin.Left);
            Sectors.Add(origin.Left);
        }

        if (origin.TopLeft == null)
        {
            position = origin.transform.position + TOP_LEFT_OFFSET;

            origin.TopLeft = Instantiate(baseSector, position, Quaternion.identity) as GameObject;
            origin.TopLeft.GetComponent<Sector>().BottomRight = origin.gameObject;

            newSectors.Add(origin.TopLeft);
            Sectors.Add(origin.TopLeft);
        }

        // resolve broken links
        foreach(var sector in newSectors)
        {
            var component = sector.GetComponent<Sector>();

            if (component.TopRight == null)
                component.TopRight = FindSectorAtPosition(sector.transform.position + TOP_RIGHT_OFFSET);
            if (component.Right == null)
                component.Right = FindSectorAtPosition(sector.transform.position + RIGHT_OFFSET);
            if (component.BottomRight == null)
                component.BottomRight = FindSectorAtPosition(sector.transform.position + BOTTOM_RIGHT_OFFSET);
            if (component.BottomLeft == null)
                component.BottomLeft = FindSectorAtPosition(sector.transform.position + BOTTOM_LEFT_OFFSET);
            if (component.Left == null)
                component.Left = FindSectorAtPosition(sector.transform.position + LEFT_OFFSET);
            if (component.TopLeft == null)
                component.TopLeft = FindSectorAtPosition(sector.transform.position + TOP_LEFT_OFFSET);
        }
    }
	
    private GameObject FindSectorAtPosition(Vector3 position)
    {
        foreach(var sector in Sectors)
        {
            if (sector.transform.position == position)
                return sector;
        }

        return null;
    }
}
