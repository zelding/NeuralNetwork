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
        public const float minDominance = 0.1f;
        public const float maxDominance = 0.65f;

        public bool isMutated = false;
        public float dominance;

        virtual internal void Mutate() {
            dominance = Random.Range(minDominance, maxDominance);
        }

        protected float MutateValue(float value, float min = -0.5f, float max = 0.5f)
        {
            float randomNumber = Random.Range(0f, 100f);

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

        override internal void Mutate()
        {
            base.Mutate();
            speed = MutateValue(speed, minSpeed, maxSpeed);
            turnSpeed = MutateValue(turnSpeed, minTurnSpeed, maxTurnSpeed);
            smoothSpeed = MutateValue(smoothSpeed, minSmoothSpeed, maxSmoothSpeed);
        }
    }

    public class Smell : AMutateable
    {
        public const float minRange = 30f;
        public const float maxRange = 180f;

        public const float minResolution = 1f;
        public const float maxResolution = 5f;

        internal float range;
        internal float resolution;

        internal Smell()
        {
            range = Random.Range(minRange, maxRange);
            resolution = Random.Range(minResolution, maxResolution);
        }

        internal Smell( Smell smell )
        {
            range = smell.range;
            resolution = smell.resolution;
        }

        override internal void Mutate()
        {
            base.Mutate();
            range = MutateValue(range, minRange, maxRange);
            resolution = MutateValue(resolution, minResolution, maxResolution);
        }
    }

    public class Sight : Smell
    {
        public const float minAngle = 30f;
        public const float maxAngle = 160f;

        internal float angle;

        internal Sight() : base()
        {
            angle = Random.Range(minAngle, maxAngle);
        }

        internal Sight( Sight sight ) : base(sight as Smell)
        {
            angle = sight.angle;
        }

        internal override void Mutate()
        {
            base.Mutate();
            range = MutateValue(range, minRange, maxRange);
            angle = MutateValue(angle, minAngle, maxAngle);
            resolution = MutateValue(resolution, minResolution, maxResolution);
        }
    }

    public Movement Legs { get; internal set; }
    public Sight Eyes { get; internal set; }
    public Smell Noze { get; private set; }

    public List<AMutateable> Chromosomes;

    public bool isMutated = false;

    public Genes()
    {
        Legs = new Movement();
        Eyes = new Sight();
        Noze = new Smell();

        Chromosomes = new List<AMutateable> {
            Legs,
            Eyes,
            Noze
        };
    }

    public Genes(Genes genes, bool copyOnly = false)
    {
        Legs = new Movement(genes.Legs);
        Eyes = new Sight(genes.Eyes);
        Noze = new Smell(genes.Noze);

        Chromosomes = new List<AMutateable> {
            Legs,
            Eyes,
            Noze
        };

        if( !copyOnly ) {
            Mutate();
        }
    }

    protected void Mutate()
    {
        foreach(var c in Chromosomes) {
            c.Mutate();
        }

        isMutated = Legs.isMutated || Eyes.isMutated || Noze.isMutated;
    }
}

