using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FoodController))]
public class FoodHelper : Editor
{

    void OnSceneGUI()
    {
        FoodController food = (FoodController)target;

        if( food != null )
        {
            Handles.DrawWireArc(food.transform.position, Vector3.up, Vector3.forward, 360, food.MaxRadius);
            Handles.DrawWireArc(food.transform.position, Vector3.left, Vector3.forward, 360, food.MaxRadius);
            Handles.DrawWireArc(food.transform.position, Vector3.forward, Vector3.left, 360, food.MaxRadius);
        }
    }

}