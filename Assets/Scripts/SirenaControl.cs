using UnityEngine;

public partial class SirenaControl : MonoBehaviour
{
    [Header("Configuración de Luces")]
    public Light luzRoja;
    public Light luzAzul;
    public float velocidad = 0.2f;

    [Header("Configuración de Rotación")]
    [Tooltip("Arrastra aquí los objetos (ejes) de las luces que van a girar")]
    public Transform luzRojaEje;
    public Transform luzAzulEje;
    public float velocidadRotacion = 400f;

    void Start() {
        // Tu sistema original de parpadeo intermitente
        InvokeRepeating("CambiarLuces", 0, velocidad);
    }

    void Update() {
        // Hacemos que los ejes de las luces giren frame a frame de forma continua
        if (luzRojaEje != null) {
            luzRojaEje.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);
        }
        if (luzAzulEje != null) {
            luzAzulEje.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);
        }
    }

    void CambiarLuces() {
        luzRoja.enabled = !luzRoja.enabled;
        luzAzul.enabled = !luzAzul.enabled;
    }
}