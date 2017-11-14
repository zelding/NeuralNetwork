using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCameraStuff : MonoBehaviour {

    public Text txt_pop_size;
    public Text txt_generation;

    private SimulationManager parent;

    // Use this for initialization
    void Start () {
        parent = FindObjectOfType<SimulationManager>();
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    private void OnGUI()
    {
        if( parent != null )
        {
            txt_pop_size.text = "Population: " + parent.WorstEntities.Count + " / " + parent.AliveEntities.Count;
            txt_generation.text = "Generation: " + parent.Cycle;
        }
    }
}
