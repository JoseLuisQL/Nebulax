using UnityEngine;

/// <summary>
/// Efectos visuales profesionales para un item coleccionable de Nebulax,
/// construidos por código (sin assets externos):
///  - Halo pulsante detrás de la gema.
///  - Partículas de chispas/brillo orbitando.
///  - Estela suave (TrailRenderer).
///  - Destello al ser recogido (burst de partículas).
///
/// Se añade junto a <see cref="Coleccionable"/>. Es autocontenido y compatible
/// con el pool (recoloca/reinicia en OnEnable).
/// </summary>
public class EfectoColeccionable : MonoBehaviour
{
    [SerializeField] private Color colorNucleo = new Color(0.4f, 0.95f, 1f, 1f);
    [SerializeField] private Color colorBorde = new Color(0.1f, 0.45f, 1f, 1f);
    [SerializeField] private float tamanoHalo = 1.6f;

    private SpriteRenderer halo;
    private ParticleSystem chispas;
    private TrailRenderer estela;
    private float t;

    private void Awake()
    {
        ConstruirHalo();
        ConstruirChispas();
        ConstruirEstela();
    }

    private void OnEnable()
    {
        t = 0f;
        if (estela != null) estela.Clear();
        if (chispas != null) { chispas.Clear(); chispas.Play(); }
    }

    private void Update()
    {
        t += Time.deltaTime;
        if (halo != null)
        {
            // Halo que "respira" en escala y alpha.
            float pulso = 0.5f + 0.5f * Mathf.Sin(t * 3.5f);
            float escala = Mathf.Lerp(tamanoHalo * 0.85f, tamanoHalo * 1.15f, pulso);
            halo.transform.localScale = Vector3.one * escala;
            Color c = halo.color;
            c.a = Mathf.Lerp(0.25f, 0.6f, pulso);
            halo.color = c;
        }
    }

    /// <summary>Destello final al recoger el item (antes de devolverlo al pool).</summary>
    public void EmitirDestello()
    {
        if (chispas != null)
        {
            chispas.Emit(30);
        }
    }

    private void ConstruirHalo()
    {
        GameObject obj = new GameObject("Halo");
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = new Vector3(0f, 0f, 0.01f);

        halo = obj.AddComponent<SpriteRenderer>();
        halo.sprite = TexturaCircular(64);
        halo.color = new Color(colorNucleo.r, colorNucleo.g, colorNucleo.b, 0.4f);
        halo.sortingOrder = 28;
        halo.material = MaterialAditivo();
        obj.transform.localScale = Vector3.one * tamanoHalo;
    }

    private void ConstruirChispas()
    {
        GameObject obj = new GameObject("Chispas");
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = Vector3.zero;

        chispas = obj.AddComponent<ParticleSystem>();
        var main = chispas.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.startColor = new ParticleSystem.MinMaxGradient(colorNucleo, colorBorde);
        main.maxParticles = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;

        var em = chispas.emission;
        em.rateOverTime = 18f;

        var sh = chispas.shape;
        sh.enabled = true;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius = 0.35f;
        sh.radiusThickness = 0f;

        var col = chispas.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(colorNucleo, 0.5f), new GradientColorKey(colorBorde, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);

        var sol = chispas.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(1f, 0f)));

        var pr = chispas.GetComponent<ParticleSystemRenderer>();
        pr.renderMode = ParticleSystemRenderMode.Billboard;
        pr.sortingOrder = 31;
        pr.material = MaterialAditivo();

        chispas.Play();
    }

    private void ConstruirEstela()
    {
        estela = gameObject.GetComponent<TrailRenderer>();
        if (estela == null)
        {
            estela = gameObject.AddComponent<TrailRenderer>();
        }
        estela.time = 0.25f;
        estela.startWidth = 0.18f;
        estela.endWidth = 0.0f;
        estela.minVertexDistance = 0.02f;
        estela.sortingOrder = 29;
        estela.material = MaterialAditivo();

        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(colorNucleo, 0f), new GradientColorKey(colorBorde, 1f) },
            new[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0f, 1f) });
        estela.colorGradient = g;
    }

    private static Material MaterialAditivo()
    {
        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive")
                 ?? Shader.Find("Particles/Additive")
                 ?? Shader.Find("Sprites/Default");
        return new Material(sh);
    }

    private static Sprite TexturaCircular(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 c = new Vector2(size / 2f, size / 2f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / (size * 0.5f);
                float a = Mathf.Clamp01(1f - d);
                a = a * a; // borde suave
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 64f);
    }
}
