using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// CINEMATICA 2 - Uso:
/// 1. Crea un GameObject vacío y llámalo "Cinematica2"
/// 2. Arrastra este script al GameObject
/// 3. En el Inspector asigna:
///    - camaraA: tu cámara principal de gameplay
///    - camaraB: cámara lateral del coche (se posiciona sola)
///    - camaraC: cámara frontal baja de la ambulancia (se posiciona sola)
///    - camaraD: cámara aérea cenital — ve los dos vehículos (se posiciona sola)
///    - objeto1:        el coche principal       (se mueve por su propio script)
///    - destinoObjeto2: la ambulancia/enemigo    (se mueve por su propio script)
/// 4. Dale Play — arranca sola
/// 5. Al terminar, camaraA se reactiva para el gameplay
///
/// FADE: se dibuja con GL directamente sobre la pantalla.
/// No necesita Canvas ni Image de UI.
/// </summary>
public class Cinematica2 : MonoBehaviour
{
    [Header("Cámaras")]
    public Camera camaraA; // cámara principal de gameplay
    public Camera camaraB; // lateral del coche
    public Camera camaraC; // frontal baja de la ambulancia
    public Camera camaraD; // aérea cenital — ve los dos

    [Header("Objetos — solo lectura de posición, no se mueven")]
    public Transform objeto1;        // coche principal
    public Transform destinoObjeto2; // ambulancia / enemigo

    [Header("Configuración de cámara")]
    public float velocidadSeguimiento = 5f;
    public float velocidadFade = 1f;
    public float escalaSlowMo = 0.3f;

    // ── Offsets de posición por cámara ───────────────────────────────
    // Todos los valores son relativos al objeto que sigue cada cámara.
    // Puedes ajustarlos en el Inspector sin tocar el código.

    [Header("CamaraA — Trasera del coche (sigue a objeto1)")]
    [Tooltip("Desplazamiento hacia atrás (eje Forward negativo)")]
    public float traseraProfundidad = 6f;
    [Tooltip("Altura sobre el coche")]
    public float traseraAltura = 2f;

    [Header("CamaraB — Lateral del coche (sigue a objeto1)")]
    [Tooltip("Distancia lateral (izquierda del coche)")]
    public float lateralDistancia = 8f;
    [Tooltip("Altura sobre el coche")]
    public float lateralAltura = 3f;
    [Tooltip("Retroceso adicional sobre el eje Forward")]
    public float lateralRetroceso = 1f;

    [Header("CamaraC — Frontal baja de la ambulancia (sigue a destinoObjeto2)")]
    [Tooltip("Distancia delantera (detrás del frente de la ambulancia)")]
    public float frontalProfundidad = 8f;
    [Tooltip("Altura sobre la ambulancia")]
    public float frontalAltura = 2f;
    [Tooltip("Elevación del punto de mira sobre el pivote de la ambulancia")]
    public float frontalLookAtY = 0.5f;

    [Header("CamaraA — Vuelta al gameplay (tras la cenital)")]
    [Tooltip("Objeto al que mirará camaraA al recuperar el control (normalmente objeto1 / el coche)")]
    public Transform objetivoFinalCamaraA;
    [Tooltip("Distancia hacia atras del objetivo al reencuadrar")]
    public float finalProfundidad = 6f;
    [Tooltip("Altura sobre el objetivo al reencuadrar")]
    public float finalAltura = 2f;

    [Header("CamaraD — Cenital entre los dos vehículos")]
    [Tooltip("Altura mínima garantizada")]
    public float cenitalAlturaMin = 20f;
    [Tooltip("Multiplicador de altura según la distancia entre vehículos")]
    public float cenitalMultAltura = 0.75f;
    [Tooltip("FOV mínimo de la cámara cenital")]
    public float cenitalFovMin = 40f;
    [Tooltip("FOV máximo de la cámara cenital")]
    public float cenitalFovMax = 90f;
    [Tooltip("Multiplicador de FOV según la distancia entre vehículos")]
    public float cenitalMultFov = 2f;

    [Header("Pantalla final — Texto parpadeante")]
    [Tooltip("Texto tipo 'Press Any Button' (componente TextMeshProUGUI). El GameObject del texto debe estar activo en la escena desde el inicio, el script solo controla su transparencia.")]
    public TextMeshProUGUI textoPressButton;
    [Tooltip("Contenido del texto, editable sin tocar código")]
    public string mensajePressButton = "PRESS ANY BUTTON";
    [Tooltip("Velocidad del parpadeo del texto (ciclos por segundo)")]
    public float velocidadParpadeo = 1.5f;
    [Tooltip("Marca esto si quieres que el texto se vea durante TODA la cinemática, desde el inicio. Si lo dejas sin marcar, el texto solo aparece al final.")]
    public bool textoVisibleDesdeElInicio = false;

