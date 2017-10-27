using System;
using UnityEngine;
using UnityEngine.UI;

public class EntityInfoRenderer : MonoBehaviour {

    public EntityController SelectedEntity;

    public Text txt_generation;
    public Text txt_entity_name;
    public Text txt_entity_energy;
    public Text txt_entity_age;
    public Text txt_entity_distance;
    public Text txt_entity_food;

    public Text txt_input;
    public Text txt_output;
	
	// Update is called once per frame
	private void OnGUI () {

        if( SelectedEntity != null )
        {
            txt_entity_name.text = "Entity name: " + SelectedEntity.Name.ToString().ToString();
            txt_entity_energy.text = "Energy: " + SelectedEntity.Energy.ToString("0.00");
            txt_entity_age.text = "Age: " + SelectedEntity.Age.ToString("0.00");
            txt_generation.text = "Generation: " + SelectedEntity.Brain.gen + " => " + SelectedEntity.Brain.lineage;
            txt_entity_distance.text = "Distance: " + SelectedEntity.Distance.ToString("0.00");
            txt_entity_food.text = "Eaten: " + SelectedEntity.Consumption.ToString("0.00");

            txt_input.text = "Input: " + JoinArray(Array.ConvertAll(SelectedEntity.Input, new Converter<float, string>(FloatFToString)));
            txt_output.text = "Output: " + JoinArray(Array.ConvertAll(SelectedEntity.Output, new Converter<float, string>(FloatFToString)));
        }
    }

    public static string FloatFToString( float f )
    {
        return f.ToString("0.00");
    }

    public static string JoinArray( string[] data )
    {
        return string.Join(", ", data);
    }
}
