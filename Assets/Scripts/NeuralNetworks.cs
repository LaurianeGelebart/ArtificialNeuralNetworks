using System;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork
{
    private int inputNodes; // Nombre de noeuds d'entrée (hors biais)
    private int hiddenNodes; // Nombre de noeuds cachés dans le réseau de neurones
    private int outputNodes; // Nombre de noeuds de sortie
    private int iterations; // Nombre d'itérations d'entraînement 

    private float[] inputs; // Tableau contenant les valeurs d'entrée du réseau de neurones
    private float[] hidden; // Tableau contenant les valeurs des noeuds cachés
    private float[] outputs; // Tableau contenant les valeurs de sortie du réseau de neurones

    private float[,] weightsInputHidden; // Matrice des poids entre les noeuds d'entrée et les noeuds cachés
    private float[,] weightsHiddenOutput; // Matrice des poids entre les noeuds cachés et les noeuds de sortie

    private float[,] changeInputHidden; // Matrice pour stocker les changements de poids entre les noeuds d'entrée et les noeuds cachés
    private float[,] changeHiddenOutput; // Matrice pour stocker les changements de poids entre les noeuds cachés et les noeuds de sortie

    private System.Random rand = new System.Random(); // Générateur de nombres aléatoires pour initialiser les poids du réseau de neurones



    /// <summary>
    /// Initialise le réseau de neurones avec un nombre donné de noeuds d'entrée, cachés et de sortie
    /// </summary>
    /// <param name="inputNodes">Nombre de noeuds d'entrée (hors biais)</param>
    /// <param name="hiddenNodes">Nombre de noeuds cachés</param>
    /// <param name="outputNodes">Nombre de noeuds de sortie</param>
    public NeuralNetwork(int inputNodes, int hiddenNodes, int outputNodes, int iterations)
    {
        this.inputNodes = inputNodes + 1; // +1 pour le noeud de biais
        this.hiddenNodes = hiddenNodes;
        this.outputNodes = outputNodes;
        this.iterations = iterations;

        // Activations des noeuds
        inputs = new float[this.inputNodes];
        hidden = new float[this.hiddenNodes];
        outputs = new float[this.outputNodes];

        // Initialisation des poids
        weightsInputHidden = MakeMatrix(this.inputNodes, this.hiddenNodes);
        weightsHiddenOutput = MakeMatrix(this.hiddenNodes, this.outputNodes);

        InitializeWeights(weightsInputHidden);
        InitializeWeights(weightsHiddenOutput);

        // Poids pour la momentum
        changeInputHidden = MakeMatrix(this.inputNodes, this.hiddenNodes);
        changeHiddenOutput = MakeMatrix(this.hiddenNodes, this.outputNodes);
    }

    /// <summary>
    /// Entraîne le réseau de neurones en utilisant une liste de modèles d'entrée/sortie pour un nombre d'itérations spécifié
    /// </summary>
    /// <param name="patterns">Liste des tuples contenant les entrées et les sorties cibles</param>
    /// <param name="learningRate">Taux d'apprentissage pour la mise à jour des poids (par défaut 0.5)</param>
    /// <param name="momentum">Momentum pour la mise à jour des poids (par défaut 0.1)</param>
    public void Train(List<Tuple<float[], float[]>> patterns, float learningRate = 0.5f, float momentum = 0.1f)
    {
        for (int i = 0; i < iterations; i++)
        {
            float totalError = 0.0f;
            foreach (var pattern in patterns)
            {
                totalError += TrainPattern(pattern, learningRate, momentum);
            }
            ErrorLog(i, totalError);
        }
    }

    /// <summary>
    /// Teste le réseau de neurones avec plusieurs modèles d'entrée/sortie
    /// </summary>
    /// <param name="patterns">Liste des tuples contenant les entrées et les sorties cibles</param>
    /// <returns>Liste des sorties obtenues pour chaque modèle</returns>
    public List<float[]> Test(List<Tuple<float[], float[]>> patterns)
    {
        List<float[]> results = new List<float[]>();
        foreach (var pattern in patterns)
        {
            float[] inputs = pattern.Item1;
            float[] expectedoutputs = pattern.Item2;
            float[] outputs = Update(inputs);
            ConsoleLog(inputs, expectedoutputs, outputs);
            results.Add(outputs);
        }
        return results;
    }


    /// <summary>
    /// Met à jour les activations du réseau de neurones en calculant les entrées, les cachés et les sorties
    /// </summary>
    /// <param name="inputValues">Valeurs d'entrée du modèle</param>
    /// <returns>Les valeurs de sortie obtenues après mise à jour</returns>
    private float[] Update(float[] inputValues)
    {
        if (inputValues.Length != inputNodes - 1)
            throw new ArgumentException("Le nombre d'entrées est incorrect.");

        // Définir l'entrée du biais à 1.0
        inputs[inputNodes - 1] = 1.0f;

        // Activations d'entrée
        for (int i = 0; i < inputNodes - 1; i++)
        {
            inputs[i] = inputValues[i];
        }

        // Activations cachées
        for (int j = 0; j < hiddenNodes; j++)
        {
            float sum = 0.0f;
            for (int i = 0; i < inputNodes; i++)
            {
                sum += inputs[i] * weightsInputHidden[i, j];
            }
            hidden[j] = Sigmoid(sum);
        }

        // Activations de sortie
        for (int k = 0; k < outputNodes; k++)
        {
            float sum = 0.0f;
            for (int j = 0; j < hiddenNodes; j++)
            {
                sum += hidden[j] * weightsHiddenOutput[j, k];
            }
            outputs[k] = Sigmoid(sum);
        }

        return (float[])outputs.Clone();
    }

    /// <summary>
    /// Effectue la rétropropagation pour ajuster les poids en fonction de l'erreur des sorties
    /// </summary>
    /// <param name="targets">Les valeurs cibles des sorties</param>
    /// <param name="learningRate">Taux d'apprentissage pour la mise à jour des poids</param>
    /// <param name="momentum">Momentum pour la mise à jour des poids</param>
    /// <returns>L'erreur totale calculée pour l'itération</returns>
    private float BackPropagate(float[] targets, float learningRate, float momentum)
    {
        float error = 0.0f;

        if (targets.Length != outputNodes)
        {
            throw new ArgumentException("Le nombre de cibles est incorrect.");
        }

        // Calcul des termes d'erreur pour la sortie
        float[] outputDeltas = new float[outputNodes];
        for (int k = 0; k < outputNodes; k++)
        {
            error += 0.5f * (targets[k] - outputs[k]) * (targets[k] - outputs[k]); // Calcul de l'erreur totale
            float delta = targets[k] - outputs[k];
            outputDeltas[k] = SigmoidDerivative(outputs[k]) * delta;
        }

        // Calcul des termes d'erreur pour les couches cachées
        float[] hiddenDeltas = new float[hiddenNodes];
        for (int j = 0; j < hiddenNodes; j++)
        {
            float errorTerm = 0.0f;
            for (int k = 0; k < outputNodes; k++)
            {
                errorTerm += outputDeltas[k] * weightsHiddenOutput[j, k];
            }
            hiddenDeltas[j] = SigmoidDerivative(hidden[j]) * errorTerm;
        }

        // Mise à jour des poids de sortie
        for (int j = 0; j < hiddenNodes; j++)
        {
            for (int k = 0; k < outputNodes; k++)
            {
                float change = outputDeltas[k] * hidden[j];
                weightsHiddenOutput[j, k] += learningRate * change + momentum * changeHiddenOutput[j, k];
                changeHiddenOutput[j, k] = change;
            }
        }

        // Mise à jour des poids d'entrée
        for (int i = 0; i < inputNodes; i++)
        {
            for (int j = 0; j < hiddenNodes; j++)
            {
                float change = hiddenDeltas[j] * inputs[i];
                weightsInputHidden[i, j] += learningRate * change + momentum * changeInputHidden[i, j];
                changeInputHidden[i, j] = change;
            }
        }

        return error; // Retourne l'erreur pour l'itération
    }

    /// <summary>
    /// Entraîne le réseau de neurones pour un seul modèle d'entrée/sortie en effectuant la mise à jour des poids
    /// </summary>
    /// <param name="pattern">Tuple contenant les entrées et les sorties cibles</param>
    /// <param name="learningRate">Taux d'apprentissage pour la mise à jour des poids</param>
    /// <param name="momentum">Momentum pour la mise à jour des poids</param>
    /// <returns>L'erreur calculée pour le modèle d'entraînement</returns>
    private float TrainPattern(Tuple<float[], float[]> pattern, float learningRate, float momentum)
    {
        float[] inputs = pattern.Item1;
        float[] targets = pattern.Item2;
        Update(inputs);
        return BackPropagate(targets, learningRate, momentum);
    }

    /// <summary>
    /// Initialise les poids avec des valeurs aléatoires
    /// </summary>
    /// <param name="weights">La matrice de poids à initialiser</param>
    private void InitializeWeights(float[,] weights)
    {
        for (int i = 0; i < weights.GetLength(0); i++)
        {
            for (int j = 0; j < weights.GetLength(1); j++)
            {
                weights[i, j] = RandomRange(-2.0f, 2.0f);
            }
        }
    }

    /// <summary>
    /// Génère un nombre aléatoire dans une plage spécifiée
    /// </summary>
    /// <param name="min">Valeur minimale</param>
    /// <param name="max">Valeur maximale</param>
    /// <returns>Un nombre aléatoire entre min et max</returns>
    private float RandomRange(float min, float max)
    {
        return (float)(rand.NextDouble() * (max - min) + min);
    }

    /// <summary>
    /// Crée une matrice de taille spécifiée et remplit de valeurs initiales
    /// </summary>
    /// <param name="rows">Nombre de lignes</param>
    /// <param name="cols">Nombre de colonnes</param>
/// <param name="fill">Valeur utilisée pour remplir la matrice (par défaut 0.0f si non spécifiée)</param>
    /// <returns>La matrice nouvellement créée</returns>
    private float[,] MakeMatrix(int rows, int cols, float fill = 0.0f)
    {
        float[,] matrix = new float[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = fill;
            }
        }
        return matrix;
    }

    /// <summary>
    /// Fonction d'activation sigmoïde
    /// </summary>
    /// <param name="x">Entrée de la fonction sigmoïde</param>
    /// <returns>La valeur sigmoïde de l'entrée</returns>
    private float Sigmoid(float x)
    {
        return (float)Math.Tanh(x);
    }

    /// <summary>
    /// Dérivée de la fonction sigmoïde
    /// </summary>
    /// <param name="y">Valeur d'entrée après activation sigmoïde</param>
    /// <returns>Dérivée de la fonction sigmoïde</returns>
    private float SigmoidDerivative(float y)
    {
        return 1.0f - y * y;
    }

    /// <summary>
    /// Affiche les entrées et les sorties dans la console
    /// </summary>
    /// <param name="inputs">Valeurs d'entrée</param>
    /// <param name="outputs">Valeurs de sortie</param>
    private void ConsoleLog(float[] inputs, float[] expectedOutputs, float[] outputs)
    {
        Debug.Log($"Input : {string.Join(", ", inputs)}, Expected outputs : {string.Join(", ", expectedOutputs)} -> Output : {string.Join(", ", outputs)}");
    }

    /// <summary>
    /// Affiche l'erreur à chaque 100 itérations
    /// </summary>
    /// <param name="i">Numéro de l'itération</param>
    /// <param name="error">Erreur totale à l'itération donnée</param>
    private void ErrorLog(int i, float error)
    {
        if (i % 100 == 0)
        {
            Debug.Log($"Erreur à l'itération {i}: {error}");
        }
    }
}
