using System;
using UnityEngine;
using UnityEngine.UI;

public class EntityInfoRenderer : MonoBehaviour {

    public EntityInfo SelectedEntity;

    public Text txt_generation;
    public Text txt_entity_name;
    public Text txt_entity_energy;
    public Text txt_entity_age;
    public Text txt_entity_distance;
    public Text txt_entity_food;
    public Text txt_fittness;
    public Text txt_topSpeed;
    public Text txt_speed;
   
    public Text txt_input;
    public Text txt_output;
	
	// Update is called once per frame
	private void OnGUI () {

        if( SelectedEntity != null )
        {
            if( SelectedEntity is EntityController  ) {
                EntityController tmp = SelectedEntity as EntityController;

                if( tmp != null && tmp.enabled ) {
                    txt_input.text = "Input: " + JoinArray(Array.ConvertAll(tmp.Input, new Converter<float, string>(FloatFToString)));
                    txt_output.text = "Output: " + JoinArray(Array.ConvertAll(tmp.Output, new Converter<float, string>(FloatFToString)));
                    txt_speed.text = "Speed: " + tmp.GetSpeed().ToString("0.00");
                }
            }

            txt_entity_name.text     = "Entity name: " + SelectedEntity.GetName();
            txt_entity_energy.text   = "Energy: "      + SelectedEntity.GetEnergy().ToString("0.00");
            txt_entity_age.text      = "Age: "         + SelectedEntity.GetAge().ToString("0.00");
            txt_generation.text      = "Generation: "  + SelectedEntity.GetBrain().gen + " => " + SelectedEntity.GetBrain().lineage;

            txt_entity_distance.text = "Distance: "    + SelectedEntity.GetDistance().ToString("0.00");
            txt_entity_food.text     = "Eaten: "       + SelectedEntity.GetConsumption().ToString("0.00");
            txt_fittness.text        = "Fittness: "    + SelectedEntity.GetFittness().ToString("0.00");
            txt_topSpeed.text        = "Top Speed: "   + SelectedEntity.GetTopSpeed().ToString("0.00");
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
