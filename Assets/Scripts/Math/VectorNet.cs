using System;
using System.Collections.Generic;
using UnityEngine;

public class VectorNet : NeuralNetwork
{
	new public Vector3[][] neurons;
	new public Vector3[][][] weights;
	new public Vector3[][][] biases;

	public VectorNet (int[] layers)
	{
		gen = 1;
		mutationsCount = 0;
		StructureId = "|" + string.Join ("-", System.Array.ConvertAll (layers, new System.Converter<int, string> (IntToString))) + "|";

		this.layers = new int[ layers.Length ];

		for (int i = 0; i < layers.Length; i++) {
			this.layers [i] = layers [i];
		}

		this.InitNeurons ();
		weights = this.InitMatrix ();
		biases = this.InitMatrix (-0.125f, 0.125f);
	}

	public VectorNet (VectorNet copyME, bool justCopy = false)
	{
		layers = new int[ copyME.layers.Length ];

		for( int i = 0; i < copyME.layers.Length; i++ ) {
			layers[ i ] = copyME.layers[ i ];
		}

		InitNeurons();
		weights = CopyMatrix(copyME.weights);
		biases = CopyMatrix(copyME.biases);

		isMutated = false;

		if( !justCopy ) {
			gen = copyME.gen + 1;
			mutationsCount = 0;
			Mutate();
			StructureId = "|" + string.Join("-", System.Array.ConvertAll(layers, new System.Converter<int, string>(IntToString))) + "|";
		}
		else {
			gen = copyME.gen;
			mutationsCount = copyME.mutationsCount;
			StructureId = copyME.StructureId;
		}
	}

	protected override void InitNeurons ()
	{
		List<Vector3[]> neuronList = new List<Vector3[]> ();

		//go through all the layers
		for (int i = 0; i < layers.Length; i++) {
			neuronList.Add (new Vector3[ layers [i] ]);
		}

		//convert it back to simple array
		neurons = neuronList.ToArray ();
	}

	new protected Vector3[][][] InitMatrix (float min = -0.5f, float max = 0.5f)
	{
		List<Vector3[][]> list = new List<Vector3[][]> ();

		//we start at 1, not 0 because the first layer is skipped
		for (int i = 1; i < layers.Length; i++) {
			List<Vector3[]> layerValueList = new List<Vector3[]> ();

			//this tells how many connections we are going to need to generate
			int neuronsInPrevLayer = layers [i - 1];

			for (int j = 0; j < neurons [i].Length; j++) {
				Vector3[] neuronValues = new Vector3[neuronsInPrevLayer];

				//set weights/biases to random
				for (int k = 0; k < neuronsInPrevLayer; k++) {
					neuronValues [k] = UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range (min, max);
				}

				layerValueList.Add (neuronValues);
			}

			list.Add (layerValueList.ToArray ());
		}

		return list.ToArray ();
	}

	protected Vector3[][][] CopyMatrix (Vector3[][][] copyData)
	{
		Vector3[][][] matrix = new Vector3[copyData.Length][][];

		for (int i = 0; i < copyData.Length; i++) {
			Vector3[][] subMatrix = new Vector3[ copyData [i].Length ][];

			for (int j = 0; j < copyData [i].Length; j++) {
				Vector3[] subSubMatrix = new Vector3[ copyData [i] [j].Length ];

				for (int k = 0; k < copyData [i] [j].Length; k++) {
					subSubMatrix [k] = copyData [i] [j] [k];
				}

				subMatrix [j] = subSubMatrix;
			}

			matrix [i] = subMatrix;
		}

		return matrix;
	}

	new public Vector3[] FeedForward (Vector3[] inputs)
	{
		if (inputs.Length != neurons [0].Length) {
			throw new System.ArgumentException ("Lengths mismatch");
		}

		for (int i = 0; i < inputs.Length; i++) {
			neurons [0] [i] = inputs [i];
		}

		for (int i = 1; i < layers.Length; i++) {
			for (int j = 0; j < neurons [i].Length; j++) {
				Vector3 value = Vector3.zero;
				//previous layer neurons
				for (int k = 0; k < neurons [i - 1].Length; k++) {
					value += Vector3.Cross (neurons [i - 1] [k], weights [i - 1] [j] [k]) + biases [i - 1] [j] [k];
				}

				neurons [i] [j] = value.normalized;
			}
		}

		return neurons [neurons.Length - 1];
	}

	public override void Mutate ()
	{
		for (int i = 0; i < weights.Length; i++) {
			for (int j = 0; j < weights [i].Length; j++) {
				for (int k = 0; k < weights [i] [j].Length; k++) {
					Vector3 weight = weights [i] [j] [k];

					float randomNumber = UnityEngine.Random.Range (0f, 1000f);

					if (randomNumber <= 2f) {
						weight *= -1f;

						isMutated = true;
						mutationsCount++;
					} else if (randomNumber <= 4f) {
						weight += (UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range (-0.5f, 0.5f));

						isMutated = true;
						mutationsCount++;
					} else if (randomNumber <= 6f) {
						float factor = UnityEngine.Random.Range (0f, 1f) + 1f;

						weight *= factor;

						isMutated = true;
						mutationsCount++;
					} else if (randomNumber <= 8f) {
						float factor = UnityEngine.Random.Range (0f, 1f);

						weight *= factor;

						isMutated = true;
						mutationsCount++;
					}

					weights [i] [j] [k] = weight;
				}
			}
		}
	}
}
