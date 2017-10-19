using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{

    public int startingFishes = 30;

    private bool isRunning = false;
    private int cycle      = 0;

    protected Entity[] entities;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		if ( isRunning )
        {

        }
	}

    private void OnGUI()
    {
        
    }
}
