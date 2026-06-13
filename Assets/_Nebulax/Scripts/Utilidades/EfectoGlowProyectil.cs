using UnityEngine;

/// <summary>
/// Efecto visual de rayo/plasma en cualquier proyectil.
/// - TrailRenderer: estela brillante que sigue al proyectil.
/// - ParticleSystem: chispas eléctricas radiales.
/// Configurable por color para jugador (cian) o enemigo (naranja/rojo).
/// </summary>
public class EfectoGlowProyectil : MonoBehaviour
{
    [SerializeField] private Color colorNucleo  = new Color(0f,   0.95f, 1f,   1f); // cian por defecto (jugador)
    [SerializeField] private Color colorBorde   = new Color(0f,   0.4f,  1f,   1f);
    [SerializeField] private float anchoEstela  = 0.12f;
    [SerializeField] private float tiempoEstela = 0.15f;
    [SerializeField] private float radioChispas = 0.05f;

    private TrailRenderer trail;
    private ParticleSystem chispas;

    private void Awake()
    {
        CrearEstela();
        CrearChispas();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ESTELA DE PLASMA
    // ─────────────────────────────────────────────────────────────────────────
    private void CrearEstela()
    {
        trail = gameObject.GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }
        
        trail.time               = tiempoEstela;
        trail.startWidth         = anchoEstela;
        trail.endWidth           = 0.003f;
        trail.minVertexDistance  = 0.008f;
        trail.shadowCastingMode  = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows     = false;
        trail.sortingOrder       = 55;

        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white,   0f),
                new GradientColorKey(colorNucleo,   0.25f),
                new GradientColorKey(colorBorde,    0.7f),
                new GradientColorKey(colorBorde * 0.4f, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f,   0f),
                new GradientAlphaKey(0.9f, 0.3f),
                new GradientAlphaKey(0.4f, 0.8f),
                new GradientAlphaKey(0f,   1f)
            });
        trail.colorGradient = g;
        trail.material      = CrearMaterialAdditivo();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CHISPAS ELÉCTRICAS
    // ─────────────────────────────────────────────────────────────────────────
    private void CrearChispas()
    {
        Transform child = transform.Find("ChispasElectricas");
        if (child != null)
        {
            chispas = child.GetComponent<ParticleSystem>();
        }
        else
        {
            GameObject obj = new GameObject("ChispasElectricas");
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = Vector3.zero;
            chispas = obj.AddComponent<ParticleSystem>();
        }

        var main = chispas.main;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.3f,  1.0f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.012f, 0.050f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(1f, 1f, 1f, 1f),
                                   new Color(colorNucleo.r, colorNucleo.g, colorNucleo.b, 0.85f));
        main.maxParticles    = 24;   // reducido (antes 80) para mejor rendimiento
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop            = true;
        main.playOnAwake     = true;

        var em = chispas.emission;
        em.rateOverTime = 18f;        // reducido (antes 50) para mejor rendimiento

        var sh = chispas.shape;
        sh.enabled         = true;
        sh.shapeType       = ParticleSystemShapeType.Circle;
        sh.radius          = radioChispas;
        sh.radiusThickness = 0f;   // Solo en el borde del círculo

        // Tamaño que se desvanece
        var sol = chispas.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.5f, 0.5f),
            new Keyframe(1f, 0f)));

        // Color eléctrico
        var col = chispas.colorOverLifetime;
        col.enabled = true;
        Gradient cg = new Gradient();
        cg.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white,  0f),
                new GradientColorKey(colorNucleo,  0.4f),
                new GradientColorKey(colorBorde,   1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f,   0f),
                new GradientAlphaKey(0.7f, 0.5f),
                new GradientAlphaKey(0f,   1f)
            });
        col.color = new ParticleSystem.MinMaxGradient(cg);

        var rend = chispas.GetComponent<ParticleSystemRenderer>();
        rend.renderMode   = ParticleSystemRenderMode.Billboard;
        rend.sortingOrder = 52;
        rend.material     = CrearMaterialAdditivo();

        chispas.Play();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPER: Material aditivo garantizado
    // ─────────────────────────────────────────────────────────────────────────
    // Material aditivo COMPARTIDO: se crea una sola vez y se reutiliza en todos
    // los proyectiles para evitar crear un material por instancia (clave para el
    // rendimiento cuando el jefe dispara muchos proyectiles).
    private static Material materialAditivoCache;

    internal static Material CrearMaterialAdditivo()
    {
        if (materialAditivoCache != null)
        {
            return materialAditivoCache;
        }

        string[] candidatos = {
            "Legacy Shaders/Particles/Additive",
            "Particles/Additive",
            "Mobile/Particles/Additive",
            "Sprites/Default"
        };
        foreach (string nombre in candidatos)
        {
            Shader sh = Shader.Find(nombre);
            if (sh != null) { materialAditivoCache = new Material(sh); return materialAditivoCache; }
        }
        materialAditivoCache = new Material(Shader.Find("Standard"));
        return materialAditivoCache;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CONFIGURACIÓN EXTERNA (llamado desde el constructor al crear el prefab)
    // ─────────────────────────────────────────────────────────────────────────
    public void ConfigurarComoEnemigo()
    {
        colorNucleo  = new Color(1f, 0.45f, 0.05f, 1f);   // naranja caliente
        colorBorde   = new Color(1f, 0.10f, 0f,    1f);   // rojo lava
        anchoEstela  = 0.10f;
        tiempoEstela = 0.13f;
        radioChispas = 0.04f;

        // Actualizamos los componentes existentes (o los creamos si no existen)
        CrearEstela();
        CrearChispas();
    }
}
