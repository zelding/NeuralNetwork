using System.Collections.Generic;
using UnityEngine;

public class Genes
{
    public interface IMutatable
    {
        void Mutate();
    }

    public abstract class AMutateable 
    {
        public bool isMutated = false;

        abstract internal void Mutate();

        protected float MutateValue(float value, float min = -0.5f, float max = 0.5f)
        {
            float randomNumber = Random.Range(0f, 1000f);

            if (randomNumber <= 2f)
            {
                value *= -1f;

                isMutated = true;
            }
            else if (randomNumber <= 4f)
            {
                value = Random.Range(min, max);

                isMutated = true;
            }
            else if (randomNumber <= 6f)
            {
                float factor = Random.Range(0f, 1f) + 1f;

                value *= factor;

                isMutated = true;
            }
            else if (randomNumber <= 8f)
            {
                float factor = Random.Range(0f, 1f);

                value *= factor;

                isMutated = true;
            }

            return value;
        }
    }

    public class Movement : AMutateable
    {
        public const float minSpeed = 5f;
        public const float maxSpeed = 15f;

        public const float minTurnSpeed = 5f;
        public const float maxTurnSpeed = 20f;

        public const float minSmoothSpeed = 0.5f;
        public const float maxSmoothSpeed = 2f;

        internal float speed;
        internal float turnSpeed;
        internal float smoothSpeed;

        internal Movement()
        {
            speed       = Random.Range(minSpeed, maxSpeed);
            turnSpeed   = Random.Range(minTurnSpeed, maxTurnSpeed);
            smoothSpeed = Random.Range(minSmoothSpeed, maxSmoothSpeed);
        }

        internal Movement(Movement movement)
        {
            speed       = movement.speed;
            turnSpeed   = movement.turnSpeed;
            smoothSpeed = movement.smoothSpeed;
        }

        internal override void Mutate()
        {
            speed = MutateValue(speed, minSpeed, maxSpeed);
            turnSpeed = MutateValue(turnSpeed, minTurnSpeed, maxTurnSpeed);
            smoothSpeed = MutateValue(smoothSpeed, minSmoothSpeed, maxSmoothSpeed);
        }
    }

    public class Sight : AMutateable
    {
        public const float minRange = 30f;
        public const float maxRange = 80f;

        public const float minAngle = 30f;
        public const float maxAngle = 120f;

        public const float minResolution = 1f;
        public const float maxResolution = 5f;

        internal float range;
        internal float angle;
        internal float resolution;

        internal Sight()
        {
            range = Random.Range(minRange, maxRange);
            angle = Random.Range(minAngle, maxAngle);
            resolution = Random.Range(minResolution, maxResolution);
        }

        internal Sight(Sight sight)
        {
            range = sight.range;
            angle = sight.angle;
            resolution = sight.resolution;
        }

        internal override void Mutate()
        {
            range = MutateValue(range, minRange, maxRange);
            angle = MutateValue(angle, minAngle, maxAngle);
            resolution = MutateValue(resolution, minResolution, maxResolution);
        }
    }

    public Movement Legs { get; internal set; }
    public Sight Eyes { get; internal set; }

    public List<AMutateable> Chromosomes;

    public bool isMutated = false;

    public Genes()
    {
        Legs = new Movement();
        Eyes = new Sight();

        Chromosomes = new List<AMutateable> {
            new Movement(),
            new Sight()
        };
    }

    public Genes(Genes genes)
    {
        Legs = new Movement(genes.Legs);
        Eyes = new Sight(genes.Eyes);

        /*Chromosomes = new List<AMutateable> {
            new Movement(),
            new Sight()
        }.ForEach(IMutatable c => c.Mutate());*/

        Mutate();
    }

    protected void Mutate()
    {
        Legs.Mutate();
        Eyes.Mutate();

        isMutated = Legs.isMutated || Eyes.isMutated;
    }
}

