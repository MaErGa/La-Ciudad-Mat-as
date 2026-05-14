using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float maxSpeed = 25f;
    public float acceleration = 8f;
    public float brakeForce = 12f;
    public float turnSpeed = 100f;
    [Header("Físicas")]
    public float drag = 2f;
    public float downforce = 50f;
    private Rigidbody rb;
    private float currentSpeed;
    private float inputVertical;
    private float inputHorizontal;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // Estabilidad
    }
    void Update()
    {
        inputVertical = Input.GetAxis("Vertical");
        inputHorizontal = Input.GetAxis("Horizontal");
    }
    void FixedUpdate()
    {
        // Aceleración y freno
        if (inputVertical > 0)
        {
            currentSpeed += acceleration * inputVertical * Time.fixedDeltaTime;
        }
        else if (inputVertical < 0)
        {
            currentSpeed += brakeForce * inputVertical * Time.fixedDeltaTime;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, drag * Time.fixedDeltaTime);
        }
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.4f, maxSpeed);
        // Movimiento hacia adelante
        Vector3 velocity = transform.forward * currentSpeed;
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        // Giro (solo si hay velocidad)
        if (Mathf.Abs(currentSpeed) > 0.5f)
        {
            float turnAmount = inputHorizontal * turnSpeed * Time.fixedDeltaTime;
            turnAmount *= Mathf.Sign(currentSpeed); // Invertir giro en reversa
            Quaternion turnRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
        // Downforce para estabilidad
        rb.AddForce(-transform.up * downforce * rb.velocity.magnitude);
    }
    public Vector3 GetVelocity() => transform.forward * currentSpeed;
    public float GetSpeed() => currentSpeed;
}