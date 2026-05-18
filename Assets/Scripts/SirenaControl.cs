using UnityEngine;

public partial class SirenaControl : MonoBehaviour
{
    public Light luzRoja;
    public Light luzAzul;
    public float velocidad = 0.2f;

    void Start() {
        InvokeRepeating("CambiarLuces", 0, velocidad);
    }

    void CambiarLuces() {
        luzRoja.enabled = !luzRoja.enabled;
        luzAzul.enabled = !luzAzul.enabled;
    }
}