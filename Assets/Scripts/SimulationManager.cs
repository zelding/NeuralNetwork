using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public Entity SelectedEntity;

    public int startingFishes = 30;
    public GameObject fishBody;

    private bool isRunning = false;
    private int cycle      = 0;

    protected List<Entity> Entities;

	// Use this for initialization
	void Start ()
    {
        Entities = new List<Entity>();

		for(int i = 0; i < startingFishes; i++)
        {
            Entities.Add( new Entity() );
        }
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