    [Header("Pantalla final — Logo (sin parpadeo)")]
    [Tooltip("Componente Image del logo. El GameObject debe estar activo en la escena desde el inicio, el script solo controla su transparencia.")]
    public Image imagenLogo;
    [Tooltip("Marca esto si quieres que el logo se vea durante TODA la cinemática, desde el inicio. Si lo dejas sin marcar, el logo solo aparece al final.")]
    public bool logoVisibleDesdeElInicio = false;

    // ── Fade por GL ───────────────────────────────────────────────────
    private float _fadeAlpha = 0f;
    private Material _fadeMat;

    void Awake()
    {
        _fadeMat = new Material(Shader.Find("Hidden/Internal-Colored"));
        _fadeMat.hideFlags = HideFlags.HideAndDontSave;
        _fadeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _fadeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _fadeMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        _fadeMat.SetInt("_ZWrite", 0);
    }

    void OnRenderObject()
    {
        if (_fadeAlpha <= 0f) return;

        GL.PushMatrix();
        GL.LoadOrtho();

        _fadeMat.SetPass(0);

        GL.Begin(GL.QUADS);
        GL.Color(new Color(0f, 0f, 0f, _fadeAlpha));
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(1, 0, 0);
        GL.Vertex3(1, 1, 0);
        GL.Vertex3(0, 1, 0);
        GL.End();

        GL.PopMatrix();
    }

    // ── Coroutine de fade ─────────────────────────────────────────────
    IEnumerator Fade(float desde, float hasta)
    {
        float t = 0f;
        while (t < velocidadFade)
        {
            t += Time.unscaledDeltaTime;
            _fadeAlpha = Mathf.Lerp(desde, hasta, t / velocidadFade);
            yield return null;
        }
        _fadeAlpha = hasta;
    }

    // ── Inicio ────────────────────────────────────────────────────────
    void Start()
    {
        camaraA.enabled = true;
        camaraB.enabled = false;
        camaraC.enabled = false;
        camaraD.enabled = false;

        // El texto empieza invisible (alpha 0) salvo que quieras verlo desde el inicio
        if (textoPressButton != null)
        {
            Color c = textoPressButton.color;
            c.a = textoVisibleDesdeElInicio ? 1f : 0f;
            textoPressButton.color = c;
        }

        // El logo empieza invisible (alpha 0) salvo que quieras verlo desde el inicio
        if (imagenLogo != null)
        {
            Color c = imagenLogo.color;
            c.a = logoVisibleDesdeElInicio ? 1f : 0f;
            imagenLogo.color = c;
        }

        RemoveExtraAudioListeners();

        // Si el texto debe parpadear durante toda la cinemática, arrancamos el loop ya
        if (textoVisibleDesdeElInicio)
            StartCoroutine(ParpadearTexto());

        _fadeAlpha = 0f;
        StartCoroutine(SecuenciaCinematica());
    }

