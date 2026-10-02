using UnityEngine;
using System.Collections.Generic;

public class Rotate_Wall : MonoBehaviour
{
    public GameObject Transform; // Asegúrate de asignar esto en el inspector del prefab
    public bool isDoor = false; // Para diferenciar entre paredes normales y puertas

    // Ahora recibimos los booleanos directamente desde el generador
    public void CalculateRotation(bool Forward, bool Back, bool Right, bool Left)
    {
        // Creamos la lista fresca cada vez
        List<int> validRotates = new List<int>() { 0, 90, 180, 270 };
        if(isDoor)
        {
            // Si es una puerta, solo queremos rotarla para que mire hacia el camino, 
            // así que eliminamos las opciones que no corresponden a caminos.
            if (Back || Forward) {validRotates.Remove(0); validRotates.Remove(180);}
            else {validRotates.Remove(90); validRotates.Remove(270);}
        }
        else
        {
            // Para paredes normales, queremos
            // En lugar de RemoveAt (que rompe el orden de la lista), 
            // usamos Remove() para borrar el valor exacto en grados.
            // He mapeado tu lógica original que bloqueaba ángulos específicos:
            if (Back) validRotates.Remove(0);
            if (Left) validRotates.Remove(90);
            if (Forward) validRotates.Remove(180);
            if (Right) validRotates.Remove(270);
        }
        // Seguridad: Si está rodeada por los 4 lados, la lista quedará vacía. 
        // Le damos una rotación por defecto.
        if (validRotates.Count == 0)
        {
            validRotates.Add(0);
        }

        // Elegimos un ángulo aleatorio de los que sobrevivieron
        int anguloElegido = validRotates[Random.Range(0, validRotates.Count)];
        
        // Aplicamos la rotación
        Transform.transform.rotation = Quaternion.Euler(0, anguloElegido, 0);
    }
}