using System;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkDemo : MonoBehaviour
{
    public int nbHiddenNodes = 10;
    public int nbOutput = 2;
    public int nbInput = 4;
    public int nbTrainIterations = 5000; 
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
        InitDisplayers();
        CalculateResultsAndUpdateDisplayers();
    }

    /// <summary>
    /// Surveille les clics pour détecter si un displayer a été cliqué
    /// Met à jour les valeurs d'entrée ou de sortie attendues en conséquence
    /// Relance l'entrainement et le calcul des sorties par le réseau de neurone sur ce nouveau pattern 
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
                    CalculateResultsAndUpdateDisplayers();
                    return; 
                }

                // Vérifie si un displayer de sortie attendue est cliqué
                Displayer clickedExpectedOutputDisplayer = expectedOutputDisplayers.Find(d => d.GameObject == hit.transform.gameObject);
                if (clickedExpectedOutputDisplayer != null)
                {
                    clickedExpectedOutputDisplayer.UpdateObjectValue();
                    CalculateResultsAndUpdateDisplayers();                    
                }
            }
        }
    }

    /// <summary>
    /// Initialise les displayers des inputs, des outputs attendus et calculés 
    /// </summary>
   private void InitDisplayers()
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
                Displayer displayer = new Displayer(inputs[i], Type.Input, cubePrefab, row, i, epsilon);
                inputDisplayers.Add(displayer);
            }

            // Crée et place les displayers des sorties attendues et calculées
            for (int i = 0; i < outputs.Length; i++)
            {
                Displayer expectedDisplayer = new Displayer(outputs[i], Type.ExpectedOutput, spherePrefab, row, inputs.Length + i, epsilon);
                expectedOutputDisplayers.Add(expectedDisplayer);

                Displayer calculatedDisplayer = new Displayer(0, Type.CalculatedOutput, spherePrefab, row, inputs.Length + outputs.Length + i, epsilon);
                calculatedOutputDisplayers.Add(calculatedDisplayer);
            }
        }
    }


    /// <summary>
    /// Génère le tableau de sorties attendues à partir d'un pattern prédéfini et en fonction du nombre de sorties donné par l'utilisateur : nbOutput
    /// </summary>
    /// <returns>Tableau de sortie</returns>
    private float[][] GenerateOutputPatterns()
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
    /// Génère le tableau d'entrées à partir d'un pattern prédéfini et en fonction du nombre d'entrées donné par l'utilisateur : nbInput
    /// </summary>
    /// <returns>Tableau d'entrée</returns>
    private float[][] GenerateInputPatterns()
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
    /// Calcule les résultats (sorties) pour chaque pattern d'entrée dans le réseau de neurones, puis va mettre à jour l'affichage 
    /// </summary>
    private void CalculateResultsAndUpdateDisplayers()
    {
        neuralNetwork = new NeuralNetwork(nbInput, nbHiddenNodes, nbOutput, nbTrainIterations);

        // création de la liste de Tuple contenant les entrée et sorties désirées  
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

        neuralNetwork.Train(patterns); // entraine le réseau de neurones 
        List<float[]> results = neuralNetwork.Test(patterns); // récupère les sorties

        UpdateDisplayers(results);
    }

    /// <summary>
    /// Met à jours les displayers avec les résultats de sortie calculés pour chaque ligne (ce qui met à jour l'affichage)
    /// </summary>
    /// <param name="results">Liste des résultats calculés</param>
    private void UpdateDisplayers(List<float[]> results)
    {
        int row = 0;
        foreach (var outputs in results)
        {
            for (int i = 0; i < outputs.Length; i++)
            {
                int outputIndex = row * outputs.Length + i;
                calculatedOutputDisplayers[outputIndex].UpdateObjectValue(outputs[i]);
            }
            row++;
        }
    }


}
