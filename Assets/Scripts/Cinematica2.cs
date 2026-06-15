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
///    - objeto2: el objetivo/enemigo hacia donde se mueve objeto1
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
    public Transform destinoObjeto2; //meta del objeto 1

    [Header("Configuración de movimiento")]
    // Equivale a metrosSeg en TrasladarObjeto — controla la velocidad general
    public float velocidadMovimiento = 3f;
    // Valor de slow motion: 0.3 = 30% de velocidad normal (EscalarTiempo)
    public float escalaSlowMo = 0.3f;
    // Velocidad del fade entre cámaras (segundos que dura la transición)
    public float velocidadFade = 0.5f;

    // Acumula tiempo frame a frame, igual que TiempoDelta
    private float tiempoTranscurrido = 0f;

    // Canvas/imagen negra para el fade — créala en el Inspector:
    // GameObject > UI > Image, color negro, ocupa toda la pantalla
    // Asígnala aquí desde el Inspector
    public UnityEngine.UI.Image pantallaFade;

    void Start()
    {
        // Al iniciar: solo camaraA visible, camaraB apagada
        camaraA.enabled = true;
        camaraB.enabled = false;

        // Asegura que el fade empieza transparente
        if (pantallaFade != null)
            pantallaFade.color = new Color(0, 0, 0, 0);

        StartCoroutine(SecuenciaCinematica());
    }

    // ─────────────────────────────────────────────────────────────────
    // FADE: oscurece o aclara la pantalla interpolando el alpha
    // Uso interno: yield return StartCoroutine(Fade(0f, 1f)) para fundir a negro
    //              yield return StartCoroutine(Fade(1f, 0f)) para abrir desde negro
    // ─────────────────────────────────────────────────────────────────
    IEnumerator Fade(float alphaInicio, float alphaFin)
    {
        if (pantallaFade == null) yield break; // si no asignaste la imagen, se salta

        float t = 0f;
        while (t < velocidadFade)
        {
            t += Time.unscaledDeltaTime; // usa tiempo real para que funcione con slow mo
            float alpha = Mathf.Lerp(alphaInicio, alphaFin, t / velocidadFade);
            pantallaFade.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        pantallaFade.color = new Color(0, 0, 0, alphaFin);
    }

    IEnumerator SecuenciaCinematica()
    {
        // ─────────────────────────────────────────
        // FASE 1: CámaraA baja desde arriba
        // Basado en Vector3Movimiento (Vector3.down) y TiempoDelta
        // objeto1 rota para mirar a destinoObjeto2 mientras la cámara baja
        // ─────────────────────────────────────────
        float duracionFase1 = 2f;
        tiempoTranscurrido = 0f;

        Vector3 posInicioCam = camaraA.transform.position;
        // Vector3.down es el mismo campo "abajo" de Vector3Movimiento
        Vector3 posFinCam = posInicioCam + Vector3.down * 4f;

        // Rotación inicial y final de objeto1 mirando a objeto2 (MirarHacia)
        Quaternion rotInicio = objeto1.rotation;
        objeto1.LookAt(destinoObjeto2); // calcula adónde tiene que girar
        Quaternion rotFin = objeto1.rotation;
        objeto1.rotation = rotInicio; // regresa para interpolar desde el inicio

        while (tiempoTranscurrido < duracionFase1)
        {
            // TiempoDelta: acumula tiempo real cada frame
            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / duracionFase1;

            // Mueve la cámara suavemente hacia abajo
            camaraA.transform.position = Vector3.Lerp(posInicioCam, posFinCam, t);

            // objeto1 rota progresivamente hacia objeto2 (MirarHacia suavizado)
            objeto1.rotation = Quaternion.Slerp(rotInicio, rotFin, t);

            yield return null;
        }

        // ─────────────────────────────────────────
        // FASE 2: CámaraA se fija mirando a objeto1 (MirarHacia)
        // LookAt es exactamente lo que hace MirarHacia en su Update()
        // ─────────────────────────────────────────
        camaraA.transform.LookAt(objeto1);
        yield return new WaitForSeconds(1f);

        // ─────────────────────────────────────────
        // FASE 3: objeto1 se traslada hacia objeto2 (TrasladarObjeto)
        // La cámara lo sigue con LookAt en tiempo real
        // ─────────────────────────────────────────
        float duracionFase3 = 2f;
        tiempoTranscurrido = 0f;

        Vector3 origenObjeto1 = objeto1.position;
        Vector3 destinoObjeto1 = destinoObjeto2.position;

        while (tiempoTranscurrido < duracionFase3)
        {
            // Time.deltaTime equivale al metrosSeg * Time.deltaTime de TrasladarObjeto
            tiempoTranscurrido += Time.deltaTime;
            objeto1.position = Vector3.Lerp(origenObjeto1, destinoObjeto1,
                                            tiempoTranscurrido / duracionFase3);

            // MirarHacia en tiempo real: la cámara nunca pierde de vista a objeto1
            camaraA.transform.LookAt(objeto1);
            yield return null;
        }

        // ─────────────────────────────────────────
        // FASE 4: Fade a negro → cambio a CámaraB → Fade de vuelta
        // + slow motion al abrir (EscalarTiempo)
        // ─────────────────────────────────────────

        // Oscurece la pantalla (fade out)
        yield return StartCoroutine(Fade(0f, 1f));

        // Con pantalla negra: desactiva camaraA, activa camaraB
        camaraA.enabled = false;
        camaraB.enabled = true;
        // CámaraB apunta a objeto1 desde su ángulo (MirarHacia)
        camaraB.transform.LookAt(objeto1);

        // Activa slow motion antes de revelar la nueva cámara (EscalarTiempo)
        // Cambia escalaSlowMo en el Inspector para ajustar la intensidad
        Time.timeScale = escalaSlowMo;

        // Abre la pantalla desde negro con la nueva cámara ya activa
        yield return StartCoroutine(Fade(1f, 0f));

        // Mantén el slow mo 2 segundos en tiempo real
        // WaitForSecondsRealtime ignora timeScale, por eso funciona con slow mo
        yield return new WaitForSecondsRealtime(2f);

        // Restaura el tiempo normal (EscalarTiempo a 1)
        Time.timeScale = 1f;

        // ─────────────────────────────────────────
        // FASE 5: objeto2 se mueve a posición final (Vector3Constructor)
        // CámaraB lo sigue con LookAt
        // ─────────────────────────────────────────

        // Vector3Constructor: posición destino definida con new Vector3
        // Cambia estos valores en el Inspector o hardcodéalos aquí
        Vector3 posicionFinal = new Vector3(0f, 0f, 10f);

        float duracionFase5 = 1.5f;
        tiempoTranscurrido = 0f;
        Vector3 origenObjeto2 = destinoObjeto2.position;

        while (tiempoTranscurrido < duracionFase5)
        {
            tiempoTranscurrido += Time.deltaTime;
            destinoObjeto2.position = Vector3.Lerp(origenObjeto2, posicionFinal,
                                            tiempoTranscurrido / duracionFase5);

            // CámaraB sigue a objeto2 mientras se mueve (MirarHacia)
            camaraB.transform.LookAt(destinoObjeto2);
            yield return null;
        }

        // ─────────────────────────────────────────
        // FIN: Fade a negro, restaura camaraA para el gameplay
        // Time.realtimeSinceStartup = concepto de TiempoPasado
        // ─────────────────────────────────────────
        yield return StartCoroutine(Fade(0f, 1f));

        camaraB.enabled = false;
        camaraA.enabled = true;

        yield return StartCoroutine(Fade(1f, 0f));

        Debug.Log($"Cinemática terminada. Tiempo real desde inicio: {Time.realtimeSinceStartup}s");
    }
}