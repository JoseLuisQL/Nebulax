using UnityEngine;

/// <summary>
/// Aura de energía profesional para la nave del jugador. La INTENSIDAD del aura
/// crece con el nivel/velocidad: cuanto más sube, más energía visible (halo más
/// grande y brillante, más partículas orbitando). Al subir de nivel emite un
/// "power-up burst".
///
/// Es autocontenido: se construye por código y se puede añadir en runtime con
/// <see cref="ObtenerOCrear"/>.
/// </summary>
public class AuraEnergiaNave : MonoBehaviour
{
    private SpriteRenderer halo;
    private ParticleSystem orbital;
    private ParticleSystem burst;
    private int nivel = 0;
    private float t;
    private float tamanoBaseHalo = 1.5f;

    public static AuraEnergiaNave ObtenerOCrear(GameObject nave)
    {
        AuraEnergiaNave aura = nave.GetComponent<AuraEnergiaNave>();
        if (aura == null)
        {
            aura = nave.AddComponent<AuraEnergiaNave>();
        }
        return aura;
    }

    private void Awake()
    {
        ConstruirHalo();
        ConstruirOrbital();
        ConstruirBurst();
        AplicarIntensidad();
    }

    private void Update()
    {
        if (halo == null) return;
        t += Time.deltaTime;

        // El aura "respira"; la amplitud crece con el nivel.
        float energia = Mathf.Clamp01(nivel / 8f);
        float pulso = 0.5f + 0.5f * Mathf.Sin(t * (3f + energia * 4f));
        float escala = tamanoBaseHalo * (1f + energia * 0.9f) * Mathf.Lerp(0.92f, 1.12f, pulso);
        halo.transform.localScale = Vector3.one * escala;

        Color c = halo.color;
        c.a = Mathf.Lerp(0.0f, 0.55f, energia) * Mathf.Lerp(0.7f, 1f, pulso);
        halo.color = c;

        // Rotación lenta del halo para dar vida.
        halo.transform.Rotate(0f, 0f, (20f + energia * 60f) * Time.deltaTime);
    }

    /// <summary>Sube el nivel del aura y dispara un destello de power-up.</summary>
    public void SubirNivel(int nuevoNivel)
    {
        nivel = nuevoNivel;
        AplicarIntensidad();
        if (burst != null)
        {
            burst.Emit(40 + nivel * 6);
        }
    }

    private void AplicarIntensidad()
    {
        float energia = Mathf.Clamp01(nivel / 8f);

        if (orbital != null)
        {
            var em = orbital.emission;
            em.rateOverTime = 10f + energia * 60f; // más partículas con más nivel
            var main = orbital.main;
            // Tinte de azul (bajo) a cian/blanco (alto).
            Color colA = Color.Lerp(new Color(0.3f, 0.7f, 1f), new Color(0.6f, 1f, 1f), energia);
            Color colB = Color.Lerp(new Color(0.1f, 0.4f, 1f), new Color(0.2f, 0.9f, 1f), energia);
            main.startColor = new ParticleSystem.MinMaxGradient(colA, colB);
        }
    }

    // ── Construcción de efectos ────────────────────────────────────────────────
    private void ConstruirHalo()
    {
        GameObject obj = new GameObject("AuraHalo");
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = Vector3.zero;

        halo = obj.AddComponent<SpriteRenderer>();
        halo.sprite = TexturaHalo(128);
        halo.color = new Color(0.4f, 0.85f, 1f, 0f);
        halo.sortingOrder = 18; // detrás de la nave (sortingOrder 20)
        halo.material = MaterialAditivo();
    }

    private void ConstruirOrbital()
    {
        GameObject obj = new GameObject("AuraOrbital");
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = Vector3.zero;

        orbital = obj.AddComponent<ParticleSystem>();
        var main = orbital.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;

        var em = orbital.emission;
        em.rateOverTime = 10f;

        var sh = orbital.shape;
        sh.enabled = true;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius = 0.6f;
        sh.radiusThickness = 0.2f;

        var col = orbital.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.4f, 0.8f, 1f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.9f, 0.3f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);

        var sol = orbital.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.3f), new Keyframe(0.4f, 1f), new Keyframe(1f, 0f)));

        var pr = orbital.GetComponent<ParticleSystemRenderer>();
        pr.renderMode = ParticleSystemRenderMode.Billboard;
        pr.sortingOrder = 19;
        pr.material = MaterialAditivo();
        orbital.Play();
    }

    private void ConstruirBurst()
    {
        GameObject obj = new GameObject("AuraBurst");
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = Vector3.zero;

        burst = obj.AddComponent<ParticleSystem>();
        var main = burst.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(0.4f, 0.9f, 1f));
        main.maxParticles = 300;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;

        var em = burst.emission;
        em.enabled = true;
        em.rateOverTime = 0f; // solo emisión manual (Emit)

        var sh = burst.shape;
        sh.enabled = true;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius = 0.1f;

        var col = burst.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.3f, 0.8f, 1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);

        var pr = burst.GetComponent<ParticleSystemRenderer>();
        pr.renderMode = ParticleSystemRenderMode.Billboard;
        pr.sortingOrder = 25;
        pr.material = MaterialAditivo();
    }

    private static Material MaterialAditivo()
    {
        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive")
                 ?? Shader.Find("Particles/Additive")
                 ?? Shader.Find("Sprites/Default");
        return new Material(sh);
    }

    private static Sprite TexturaHalo(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 c = new Vector2(size / 2f, size / 2f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / (size * 0.5f);
                // Anillo suave: brillo hacia el borde medio, hueco al centro.
                float a = Mathf.Clamp01(1f - d);
                a = Mathf.Pow(a, 1.6f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 64f);
    }
}
