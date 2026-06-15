using System.Collections;
using UnityEngine;

/// <summary>
/// CINEMATICA 2 - Uso:
/// 1. Crea un GameObject vacío en la escena y llámalo "Cinematica2"
/// 2. Arrastra este script al GameObject
/// 3. En el Inspector asigna:
///    - camaraA: tu cámara principal (la que ya tienes en escena)
///    - camaraB: una segunda cámara en posición/ángulo diferente
///    - objeto1: el personaje principal o actor que se mueve
///    - destinoObjeto2: el objetivo/enemigo hacia donde se mueve objeto1
///    - pantallaFade: una UI Image negra que cubra toda la pantalla
/// 4. Dale Play — la cinemática arranca sola desde Start()
/// 5. Al terminar, camaraA se reactiva para volver al gameplay
/// </summary>
public class Cinematica2 : MonoBehaviour
{
    [Header("Cámaras")]
    // Arrastra aquí tu Main Camera desde la jerarquía
    public Camera camaraA;
    // Arrastra aquí una segunda cámara colocada en otro ángulo de la escena
    public Camera camaraB;

    [Header("Objetos de la escena")]
    // Personaje u objeto que se moverá durante la cinemática
    public Transform objeto1;
    // Destino u objetivo al que se dirige objeto1
    public Transform destinoObjeto2;

    [Header("Configuración de movimiento")]
    // Velocidad de traslado de objeto1 hacia el destino
    public float velocidadMovimiento = 3f;
    // Valor de slow motion: 0.3 = 30% de velocidad normal
    public float escalaSlowMo = 0.3f;
    // Duración del fade en segundos
    public float velocidadFade = 0.5f;

    // Canvas/imagen negra para el fade
    // Crea: GameObject > UI > Image, color negro, que ocupe toda la pantalla
    public UnityEngine.UI.Image pantallaFade;

    void Start()
    {
        // CORRECCIÓN 1: Advertencia clara si faltan referencias obligatorias
        if (camaraA == null || camaraB == null || objeto1 == null || destinoObjeto2 == null)
        {
            Debug.LogError("Cinematica2: Faltan referencias en el Inspector. Asigna camaraA, camaraB, objeto1 y destinoObjeto2.");
            return;
        }

        if (pantallaFade == null)
            Debug.LogWarning("Cinematica2: pantallaFade no asignado. Los fades serán ignorados.");

        // Al iniciar: solo camaraA visible, camaraB apagada
        camaraA.enabled = true;
        camaraB.enabled = false;

        // Asegura que el fade empieza transparente
        if (pantallaFade != null)
            pantallaFade.color = new Color(0, 0, 0, 0);

        StartCoroutine(SecuenciaCinematica());
    }

