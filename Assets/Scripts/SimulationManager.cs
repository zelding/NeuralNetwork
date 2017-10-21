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
            Vector3 startPosition = new Vector3(Random.Range(-10f, 10f), 0.85f, Random.Range(-10f, 10f));
            GameObject fish = Instantiate(fishBody, startPosition, Quaternion.identity);
            Entities.Add( fish.GetComponent<Entity>() );
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
