using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SpaceGame
{
    public class Sector : MonoBehaviour {

        private List<Tile> Tiles;

	    // Use this for initialization
	    void Start () {
            Tiles = new List<Tile>();
	    }

        public void Generate(System.Random seed)
        {

        }
	
	    // Update is called once per frame
	    void Update () {
	
	    }
    }
}
