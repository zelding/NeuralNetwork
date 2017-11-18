using System;
using System.Collections.Generic;
using UnityEngine;

class FereCamera : MonoBehaviour
{
    public EightDirController Legs;

    private void Start()
    {
        Legs = new EightDirController( GetComponent<Rigidbody>() );
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }
}

