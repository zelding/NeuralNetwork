using System.Collections.Generic;
using UnityEngine;

public class Genes
{
    public class Movement
    {
        internal float speed;
        internal float turnSpeed;
        internal float smoothSpeed;

        internal Movement()
        {
            speed       = Random.Range(5f, 15f);
            turnSpeed   = Random.Range(5f, 20f);
            smoothSpeed = Random.Range(0.5f, 2f);
        }

        internal Movement(Movement movement)
        {
            speed       = movement.speed;
            turnSpeed   = movement.turnSpeed;
            smoothSpeed = movement.smoothSpeed;
        }
    }

    public Movement Legs { get; internal set; }

    public Genes()
    {
        Legs = new Movement();
    }

    public Genes(Genes genes)
    {
        Legs = new Movement(genes.Legs);
    }
}