    // ─────────────────────────────────────────────────────────────────
    // FADE: oscurece o aclara la pantalla interpolando el alpha.
    // Usa unscaledDeltaTime para que funcione correctamente con slow motion.
    // yield return StartCoroutine(Fade(0f, 1f)) → funde a negro
    // yield return StartCoroutine(Fade(1f, 0f)) → abre desde negro
    // ─────────────────────────────────────────────────────────────────
    IEnumerator Fade(float alphaInicio, float alphaFin)
    {
        if (pantallaFade == null) yield break;

        float t = 0f;
        while (t < velocidadFade)
        {
            t += Time.unscaledDeltaTime; // tiempo real, no afectado por slow motion
            float alpha = Mathf.Lerp(alphaInicio, alphaFin, t / velocidadFade);
            pantallaFade.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        // Asegura el valor final exacto al terminar el bucle
        pantallaFade.color = new Color(0, 0, 0, alphaFin);
    }

    IEnumerator SecuenciaCinematica()
    {
        // Desactiva todos los scripts de objeto1 para que no se mueva solo
        MonoBehaviour[] scriptsObjeto1 = objeto1.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scriptsObjeto1)
            script.enabled = false;

        // ─────────────────────────────────────────
        // FASE 1: CámaraA baja desde arriba mientras objeto1 gira hacia destinoObjeto2
        // ─────────────────────────────────────────
        float duracionFase1 = 2f;
        float tiempoFase1 = 0f; // CORRECCIÓN 2: variable local por fase, evita reutilizar el campo

        Vector3 posInicioCam = camaraA.transform.position;
        Vector3 posFinCam = posInicioCam + Vector3.down * 4f;

        // Calcula la rotación final de objeto1 mirando a destinoObjeto2
        Quaternion rotInicio = objeto1.rotation;
        objeto1.LookAt(destinoObjeto2);
        Quaternion rotFin = objeto1.rotation;
        objeto1.rotation = rotInicio; // regresa al inicio para interpolar suavemente

        while (tiempoFase1 < duracionFase1)
        {
            tiempoFase1 += Time.deltaTime;
            float t = tiempoFase1 / duracionFase1;

            camaraA.transform.position = Vector3.Lerp(posInicioCam, posFinCam, t);
            objeto1.rotation = Quaternion.Slerp(rotInicio, rotFin, t);

            yield return null;
        }

        // ─────────────────────────────────────────
        // FASE 2: CámaraA apunta a objeto1 y espera un momento
        // ─────────────────────────────────────────
        camaraA.transform.LookAt(objeto1);
        yield return new WaitForSeconds(1f);

        // ─────────────────────────────────────────
        // FASE 3: objeto1 se acerca a destinoObjeto2 (se detiene cerca, no encima)
        // La cámara lo sigue con LookAt en tiempo real
        // ─────────────────────────────────────────
        float duracionFase3 = 2f;
        float tiempoFase3 = 0f; // variable local

        Vector3 origenObjeto1 = objeto1.position;

        // CORRECCIÓN 3: objeto1 se detiene a 1 unidad de distancia del destino,
        // así no se superpone visualmente con destinoObjeto2
        Vector3 direccion = (destinoObjeto2.position - objeto1.position).normalized;
        float distancia = Vector3.Distance(objeto1.position, destinoObjeto2.position);
        float margen = Mathf.Min(1f, distancia * 0.5f); // margen adaptable: nunca más de la mitad
        Vector3 destinoObjeto1 = destinoObjeto2.position - direccion * margen;

        while (tiempoFase3 < duracionFase3)
        {
            tiempoFase3 += Time.deltaTime;
            objeto1.position = Vector3.Lerp(origenObjeto1, destinoObjeto1,
                                            tiempoFase3 / duracionFase3);

            camaraA.transform.LookAt(objeto1);
            yield return null;
        }

        // ─────────────────────────────────────────
        // FASE 4: Fade a negro → cambio a CámaraB → Fade de vuelta con slow motion
        // ─────────────────────────────────────────

        yield return StartCoroutine(Fade(0f, 1f)); // oscurece la pantalla

        // Con pantalla negra: cambia de cámara de forma imperceptible
        camaraA.enabled = false;
        camaraB.enabled = true;
        camaraB.transform.LookAt(objeto1); // CámaraB ya apunta a objeto1

        // Activa slow motion antes de revelar la nueva cámara
        Time.timeScale = escalaSlowMo;

        yield return StartCoroutine(Fade(1f, 0f)); // abre con la nueva cámara

        // Espera en tiempo real (ignora timeScale, funciona con slow motion)
        yield return new WaitForSecondsRealtime(2f);

        // Restaura el tiempo normal
        Time.timeScale = 1f;

        // ─────────────────────────────────────────
        // FASE 5: destinoObjeto2 se mueve a posición final, CámaraB lo sigue
        // ─────────────────────────────────────────

        // CORRECCIÓN 4: La posición final era fija (0,0,10). Ahora es relativa
        // a la posición actual de destinoObjeto2 para que funcione en cualquier escena.
        // Si prefieres una posición absoluta, cambia esto por: new Vector3(0f, 0f, 10f)
        Vector3 posicionFinal = destinoObjeto2.position + new Vector3(3f, 0f, 3f);

        float duracionFase5 = 1.5f;
        float tiempoFase5 = 0f; // variable local
        Vector3 origenObjeto2 = destinoObjeto2.position;

        while (tiempoFase5 < duracionFase5)
        {
            tiempoFase5 += Time.deltaTime;
            destinoObjeto2.position = Vector3.Lerp(origenObjeto2, posicionFinal,
                                            tiempoFase5 / duracionFase5);

            camaraB.transform.LookAt(destinoObjeto2);
            yield return null;
        }

        // ─────────────────────────────────────────
        // FIN: Fade a negro, restaura camaraA para el gameplay
        // ─────────────────────────────────────────
        yield return StartCoroutine(Fade(0f, 1f));

        camaraB.enabled = false;
        camaraA.enabled = true;

        yield return StartCoroutine(Fade(1f, 0f));

        // Reactiva todos los scripts de objeto1 para volver al gameplay
        foreach (MonoBehaviour script in scriptsObjeto1)
            script.enabled = true;

        Debug.Log($"Cinemática terminada. Tiempo real desde inicio: {Time.realtimeSinceStartup:F2}s");
    }
}