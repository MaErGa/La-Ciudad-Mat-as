using UnityEngine;

public class CocheIA : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 8f;

    [Header("Reinicio")]
    public float limiteZ = 100f;      // Z donde reaparece (inicio de la carretera)
    public float finCarretera = -100f; // Z donde termina la carretera

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Mover hacia adelante
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);

        // Si llega al final, volver al inicio
        if (transform.position.z < finCarretera)
        {
            transform.position = posicionInicial;
        }
    }
}
