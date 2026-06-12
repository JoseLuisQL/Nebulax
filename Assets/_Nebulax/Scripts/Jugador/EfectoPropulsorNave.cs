using UnityEngine;

/// <summary>
/// Propulsor de plasma de alta fidelidad visual para la nave jugador.
/// - 3 boquillas (centro + 2 laterales) que reflejan los motores reales del sprite.
/// - Llama base siempre activa (idle pulsante).
/// - Chorro largo y brillante al acelerar hacia arriba.
/// - Casi apagado al bajar (lógica de frenado).
/// - Los GameObjects viven en escena raíz y se siguen vía LateUpdate para
///   evitar la distorsión de escala del padre (0.23f).
/// </summary>
public class EfectoPropulsorNave : MonoBehaviour
{
    // ── Sistemas de partículas ───────────────────────────────────────────────
    private ParticleSystem[] psChorroCentral = new ParticleSystem[1];   // motor central
    private ParticleSystem[] psChorroLateral = new ParticleSystem[2];   // motores laterales
    private ParticleSystem   psNucleo;       // resplandor blanco en la boca
    private ParticleSystem   psHumo;         // cola de humo de iones

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private float velocidadNormY;   // -1 (bajando) → +1 (subiendo)

    // ── Constantes de emisión ────────────────────────────────────────────────
    private const float IDLE_CENTRAL   = 35f;
    private const float BOOST_CENTRAL  = 180f;
    private const float FRENO_CENTRAL  = 6f;

    private const float IDLE_LATERAL   = 20f;
    private const float BOOST_LATERAL  = 110f;
    private const float FRENO_LATERAL  = 3f;

