using UnityEngine;

public class SeguirCoche : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    [Tooltip("Arrastra aquí el coche al que debe seguir esta luz")]
    public Transform coche;

    // Guardamos la distancia y rotación inicial para que no se descoloque al dar Play
    private Vector3 offsetPosicion;
    private Quaternion offsetRotacion;

    void Start()
    {
        if (coche != null)
        {
            // Calculamos la distancia relativa inicial entre la luz y el coche
            offsetPosicion = transform.position - coche.position;
            offsetRotacion = Quaternion.Inverse(coche.rotation) * transform.rotation;
        }
        else
        {
            Debug.LogError("¡No has asignado el coche en el script de la luz!", this);
        }
    }

    // Usamos LateUpdate para que la luz se mueva JUSTO DESPUÉS de que el coche se haya movido en ese frame
    void LateUpdate()
    {
        if (coche != null)
        {
            // Mueve la luz siguiendo la posición del coche + su distancia inicial
            transform.position = coche.position + (coche.rotation * offsetPosicion);

            // Rota la luz para que apunte siempre hacia donde apunta el coche
            transform.rotation = coche.rotation * offsetRotacion;
        }
    }
}