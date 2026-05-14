using UnityEngine;

public class CamaraCoche : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform coche;

    [Header("Posicion")]
    public Vector3 offset = new Vector3(0f, 3f, -6f);

    [Header("Suavidad")]
    public float suavidad = 5f;
    public float suavidadRotacion = 3f;

    void LateUpdate()
    {
        if (coche == null) return;

        // Posicion objetivo detras y encima del coche
        Vector3 posObjetivo = coche.TransformPoint(offset);

        // Mover la camara suavemente
        transform.position = Vector3.Lerp(transform.position, posObjetivo, suavidad * Time.deltaTime);

        // Rotar la camara para mirar al coche
        Quaternion rotObjetivo = Quaternion.LookRotation(coche.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, suavidadRotacion * Time.deltaTime);
    }
}