    // ── Separación lateral de las boquillas (en unidades mundo) ─────────────
    private const float SEP_LATERAL = 0.14f;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ConstruirTodo();
    }

    private void Start()  => ReposicionarTodos();
    private void LateUpdate()
    {
        if (rb != null)
            velocidadNormY = Mathf.Clamp(rb.linearVelocity.y / 6f, -1f, 1f);

        ReposicionarTodos();
        ActualizarIntensidad();
    }

    private void OnDestroy()
    {
        DestruirSi(psNucleo);
        DestruirSi(psHumo);
        foreach (var ps in psChorroCentral) DestruirSi(ps);
        foreach (var ps in psChorroLateral) DestruirSi(ps);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  POSICIONAMIENTO DINÁMICO
    // ─────────────────────────────────────────────────────────────────────────
    private void ReposicionarTodos()
    {
        if (sr == null) return;

        float botY = sr.bounds.min.y;         // borde inferior real del sprite
        float cx   = transform.position.x;
        float cz   = transform.position.z;

        // Centro del motor central
        Vector3 posCentro  = new Vector3(cx, botY + 0.03f, cz);
        // Boquillas laterales (ligeramente más arriba)
        Vector3 posIzq     = new Vector3(cx - SEP_LATERAL, botY + 0.06f, cz);
        Vector3 posDer     = new Vector3(cx + SEP_LATERAL, botY + 0.06f, cz);
        // Núcleo de plasma (justo en la boca)
        Vector3 posNucleo  = new Vector3(cx, botY + 0.05f, cz);

        if (psChorroCentral[0] != null) psChorroCentral[0].transform.position = posCentro;
        if (psChorroLateral[0] != null) psChorroLateral[0].transform.position = posIzq;
        if (psChorroLateral[1] != null) psChorroLateral[1].transform.position = posDer;
        if (psNucleo != null)           psNucleo.transform.position           = posNucleo;
        if (psHumo   != null)           psHumo.transform.position             = posCentro;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  AJUSTE DINÁMICO DE INTENSIDAD
    // ─────────────────────────────────────────────────────────────────────────
    private void ActualizarIntensidad()
    {
        float t = Mathf.InverseLerp(-1f, 1f, velocidadNormY);   // 0=freno → 1=boost
        bool  idle = Mathf.Abs(velocidadNormY) < 0.12f;

        // ── Emisión ──
        float emiCentral = idle ? IDLE_CENTRAL  : Mathf.Lerp(FRENO_CENTRAL,  BOOST_CENTRAL,  t);
        float emiLateral = idle ? IDLE_LATERAL  : Mathf.Lerp(FRENO_LATERAL,  BOOST_LATERAL,  t);
        float emiNucleo  = idle ? 30f            : Mathf.Lerp(5f,   80f,  t);
        float emiHumo    = idle ? 0f             : Mathf.Lerp(0f,   40f,  t);

        SetEmision(psChorroCentral[0], emiCentral);
        SetEmision(psChorroLateral[0], emiLateral);
        SetEmision(psChorroLateral[1], emiLateral);
        SetEmision(psNucleo,           emiNucleo);
        SetEmision(psHumo,             emiHumo);

        // ── Velocidad del chorro (boost) ──
        float velMin = Mathf.Lerp(0.8f, 4.5f, t);
        float velMax = Mathf.Lerp(2.0f, 8.0f, t);
        ActualizarVelocidad(psChorroCentral[0], velMin, velMax);
        ActualizarVelocidad(psChorroLateral[0], velMin * 0.7f, velMax * 0.7f);
        ActualizarVelocidad(psChorroLateral[1], velMin * 0.7f, velMax * 0.7f);

        // ── Tamaño del núcleo ──
        if (psNucleo != null)
        {
            var m = psNucleo.main;
            float sz = idle ? 0.18f : Mathf.Lerp(0.08f, 0.42f, t);
            m.startSize = new ParticleSystem.MinMaxCurve(sz * 0.6f, sz);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CONSTRUCCIÓN DE PARTÍCULAS
    // ─────────────────────────────────────────────────────────────────────────
    private void ConstruirTodo()
    {
        Color colorNucleo  = new Color(0.7f, 0.98f, 1f, 1f);
        Color colorMedia   = new Color(0.15f, 0.55f, 1f, 0.85f);
        Color colorBorde   = new Color(0.05f, 0.15f, 0.8f, 0.5f);

        // ── Motor central (el más grande) ────────────────────────────────────
        psChorroCentral[0] = CrearChorro("Motor_Central",
            angulo: 7f, radio: 0.045f,
            vidaMin: 0.30f, vidaMax: 0.60f,
            tamMin: 0.06f,  tamMax: 0.18f,
            colorA: colorNucleo, colorB: colorMedia, colorC: colorBorde,
            emisionBase: IDLE_CENTRAL, sortingOrder: 38);

        // ── Motores laterales (más finos) ─────────────────────────────────────
        psChorroLateral[0] = CrearChorro("Motor_Izquierdo",
            angulo: 5f, radio: 0.022f,
            vidaMin: 0.22f, vidaMax: 0.45f,
            tamMin: 0.04f,  tamMax: 0.12f,
            colorA: colorNucleo, colorB: colorMedia, colorC: colorBorde,
            emisionBase: IDLE_LATERAL, sortingOrder: 37);

        psChorroLateral[1] = CrearChorro("Motor_Derecho",
            angulo: 5f, radio: 0.022f,
            vidaMin: 0.22f, vidaMax: 0.45f,
            tamMin: 0.04f,  tamMax: 0.12f,
            colorA: colorNucleo, colorB: colorMedia, colorC: colorBorde,
            emisionBase: IDLE_LATERAL, sortingOrder: 37);

        // ── Núcleo de plasma (halo blanco en la boca del motor) ───────────────
        {
            GameObject obj = NuevoObjeto("Nucleo_Plasma", Quaternion.identity);
            psNucleo = obj.AddComponent<ParticleSystem>();

            var m = psNucleo.main;
            m.startLifetime  = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            m.startSpeed     = new ParticleSystem.MinMaxCurve(0.1f,  0.5f);
            m.startSize      = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
            m.startColor     = new ParticleSystem.MinMaxGradient(
                                   new Color(1f, 1f, 1f, 1f),
                                   new Color(0.6f, 0.95f, 1f, 0.8f));
            m.maxParticles   = 100;
            m.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = psNucleo.emission;   em.rateOverTime = 30f;

            var sh = psNucleo.shape;
            sh.enabled    = true;
            sh.shapeType  = ParticleSystemShapeType.Circle;
            sh.radius     = 0.06f;
            sh.radiusThickness = 0f;

            AplicarColorOverLifetime(psNucleo,
                new Color(1f, 1f, 1f, 1f),
                new Color(0.4f, 0.85f, 1f, 0.7f),
                new Color(0.1f, 0.3f, 0.9f, 0f));

            AplicarRenderer(psNucleo, 39);
            psNucleo.Play();
        }

        // ── Humo de iones (cola larga azul oscuro, solo en boost) ─────────────
        {
            GameObject obj = NuevoObjeto("Humo_Iones", Quaternion.Euler(180f, 0f, 0f));
            psHumo = obj.AddComponent<ParticleSystem>();

            var m = psHumo.main;
            m.startLifetime  = new ParticleSystem.MinMaxCurve(0.7f, 1.2f);
            m.startSpeed     = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            m.startSize      = new ParticleSystem.MinMaxCurve(0.03f, 0.09f);
            m.startColor     = new ParticleSystem.MinMaxGradient(
                                   new Color(0.3f, 0.7f, 1f, 0.5f),
                                   new Color(0.1f, 0.2f, 0.7f, 0.2f));
            m.maxParticles   = 200;
            m.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = psHumo.emission;   em.rateOverTime = 0f;

            var sh = psHumo.shape;
            sh.enabled   = true;
            sh.shapeType = ParticleSystemShapeType.Cone;
            sh.angle     = 4f;
            sh.radius    = 0.04f;

            AplicarSizeOverLifetime(psHumo, 0.8f, 0.35f, 0f);
            AplicarColorOverLifetime(psHumo,
                new Color(0.5f, 0.85f, 1f, 0.45f),
                new Color(0.2f, 0.45f, 0.9f, 0.2f),
                new Color(0.05f, 0.1f, 0.5f, 0f));

            AplicarRenderer(psHumo, 36);
            psHumo.Play();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  FACTORY: crea un chorro de motor
    // ─────────────────────────────────────────────────────────────────────────
    private ParticleSystem CrearChorro(string nombre,
        float angulo, float radio,
        float vidaMin, float vidaMax, float tamMin, float tamMax,
        Color colorA, Color colorB, Color colorC,
        float emisionBase, int sortingOrder)
    {
        GameObject obj = NuevoObjeto(nombre, Quaternion.Euler(180f, 0f, 0f));
        ParticleSystem ps = obj.AddComponent<ParticleSystem>();

        var m = ps.main;
        m.startLifetime   = new ParticleSystem.MinMaxCurve(vidaMin, vidaMax);
        m.startSpeed      = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        m.startSize       = new ParticleSystem.MinMaxCurve(tamMin, tamMax);
        m.startColor      = new ParticleSystem.MinMaxGradient(colorA, colorB);
        m.maxParticles    = 400;
        m.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission;   em.rateOverTime = emisionBase;

        var sh = ps.shape;
        sh.enabled    = true;
        sh.shapeType  = ParticleSystemShapeType.Cone;
        sh.angle      = angulo;
        sh.radius     = radio;

        AplicarSizeOverLifetime(ps, 1f, 0.5f, 0f);
        AplicarColorOverLifetime(ps, colorA, colorB, colorC);
        AplicarRenderer(ps, sortingOrder);

        ps.Play();
        return ps;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────────
    private GameObject NuevoObjeto(string nombre, Quaternion rot)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.position = transform.position;
        obj.transform.rotation = rot;
        return obj;
    }

    private static void SetEmision(ParticleSystem ps, float rate)
    {
        if (ps == null) return;
        var em = ps.emission;
        em.rateOverTime = rate;
    }

    private static void ActualizarVelocidad(ParticleSystem ps, float min, float max)
    {
        if (ps == null) return;
        var m = ps.main;
        m.startSpeed = new ParticleSystem.MinMaxCurve(min, max);
    }

    private static void AplicarSizeOverLifetime(ParticleSystem ps, float k0, float k05, float k1)
    {
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, k0), new Keyframe(0.5f, k05), new Keyframe(1f, k1)));
    }

    private static void AplicarColorOverLifetime(ParticleSystem ps, Color c0, Color c1, Color c2)
    {
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(c0, 0f),
                new GradientColorKey(c1, 0.5f),
                new GradientColorKey(c2, 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(c0.a, 0f),
                new GradientAlphaKey(c1.a, 0.5f),
                new GradientAlphaKey(0f,   1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);
    }

    private static void AplicarRenderer(ParticleSystem ps, int sortingOrder)
    {
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.renderMode   = ParticleSystemRenderMode.Billboard;
        r.sortingOrder = sortingOrder;

        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive")
                 ?? Shader.Find("Particles/Additive")
                 ?? Shader.Find("Mobile/Particles/Additive")
                 ?? Shader.Find("Sprites/Default");
        if (sh != null) r.material = new Material(sh);
    }

    private static void DestruirSi(ParticleSystem ps)
    {
        if (ps != null) Object.Destroy(ps.gameObject);
    }
}
