using UnityEngine;

/// <summary>
/// Car Player Controller para Unity 3D
/// Requiere: Rigidbody + 4 WheelColliders en el coche
/// Autor: generado con Claude
/// </summary>
public class CarController : MonoBehaviour
{
    [Header("Ruedas - WheelColliders")]
    public WheelCollider wheelFL; // Delantera izquierda
    public WheelCollider wheelFR; // Delantera derecha
    public WheelCollider wheelRL; // Trasera izquierda
    public WheelCollider wheelRR; // Trasera derecha

    [Header("Meshes visuales de las ruedas (opcional)")]
    public Transform meshFL;
    public Transform meshFR;
    public Transform meshRL;
    public Transform meshRR;

    [Header("Configuración del motor")]
    [Tooltip("Torque máximo del motor en Nm")]
    public float motorTorque = 1500f;

    [Tooltip("Torque de freno en Nm")]
    public float brakeTorque = 3000f;

    [Tooltip("Velocidad máxima en km/h")]
    public float maxSpeed = 180f;

    [Header("Configuración de dirección")]
    [Tooltip("Ángulo máximo de giro de las ruedas delanteras")]
    public float maxSteerAngle = 30f;

    [Tooltip("Suavidad del giro (mayor = más suave)")]
    [Range(1f, 10f)]
    public float steerSmoothness = 5f;

    [Header("Tipo de tracción")]
    public DriveType driveType = DriveType.AllWheelDrive;

    [Header("Freno de mano")]
    public bool handbrakeAffectsRear = true;

    [Header("Centro de masa (ajusta estabilidad)")]
    public Vector3 centerOfMassOffset = new Vector3(0f, -0.5f, 0f);

    // --- Variables privadas ---
    private Rigidbody rb;
    private float currentSteerAngle;
    private float currentSpeed;

    // Inputs
    private float throttleInput;
    private float steerInput;
    private float brakeInput;
    private bool handbrakeInput;

    public enum DriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        AllWheelDrive
    }

    // -------------------------------------------------------
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("[CarController] Falta el componente Rigidbody en el coche.");
            return;
        }

        // Bajar el centro de masa para más estabilidad
        rb.centerOfMass += centerOfMassOffset;
    }

    // -------------------------------------------------------
    void Update()
    {
        GatherInput();
        UpdateWheelMeshes();
        DisplayDebugHUD();
    }

    // -------------------------------------------------------
    void FixedUpdate()
    {
        currentSpeed = rb.velocity.magnitude * 3.6f; // convertir m/s → km/h
        ApplySteering();
        ApplyMotor();
        ApplyBrakes();
    }

    // -------------------------------------------------------
    /// <summary>Recoger inputs del jugador</summary>
    void GatherInput()
    {
        // Acelerador / marcha atrás
        throttleInput = Input.GetAxis("Vertical");       // W/S o flechas arriba/abajo

        // Dirección
        steerInput = Input.GetAxis("Horizontal");        // A/D o flechas izquierda/derecha

        // Freno independiente (Espacio) — también actúa la retención del motor en Vertical=0
        brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        // Freno de mano
        handbrakeInput = Input.GetKey(KeyCode.LeftShift);
    }

    // -------------------------------------------------------
    /// <summary>Aplicar torque de dirección a las ruedas delanteras</summary>
    void ApplySteering()
    {
        float targetAngle = maxSteerAngle * steerInput;

        // Suavizar el giro para evitar oversteer brusco
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, Time.fixedDeltaTime * steerSmoothness);

        wheelFL.steerAngle = currentSteerAngle;
        wheelFR.steerAngle = currentSteerAngle;
    }

    // -------------------------------------------------------
    /// <summary>Aplicar torque del motor según el tipo de tracción</summary>
    void ApplyMotor()
    {
        // Limitar velocidad máxima
        float torque = (currentSpeed < maxSpeed) ? throttleInput * motorTorque : 0f;

        switch (driveType)
        {
            case DriveType.FrontWheelDrive:
                wheelFL.motorTorque = torque;
                wheelFR.motorTorque = torque;
                wheelRL.motorTorque = 0f;
                wheelRR.motorTorque = 0f;
                break;

            case DriveType.RearWheelDrive:
                wheelFL.motorTorque = 0f;
                wheelFR.motorTorque = 0f;
                wheelRL.motorTorque = torque;
                wheelRR.motorTorque = torque;
                break;

            case DriveType.AllWheelDrive:
                float split = torque / 4f;
                wheelFL.motorTorque = split;
                wheelFR.motorTorque = split;
                wheelRL.motorTorque = split;
                wheelRR.motorTorque = split;
                break;
        }
    }

    // -------------------------------------------------------
    /// <summary>Aplicar frenos normales y freno de mano</summary>
    void ApplyBrakes()
    {
        if (handbrakeInput && handbrakeAffectsRear)
        {
            // Freno de mano: bloquea ruedas traseras (deriva)
            wheelFL.brakeTorque = 0f;
            wheelFR.brakeTorque = 0f;
            wheelRL.brakeTorque = brakeTorque * 2f;
            wheelRR.brakeTorque = brakeTorque * 2f;

            // Quitar motor en ruedas traseras
            wheelRL.motorTorque = 0f;
            wheelRR.motorTorque = 0f;
        }
        else if (brakeInput > 0f)
        {
            // Freno normal en las 4 ruedas
            float brake = brakeInput * brakeTorque;
            wheelFL.brakeTorque = brake;
            wheelFR.brakeTorque = brake;
            wheelRL.brakeTorque = brake;
            wheelRR.brakeTorque = brake;
        }
        else
        {
            // Sin frenado
            wheelFL.brakeTorque = 0f;
            wheelFR.brakeTorque = 0f;
            wheelRL.brakeTorque = 0f;
            wheelRR.brakeTorque = 0f;
        }
    }

    // -------------------------------------------------------
    /// <summary>
    /// Sincronizar la posición/rotación visual de cada rueda
    /// con su WheelCollider correspondiente.
    /// </summary>
    void UpdateWheelMeshes()
    {
        UpdateSingleMesh(wheelFL, meshFL);
        UpdateSingleMesh(wheelFR, meshFR);
        UpdateSingleMesh(wheelRL, meshRL);
        UpdateSingleMesh(wheelRR, meshRR);
    }

    void UpdateSingleMesh(WheelCollider col, Transform mesh)
    {
        if (mesh == null) return;

        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }

    // -------------------------------------------------------
    /// <summary>HUD de depuración en pantalla (solo en Editor / Development Build)</summary>
    void DisplayDebugHUD()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Mostrar velocidad en consola solo cada 0.5s aprox
        // (Para HUD real usa Unity UI o OnGUI)
#endif
    }

    // -------------------------------------------------------
    // Getter público para HUD externo
    public float GetSpeedKMH() => currentSpeed;
    public float GetThrottle() => throttleInput;
    public float GetSteer() => steerInput;
    public bool IsHandbraking() => handbrakeInput;

    // -------------------------------------------------------
    void OnGUI()
    {
        // HUD simple de velocidad — desactívalo si usas Canvas UI
        GUI.color = Color.white;
        GUI.Label(new Rect(20, 20, 300, 30),
            $"Velocidad: {currentSpeed:F1} km/h");
        GUI.Label(new Rect(20, 45, 300, 30),
            $"Acelerador: {throttleInput:F2}  |  Giro: {steerInput:F2}");
        GUI.Label(new Rect(20, 70, 300, 30),
            $"Freno: {(brakeInput > 0 ? "SÍ" : "no")}  |  " +
            $"Freno de mano: {(handbrakeInput ? "SÍ" : "no")}");
    }
}
