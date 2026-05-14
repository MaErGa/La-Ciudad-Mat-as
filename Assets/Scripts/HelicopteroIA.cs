using UnityEngine;

public class HelicopteroIA : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 10f;

    [Header("Reinicio")]
    public float finCarretera = 300f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Mover en direccion local del helicoptero
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime, Space.Self);

        // Mantener altura original
        transform.position = new Vector3(
            transform.position.x,
            posicionInicial.y,
            transform.position.z
        );

        // Reiniciar si se aleja demasiado
        float distancia = Vector3.Distance(transform.position, posicionInicial);
        if (distancia > finCarretera)
            transform.position = posicionInicial;
    }
}