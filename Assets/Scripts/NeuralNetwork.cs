using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork
{

    public List<NeuralNetwork> History { get; protected set; }

    protected NeuralNetwork LastVersion { get; private set; }

    protected readonly bool historyEnabled = false;

    public readonly float PHI = Mathf.Pow(5f,0.5f) * 0.5f + 0.5f;

    public bool isMutated = false;

    public int mutationsCount;

    public int[]       layers;

    public float[][]   neurons;
    public float[][][] weights;
    public float[][][] biases;

    public uint gen       = 0;

    public static float Normalize( float value )
    {
        return (float) System.Math.Tanh(value);
        //return (1.0f / (1.0f + Mathf.Pow(Mathf.Exp(1), -value)) - 0.5f) * 2;
    }

    /// <summary>
    /// Number of neurons in a given layer
    /// </summary>
    /// <param name="layers"></param>
    /// <param name="historyEnabled" type="bool" default="false">Enables the neuralnet data to bi stored in an ordered array</param>
	public NeuralNetwork( int[] layers, bool historyEnabled = false )
    {
        this.historyEnabled =  historyEnabled;
        gen = 1;
        mutationsCount = 0;

        this.layers = new int[ layers.Length ];

        for( int i = 0; i < layers.Length; i++ )
        {
            this.layers[ i ] = layers[ i ];
        }

        if( historyEnabled )
        {
            var LastVersion = new NeuralNetwork(this);
            var History     = new List<NeuralNetwork>();
        }

        InitNeurons();
        weights = InitMatrix();
        biases = InitMatrix(-0.174f, 0.174f);

        if( historyEnabled )
        {
            History.Add(new NeuralNetwork(LastVersion) );
            LastVersion = new NeuralNetwork(this);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="copyME"></param>
    public NeuralNetwork( NeuralNetwork copyME, bool justCopy = false )
    {
        historyEnabled = false;

        layers = new int[ copyME.layers.Length ];

        for( int i = 0; i < copyME.layers.Length; i++ )
        {
            layers[ i ] = copyME.layers[ i ];
        }

        InitNeurons();
        weights = CopyMatrix(copyME.weights);
        biases = CopyMatrix(copyME.biases);

        isMutated = false;

        if( !justCopy )
        {
            gen = copyME.gen + 1;
            mutationsCount = 0;
            Mutate();
        }
        else {
            gen = copyME.gen;
            mutationsCount = copyME.mutationsCount;
        }
    }

    protected void SaveState()
    {
        if( historyEnabled )
        {
            History.Add(new NeuralNetwork(LastVersion));
            LastVersion = new NeuralNetwork(this);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected void InitNeurons()
    {
        List<float[]> neuronList = new List<float[]>();

        //go through all the layers
        for( int i = 0; i < layers.Length; i++ )
        {
            neuronList.Add(new float[ layers[ i ] ]);
        }

        //convert it back to simple array
        neurons = neuronList.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    protected float[][][] InitMatrix( float min = -0.5f, float max = 0.5f )
    {
        List<float[][]> list = new List<float[][]>();

        //we start at 1, not 0 because the first layer is skipped
        for( int i = 1; i < layers.Length; i++ )
        {
            List<float[]> layerValueList = new List<float[]>();

            //this tells how many connections we are going to need to generate
            int neuronsInPrevLayer = layers[i - 1];

            for( int j = 0; j < neurons[ i ].Length; j++ )
            {
                float[] neuronValues = new float[neuronsInPrevLayer];

                //set weights to random
                for( int k = 0; k < neuronsInPrevLayer; k++ )
                {
                    neuronValues[ k ] = Random.Range(min, max);
                }

                layerValueList.Add(neuronValues);
            }

            list.Add(layerValueList.ToArray());
        }

        return list.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="copyData"></param>
    /// <returns></returns>
    protected float[][][] CopyMatrix( float[][][] copyData )
    {
        float[][][] matrix = new float[copyData.Length][][];

        for( int i = 0; i < copyData.Length; i++ )
        {
            float[][] subMatrix = new float[ copyData[i].Length ][];

            for( int j = 0; j < copyData[ i ].Length; j++ )
            {
                float[] subSubMatrix = new float[ copyData[i][j].Length ];

                for( int k = 0; k < copyData[ i ][ j ].Length; k++ )
                {
                    subSubMatrix[ k ] = copyData[ i ][ j ][ k ];
                }

                subMatrix[ j ] = subSubMatrix;
            }

            matrix[ i ] = subMatrix;
        }

        return matrix;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    public float[] FeedForward( float[] inputs )
    {
        if( inputs.Length != neurons[ 0 ].Length )
        {
            throw new System.ArgumentException("Lengths mismatch");
        }

        for( int i = 0; i < inputs.Length; i++ )
        {
            neurons[ 0 ][ i ] = inputs[ i ];
        }

        for( int i = 1; i < layers.Length; i++ )
        {
            for( int j = 0; j < neurons[ i ].Length; j++ )
            {
                float value = 0.0f;
                //previous layer neurons
                for( int k = 0; k < neurons[ i - 1 ].Length; k++ )
                {
                    value += biases[ i - 1 ][ j ][ k ] + weights[ i - 1 ][ j ][ k ] * neurons[ i - 1 ][ k ];
                }

                neurons[ i ][ j ] = Normalize(value);
            }
        }

        return neurons[ neurons.Length - 1 ];
    }

    /// <summary>
    /// 
    /// </summary>
    public void Mutate()
    {
        for( int i = 0; i < weights.Length; i++ )
        {
            for( int j = 0; j < weights[ i ].Length; j++ )
            {
                for( int k = 0; k < weights[ i ][ j ].Length; k++ )
                {
                    float weight = weights[i][j][k];

                    float randomNumber = Random.Range(0f, 1000f);

                    if( randomNumber <= 2f )
                    {
                        weight *= -1f;

                        isMutated = true;
                        mutationsCount++;
                    }
                    else if( randomNumber <= 4f )
                    {
                        weight = Random.Range(-0.5f, 0.5f);

                        isMutated = true;
                        mutationsCount++;
                    }
                    else if( randomNumber <= 6f )
                    {
                        float factor = Random.Range(0f, 1f) + 1f;

                        weight *= factor;

                        isMutated = true;
                        mutationsCount++;
                    }
                    else if( randomNumber <= 8f )
                    {
                        float factor = Random.Range(0f, 1f);

                        weight *= factor;

                        isMutated = true;
                        mutationsCount++;
                    }

                    weights[ i ][ j ][ k ] = weight;
                }
            }
        }
    }
}
