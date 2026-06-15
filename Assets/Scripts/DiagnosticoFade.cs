using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DIAGNOSTICO FADE - Uso:
/// 1. Crea un GameObject vacío y arrastra este script
/// 2. Asigna la misma Image negra que usas en Cinematica2
/// 3. Dale Play y mira la Consola
/// 4. Comparte aquí lo que aparece
/// </summary>
public class DiagnosticoFade : MonoBehaviour
{
    public Image pantallaFade;

    void Start()
    {
        if (pantallaFade == null)
        {
            Debug.LogError("FADE: pantallaFade es NULL — no está asignada en el Inspector");
            return;
        }

        Debug.Log($"FADE: Image encontrada en objeto '{pantallaFade.gameObject.name}'");

        Canvas canvas = pantallaFade.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("FADE: La Image NO tiene un Canvas padre");
            return;
        }

        Debug.Log($"FADE: Canvas encontrado — RenderMode: {canvas.renderMode}");
        Debug.Log($"FADE: Canvas SortOrder: {canvas.sortingOrder}");
        Debug.Log($"FADE: Image color actual: {pantallaFade.color}");
        Debug.Log($"FADE: Image RectTransform sizeDelta: {pantallaFade.rectTransform.sizeDelta}");
        Debug.Log($"FADE: Image RectTransform anchorMin: {pantallaFade.rectTransform.anchorMin}");
        Debug.Log($"FADE: Image RectTransform anchorMax: {pantallaFade.rectTransform.anchorMax}");

        // Prueba directa: pone la pantalla en negro puro
        // Si no ves negro al darle Play, el problema es visual (Canvas/orden)
        pantallaFade.color = new Color(0, 0, 0, 1);
        Debug.Log("FADE: Se aplicó negro puro (alpha=1) — ¿ves la pantalla negra?");
    }
}
