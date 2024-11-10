using System;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkDemo : MonoBehaviour
{
    public int nbHiddenNodes = 2;
    public int nbOutput = 2;
    public int nbInput = 4;
    public int nbTrainIterations = 1000; 
    public float epsilon = 0.1f;

    public GameObject spherePrefab;
    public GameObject cubePrefab;

    private List<Displayer> inputDisplayers = new List<Displayer>();
    private List<Displayer> expectedOutputDisplayers = new List<Displayer>(); // Sorties attendues
    private List<Displayer> calculatedOutputDisplayers = new List<Displayer>(); // Sorties calculées

    private NeuralNetwork neuralNetwork;

    /// <summary>
    /// Initialise le réseau de neurones et les displayers pour les afficher
    /// </summary>
    void Start()
    {
        neuralNetwork = new NeuralNetwork(nbInput, nbHiddenNodes, nbOutput, nbTrainIterations);
        InitDisplayers();
        CalculateAndDisplayResults();
    }

    /// <summary>
    /// Surveille les clics pour détecter si un displayer a été sélectionné, et met à jour les valeurs d'entrée ou de sortie attendues en conséquence
    /// </summary>
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Vérifie si un displayer d'entrée est cliqué
                Displayer clickedInputDisplayer = inputDisplayers.Find(d => d.GameObject == hit.transform.gameObject);
                if (clickedInputDisplayer != null)
                {
                    clickedInputDisplayer.UpdateObjectValue();
                    CalculateAndDisplayRow(clickedInputDisplayer.Row); // Recalcule les résultats avec la nouvelle valeur
                    return; // Sort de la méthode après mise à jour des inputs
                }

                // Vérifie si un displayer de sortie attendue est cliqué
                Displayer clickedExpectedOutputDisplayer = expectedOutputDisplayers.Find(d => d.GameObject == hit.transform.gameObject);
                if (clickedExpectedOutputDisplayer != null)
                {
                    clickedExpectedOutputDisplayer.UpdateObjectValue();
                    CalculateAndDisplayRow(clickedExpectedOutputDisplayer.Row); // Recalcule les résultats avec la nouvelle valeur
                }
            }
        }
    }

    /// <summary>
    /// Initialise les displayers des inputs, des outputs attendus et calculés, et les place dans la scène
    /// </summary>
    void InitDisplayers()
    {
        float[][] inputPatterns = GenerateInputPatterns();
        float[][] outputPatterns = GenerateOutputPatterns();
        
        for (int row = 0; row < inputPatterns.Length; row++)
        {
            float[] inputs = inputPatterns[row];
            float[] outputs = outputPatterns[row];

            // Crée et place les displayers d'entrée
            for (int i = 0; i < inputs.Length; i++)
            {
                Displayer displayer = new Displayer(inputs[i], Type.Input, cubePrefab, row, epsilon);
                inputDisplayers.Add(displayer);
                DisplayDisplayer(displayer, i, row);
            }

            // Crée et place les displayers des sorties attendues et calculées
            for (int i = 0; i < outputs.Length; i++)
            {
                Displayer expectedDisplayer = new Displayer(outputs[i], Type.ExpectedOutput, spherePrefab, row, epsilon);
                expectedOutputDisplayers.Add(expectedDisplayer);
                DisplayDisplayer(expectedDisplayer, inputs.Length + i, row);

                Displayer calculatedDisplayer = new Displayer(0, Type.CalculatedOutput, spherePrefab, row, epsilon);
                calculatedOutputDisplayers.Add(calculatedDisplayer);
                DisplayDisplayer(calculatedDisplayer, inputs.Length + outputs.Length + i, row);
            }
        }
    }

    /// <summary>
    /// Génère des patterns par défaut pour les sorties attendues selon les paramètres de l'application
    /// </summary>
    /// <returns>Tableau des patterns de sortie</returns>
    float[][] GenerateOutputPatterns()
    {
        int defaultNbOutput = 2;
        int defaultOutputLenght = 4;
        float[][] defaultOutputPatterns = new float[][]
        {
            new float[] { 0, 0 },
            new float[] { 1, 0 },
            new float[] { 1, 1 },
            new float[] { 0, 1 }
        };

        if (defaultNbOutput == nbOutput)
        {
            return defaultOutputPatterns;
        }

        float[][] outputPatterns = new float[defaultOutputLenght][];
        for (int i = 0; i < defaultOutputLenght; i++)
        {
            outputPatterns[i] = new float[nbOutput];
            for (int j = 0; j < nbOutput; j++)
            {
                outputPatterns[i][j] = (j < defaultNbOutput) ? defaultOutputPatterns[i][j] : 0;
            }
        }

        return outputPatterns;
    }

    /// <summary>
    /// Génère des patterns par défaut pour les entrées selon les paramètres de l'application
    /// </summary>
    /// <returns>Tableau des patterns d'entrée</returns>
    float[][] GenerateInputPatterns()
    {
        int defaultNbInput = 4;
        int defaultInputLenght = 4;
        float[][] defaultInputPatterns = new float[][]
        {
            new float[] { 0, 0, 1, 1 },
            new float[] { 0, 1, 0, 0 },
            new float[] { 1, 0, 1, 0 },
            new float[] { 1, 1, 0, 1 }
        };
        
        if (defaultNbInput == nbInput)
        {
            return defaultInputPatterns;
        }

        float[][] inputPatterns = new float[defaultInputLenght][];
        for (int i = 0; i < defaultInputLenght; i++)
        {
            inputPatterns[i] = new float[nbInput];
            for (int j = 0; j < nbInput; j++)
            {
                inputPatterns[i][j] = (j < defaultNbInput) ? defaultInputPatterns[i][j] : 0;
            }
        }
        return inputPatterns;
    }

    /// <summary>
    /// Calcule et affiche les résultats pour chaque pattern d'entrée dans le réseau de neurones
    /// </summary>
    void CalculateAndDisplayResults()
    {
        List<Tuple<float[], float[]>> patterns = new List<Tuple<float[], float[]>>();

        for (int row = 0; row < inputDisplayers.Count / nbInput; row++)
        {
            float[] inputs = new float[nbInput];
            float[] expectedOutputs = new float[nbOutput];

            for (int i = 0; i < nbInput; i++)
            {
                inputs[i] = inputDisplayers[row * nbInput + i].ObjectValue;
            }
            for (int i = 0; i < nbOutput; i++)
            {
                expectedOutputs[i] = expectedOutputDisplayers[row * nbOutput + i].ObjectValue;
            }
            patterns.Add(Tuple.Create(inputs, expectedOutputs));
        }

        neuralNetwork.Train(patterns);
        List<float[]> results = neuralNetwork.Test(patterns);

        DisplayResults(results);
    }

    /// <summary>
    /// Affiche les résultats calculés pour chaque ligne, en mettant à jour les displayers de sorties calculées
    /// </summary>
    /// <param name="results">Liste des résultats calculés</param>
    void DisplayResults(List<float[]> results)
    {
        int row = 0;
        foreach (var outputs in results)
        {
            for (int i = 0; i < outputs.Length; i++)
            {
                int outputIndex = row * outputs.Length + i;
                calculatedOutputDisplayers[outputIndex].UpdateObjectValue(outputs[i]);
                DisplayDisplayer(calculatedOutputDisplayers[outputIndex], inputDisplayers.Count / nbInput + expectedOutputDisplayers.Count / nbOutput + i, row); 
            }
            row++;
        }
    }

    /// <summary>
    /// Positionne le displayer dans la scène à une position spécifique
    /// </summary>
    /// <param name="displayer">Displayer à afficher</param>
    /// <param name="col">Colonne où placer le displayer</param>
    /// <param name="row">Ligne où placer le displayer</param>
    void DisplayDisplayer(Displayer displayer, int col, int row)
    {
        float xOffset = 2.0f;
        float yOffset = -1.5f;

        Vector3 position = new Vector3(col * xOffset, row * yOffset, 0);
        displayer.GameObject.transform.position = position;
        displayer.GameObject.GetComponent<Renderer>().material.color = displayer.GetDisplayerColor();
    }

    /// <summary>
    /// Calcule et affiche les résultats pour une ligne spécifique de données
    /// </summary>
    /// <param name="row">Ligne à calculer et afficher</param>
    void CalculateAndDisplayRow(int row)
    {
        neuralNetwork = new NeuralNetwork(nbInput, nbHiddenNodes, nbOutput, nbTrainIterations);

        float[] inputs = new float[nbInput];
        float[] expectedOutputs = new float[nbOutput];

        for (int i = 0; i < nbInput; i++)
        {
            inputs[i] = inputDisplayers[row * nbInput + i].ObjectValue;
        }
        for (int i = 0; i < nbOutput; i++)
        {
            expectedOutputs[i] = expectedOutputDisplayers[row * nbOutput + i].ObjectValue;
        }
        Tuple<float[], float[]> pattern = new Tuple<float[], float[]>(inputs, expectedOutputs);

        neuralNetwork.Train(pattern);
        float[] outputs = neuralNetwork.Test(pattern);

        for (int i = 0; i < outputs.Length; i++)
         {
            int outputIndex = row * outputs.Length + i;
            calculatedOutputDisplayers[outputIndex].UpdateObjectValue(outputs[i]);
            DisplayDisplayer(calculatedOutputDisplayers[outputIndex], inputDisplayers.Count / nbInput + expectedOutputDisplayers.Count / nbOutput + i, row); 
        }
    }
}