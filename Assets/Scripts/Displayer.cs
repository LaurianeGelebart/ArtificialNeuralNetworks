using System;
using UnityEngine;

/// <summary>
/// Enumération des types d'objets affichés : entrée, sortie calculée et sortie attendue
/// </summary>
public enum Type
{
    Input,
    CalculatedOutput,
    ExpectedOutput
}

/// <summary>
/// Classe Displayer pour visualiser les valeurs et états dans une interface Unity
/// </summary>
public class Displayer
{
    private float objectValue; // Valeur de l'objet affiché
    private Type objectType; // Type de l'objet (entrée, sortie calculée, sortie attendue)
    private Vector3 position; // Position de l'objet dans l'espace
    private float epsilon; // Tolérance pour comparer les valeurs
    private GameObject gameObject; // Référence à l'objet Unity affiché

    // Getters
    public Type Type => objectType;
    public Vector3 Position => position;
    public float ObjectValue => objectValue;
    public GameObject GameObject
    {
        get => gameObject;
        set
        {
            gameObject = value;
        }
    }


    /// <summary>
    /// Constructeur pour initialiser un objet Displayer avec sa valeur, son type, un prefab Unity, 
    /// une ligne d'affichage et une tolérance d'approximation des valeurs.
    /// </summary>
    /// <param name="val">Valeur initiale de l'objet</param>
    /// <param name="type">Type de l'objet (entrée, sortie calculée ou attendue)</param>
    /// <param name="prefab">Prefab Unity pour représenter l'objet</param>
    /// <param name="row">Ligne associée à l'objet</param>
    /// <param name="col">Colonne associée à l'objet</param>
    /// <param name="epsilon">Tolérance pour comparer les valeurs (ex: pour vérifier une proximité avec 0 ou 1)</param>
    public Displayer(float val, Type type, GameObject prefab, int row, int col, float epsilon)
    {
        this.objectValue = val;
        this.objectType = type;
        this.gameObject = GameObject.Instantiate(prefab);
        this.gameObject.GetComponent<Renderer>().material.color = GetDisplayerColor(); 

        this.epsilon = epsilon;

         float xOffset = 2.0f;
         float yOffset = -1.5f;
        if(type==Type.CalculatedOutput){
            xOffset = 2.2f;
        }
        this.position = new Vector3(col * xOffset, row * yOffset, 0);
        this.gameObject.transform.position = this.position;
    }



    /// <summary>
    /// Met à jour la valeur de l'objet en basculant entre 0 et 1, puis met à jour la couleur d'affichage.
    /// </summary>
    public void UpdateObjectValue()
    {
        this.objectValue = Mathf.Abs(1 - this.objectValue); 
        if (this.gameObject != null)
        {
            this.gameObject.GetComponent<Renderer>().material.color = GetDisplayerColor(); // Met à jour la couleur
        }
    }

    /// <summary>
    /// Met à jour la valeur de l'objet avec une nouvelle valeur spécifiée, puis met à jour la couleur d'affichage.
    /// </summary>
    /// <param name="newObjectValue">Nouvelle valeur pour l'objet</param>
    public void UpdateObjectValue(float newObjectValue)
    {
        this.objectValue = newObjectValue;
        if (this.gameObject != null)
        {
            this.gameObject.GetComponent<Renderer>().material.color = GetDisplayerColor(); // Met à jour la couleur
        }
    }

    /// <summary>
    /// Détermine la couleur d'affichage en fonction de la valeur de l'objet et de la tolérance epsilon.
    /// Retourne blanc si la valeur est proche de 0, noir si proche de 1, sinon rouge.
    /// </summary>
    /// <returns>Couleur à appliquer pour représenter la valeur de l'objet</returns>
    public Color GetDisplayerColor()
    {
        if (Mathf.Abs(this.objectValue) <= epsilon) return Color.white;
        if (Mathf.Abs(this.objectValue - 1) <= epsilon) return Color.black;

        return Color.red;
    }
}
