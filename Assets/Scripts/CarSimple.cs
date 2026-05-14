using UnityEngine;

public class CarSimple : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 15f;
    public float velocidadAtras = 15f;
    public float fuerzaGiro = 80f;

    [Header("Freno")]
    public float freno = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 1000f;
        rb.drag = 3f;
        rb.angularDrag = 5f;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void FixedUpdate()
    {
        float acelerador = Input.GetAxis("Vertical");
        float giro = Input.GetAxis("Horizontal");

        float velocidadActual = rb.velocity.magnitude;
        bool yendoAdelante = Vector3.Dot(rb.velocity, transform.forward) > 0;

        // Frenar inercia al cambiar direccion
        if (acelerador < 0 && yendoAdelante)
            rb.AddForce(-transform.forward * 20f, ForceMode.Acceleration);

        if (acelerador > 0 && !yendoAdelante && velocidadActual > 0.5f)
            rb.AddForce(transform.forward * 20f, ForceMode.Acceleration);

        // Adelante y atras
        float fuerza = acelerador > 0 ? velocidad : velocidadAtras;
        rb.AddForce(transform.forward * acelerador * fuerza, ForceMode.Acceleration);

        // Giro
        if (velocidadActual > 0.5f)
        {
            float direccion = yendoAdelante ? 1f : -1f;
            rb.AddTorque(transform.up * giro * fuerzaGiro * direccion, ForceMode.Acceleration);
        }

        // Freno con Espacio
        if (Input.GetKey(KeyCode.Space))
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, freno * Time.fixedDeltaTime);
    }
}