    void RemoveExtraAudioListeners()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        foreach (AudioListener al in listeners)
            if (al.gameObject != camaraA.gameObject)
                al.enabled = false;
    }

    // ── Helper: activa solo una cámara ────────────────────────────────
    void ActivarSolo(Camera cam)
    {
        camaraA.enabled = (cam == camaraA);
        camaraB.enabled = (cam == camaraB);
        camaraC.enabled = (cam == camaraC);
        camaraD.enabled = (cam == camaraD);
    }

    // ── Posiciones cinematográficas (usan los offsets del Inspector) ──

    Vector3 PosTrasera(Transform obj)
        => obj.position
           + obj.forward * -traseraProfundidad
           + Vector3.up * traseraAltura;

    Vector3 PosLateral(Transform obj)
    {
        Vector3 izquierda = -obj.right;
        return obj.position
             + izquierda * lateralDistancia
             + Vector3.up * lateralAltura
             + obj.forward * -lateralRetroceso;
    }

    Vector3 PosFrontalBaja(Transform obj)
        => obj.position
           + obj.forward * -frontalProfundidad
           + Vector3.up * frontalAltura;

    Vector3 PosCenital()
    {
        Vector3 puntoMedio = (objeto1.position + destinoObjeto2.position) * 0.5f;
        float distancia = Vector3.Distance(objeto1.position, destinoObjeto2.position);
        float altura = Mathf.Max(cenitalAlturaMin, distancia * cenitalMultAltura);
        return puntoMedio + Vector3.up * altura;
    }

    // ── Parpadeo continuo del texto (loop independiente) ──────────────
    IEnumerator ParpadearTexto()
    {
        while (true)
        {
            if (textoPressButton != null)
            {
                float alpha = (Mathf.Sin(Time.unscaledTime * velocidadParpadeo * Mathf.PI * 2f) + 1f) * 0.5f;
                Color ct = textoPressButton.color;
                ct.a = alpha;
                textoPressButton.color = ct;
            }
            yield return null;
        }
    }

    // ── Pantalla final: logo (fade simple) + texto (parpadeante) + espera de input ──
    IEnumerator MostrarPantallaLogo()
    {
        if (textoPressButton != null)
            textoPressButton.text = mensajePressButton;

        // El logo aparece con un fade simple, sin parpadeo, y se queda fijo
        // Si ya estaba visible desde el inicio, se omite el fade (ya está en alpha 1)
        if (imagenLogo != null && !logoVisibleDesdeElInicio)
        {
            float t = 0f;
            Color c = imagenLogo.color;
            while (t < velocidadFade)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(0f, 1f, t / velocidadFade);
                imagenLogo.color = c;
                yield return null;
            }
            c.a = 1f;
            imagenLogo.color = c;
        }

        // Si el texto ya estaba parpadeando desde el inicio, ese loop sigue corriendo solo
        // Si no, lo arrancamos ahora que empieza la pantalla final
        Coroutine parpadeo = textoVisibleDesdeElInicio ? null : StartCoroutine(ParpadearTexto());

        yield return new WaitUntil(() => Input.anyKeyDown);

        // Solo detenemos el loop que arrancamos aquí; el de Start() sigue su curso normal
        if (parpadeo != null)
            StopCoroutine(parpadeo);

        // Deja el texto totalmente visible al confirmar el input
        if (textoPressButton != null)
        {
            Color ct = textoPressButton.color;
            ct.a = 1f;
            textoPressButton.color = ct;
        }
    }

    // ── Secuencia ─────────────────────────────────────────────────────
    IEnumerator SecuenciaCinematica()
    {
        // FASE 1 (3s): camaraA sigue al coche desde atrás
        ActivarSolo(camaraA);
        float duracion = 3f, tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            camaraA.transform.position = Vector3.Lerp(
                camaraA.transform.position,
                PosTrasera(objeto1),
                velocidadSeguimiento * Time.deltaTime);
            camaraA.transform.LookAt(objeto1);
            yield return null;
        }

        // FASE 2 (3s): fade → camaraB lateral del coche
        yield return StartCoroutine(Fade(0f, 1f));
        ActivarSolo(camaraB);
        yield return StartCoroutine(Fade(1f, 0f));

        tiempo = 0f; duracion = 3f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            camaraB.transform.position = Vector3.Lerp(
                camaraB.transform.position,
                PosLateral(objeto1),
                velocidadSeguimiento * Time.deltaTime);
            camaraB.transform.LookAt(objeto1);
            yield return null;
        }

        // FASE 3 (3s): fade → camaraC frontal baja de la AMBULANCIA + slow mo
        yield return StartCoroutine(Fade(0f, 1f));
        ActivarSolo(camaraC);
        Time.timeScale = escalaSlowMo;
        yield return StartCoroutine(Fade(1f, 0f));

        tiempo = 0f; duracion = 3f;
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            camaraC.transform.position = Vector3.Lerp(
                camaraC.transform.position,
                PosFrontalBaja(destinoObjeto2),
                velocidadSeguimiento * Time.unscaledDeltaTime);
            camaraC.transform.LookAt(destinoObjeto2.position + Vector3.up * frontalLookAtY);
            yield return null;
        }

        Time.timeScale = 1f;

        // FASE 4 (3s): fade → camaraD cenital — ve el coche Y la ambulancia
        yield return StartCoroutine(Fade(0f, 1f));
        ActivarSolo(camaraD);
        yield return StartCoroutine(Fade(1f, 0f));

        tiempo = 0f; duracion = 3f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            camaraD.transform.position = Vector3.Lerp(
                camaraD.transform.position,
                PosCenital(),
                velocidadSeguimiento * Time.deltaTime);

            Vector3 puntoMedio = (objeto1.position + destinoObjeto2.position) * 0.5f;
            camaraD.transform.LookAt(puntoMedio);

            float distancia = Vector3.Distance(objeto1.position, destinoObjeto2.position);
            camaraD.fieldOfView = Mathf.Clamp(distancia * cenitalMultFov, cenitalFovMin, cenitalFovMax);
            yield return null;
        }

        // FIN: fade a negro → restaura camaraA para el gameplay
        yield return StartCoroutine(Fade(0f, 1f));
        ActivarSolo(camaraA);

        // Determina el objetivo: campo asignado en Inspector, o por defecto objeto1
        Transform objetivo = (objetivoFinalCamaraA != null) ? objetivoFinalCamaraA : objeto1;

        // Reposiciona y orienta camaraA antes de hacer el fade de entrada
        camaraA.transform.position = objetivo.position + objetivo.forward * -finalProfundidad + Vector3.up * finalAltura;
        camaraA.transform.LookAt(objetivo);

        yield return StartCoroutine(Fade(1f, 0f));

        // Muestra el logo y el texto "Press Any Button", espera input del jugador
        yield return StartCoroutine(MostrarPantallaLogo());

        Debug.Log($"Cinematica terminada. Tiempo real transcurrido: {Time.realtimeSinceStartup:F1}s");
    }
}