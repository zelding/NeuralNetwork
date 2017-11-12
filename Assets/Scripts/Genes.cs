﻿using System.Collections.Generic;
using UnityEngine;

public class Genes
{
    public interface IMutatable //why is it public?
    {
        void Mutate();
    }

    #region TYPES

    public abstract class Chromosome
    {
        public const float minDominance = 0.1f;
        public const float maxDominance = 0.65f;

        public bool isMutated = false;
        public float dominance;

        virtual internal void Mutate()
        {
            dominance = Random.Range(minDominance, maxDominance);
        }

        //struct for data representation ?

        protected float MutateValue( float value, float min = -0.5f, float max = 0.5f )
        {
            float randomNumber = Random.Range(0f, 1000f);

            if( randomNumber <= 2f ) {
                value *= -1f;

                isMutated = true;
            }
            else if( randomNumber <= 4f ) {
                value = Random.Range(min, max);

                isMutated = true;
            }
            else if( randomNumber <= 6f ) {
                float factor = Random.Range(0f, 1f) + 1f;

                value *= factor;

                isMutated = true;
            }
            else if( randomNumber <= 8f ) {
                float factor = Random.Range(0f, 1f);

                value *= factor;

                isMutated = true;
            }

            return Mathf.Clamp(value, min, max);
        }

        protected int MutateValue( int value, int minvalue, int maxValue )
        {
            return 0;
        }
    }

    public class Skin : Chromosome
    {
        public const float minValue = 0.0f;
        public const float maxValue = 1.0f;

        internal float r;
        internal float g;
        internal float b;

        public Skin() : base()
        {
            r = Random.Range(minValue, maxValue);
            g = Random.Range(minValue, maxValue);
            b = Random.Range(minValue, maxValue);
        }

        public Skin (Skin skin)
        {
            r = skin.r;
            g = skin.g;
            b = skin.b;
        }

        public Color GetColor()
        {
            return new Color(r, g, b);
        }

        internal override void Mutate()
        {
            base.Mutate();

            r = MutateValue(r, minValue, maxValue);
            g = MutateValue(g, minValue, maxValue);
            b = MutateValue(b, minValue, maxValue);
        }
    }

    public class NeuronStructure : Chromosome
    {
        public const int minHiddenLayers = 1;
        public const int maxHiddenLayers = 3;
        public const int minNeuronsInLayers = 8;
        public const int maxNeuronsInLayers = 32;

        internal int hiddenLayers;
        internal int[] neuronsInHiddenLayers;

        internal NeuronStructure()
        {
            hiddenLayers = Mathf.RoundToInt(Random.Range(minHiddenLayers, maxHiddenLayers));
            neuronsInHiddenLayers = new int[ hiddenLayers ];

            for( int i = 0; i < hiddenLayers; i++ ) {
                neuronsInHiddenLayers[ i ] = Mathf.RoundToInt(Random.Range(minNeuronsInLayers, maxNeuronsInLayers));
            }
        }

        internal NeuronStructure( NeuronStructure ns )
        {
            hiddenLayers = ns.hiddenLayers;
            neuronsInHiddenLayers = new int[ hiddenLayers ];

            for( int i = 0; i < hiddenLayers; i++ ) {
                neuronsInHiddenLayers[ i ] = ns.neuronsInHiddenLayers[ i ];
            }
        }

        override internal void Mutate()
        {
            base.Mutate();

            hiddenLayers = MutateValue(hiddenLayers, minHiddenLayers, maxHiddenLayers);

            int[] _neuronsInHiddenLayers = new int[ hiddenLayers ];

            for( int i = 0; i < hiddenLayers; i++ ) {
                int oldLength = neuronsInHiddenLayers.Length;

                if( i < oldLength ) {
                    _neuronsInHiddenLayers[ i ] = MutateValue(neuronsInHiddenLayers[ i ], minHiddenLayers, maxNeuronsInLayers);
                }
                else {
                    _neuronsInHiddenLayers[ i ] = Mathf.RoundToInt(Random.Range(minNeuronsInLayers, maxNeuronsInLayers));
                }
            }

            neuronsInHiddenLayers = _neuronsInHiddenLayers;
        }
    }

    public class Movement : Chromosome
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
            speed = Random.Range(minSpeed, maxSpeed);
            turnSpeed = Random.Range(minTurnSpeed, maxTurnSpeed);
            smoothSpeed = Random.Range(minSmoothSpeed, maxSmoothSpeed);
        }

        internal Movement( Movement movement )
        {
            speed = movement.speed;
            turnSpeed = movement.turnSpeed;
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

    public class Hearing : Chromosome
    {
        public const float minRange = 30f;
        public const float maxRange = 180f;

        internal float range;

        internal Hearing()
        {
            range = Random.Range(minRange, maxRange);
        }

        internal Hearing( Hearing hearing )
        {
            range = hearing.range;
        }

        override internal void Mutate()
        {
            base.Mutate();
            range = MutateValue(range, minRange, maxRange);
        }
    }

    public class Smell : Hearing
    {
        public const float minResolution = 1f;
        public const float maxResolution = 5f;

        internal float resolution;

        internal Smell() : base()
        {
            resolution = Random.Range(minResolution, maxResolution);
        }

        internal Smell( Smell smell ) : base(smell as Hearing)
        {
            resolution = smell.resolution;
        }

        override internal void Mutate()
        {
            base.Mutate();
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
            angle = MutateValue(angle, minAngle, maxAngle);
        }
    }

    #endregion

    public Movement Legs { get; internal set; }
    public Hearing Ears { get; internal set; }
    public Smell Noze { get; internal set; }
    public Sight Eyes { get; internal set; }
    public Skin Color { get; internal set; }
    public NeuronStructure Brain { get; internal set; }

    public List<Chromosome> Chromosomes;

    public bool isMutated = false;

    public Genes()
    {
        Legs  = new Movement();
        Ears  = new Hearing();
        Noze  = new Smell();
        Eyes  = new Sight();
        Brain = new NeuronStructure();
        Color = new Skin();

        Chromosomes = new List<Chromosome> {
            Legs,
            Eyes,
            Noze,
            Ears,
            Brain,
            Color
        };
    }

    public Genes(Genes genes, bool copyOnly = false)
    {
        Legs = new Movement(genes.Legs);
        Ears = new Hearing(genes.Ears);
        Noze = new Smell(genes.Noze);
        Eyes = new Sight(genes.Eyes);
        Brain = new NeuronStructure(genes.Brain);
        Color = new Skin(genes.Color);

        Chromosomes = new List<Chromosome> {
            Legs,
            Eyes,
            Noze,
            Ears,
            Brain,
            Color
        };

        if( !copyOnly ) {
            Mutate();
        }
    }

    protected void Mutate()
    {
        foreach( Chromosome c in Chromosomes) {
            c.Mutate();
        }

        isMutated = Legs.isMutated || Eyes.isMutated || Noze.isMutated || Ears.isMutated || Brain.isMutated || Color.isMutated;
    }

}

