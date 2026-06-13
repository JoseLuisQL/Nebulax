using UnityEngine;

/// <summary>
/// Guía visual profesional para la apertura del arco de batalla.
/// Crea un portal de energía pulsante con efectos que indican
/// exactamente dónde debe pasar la nave jugador.
/// Se posiciona automáticamente en el centro de la apertura del sprite.
/// </summary>
public class GuiaArcoEstructura : MonoBehaviour
{
    // ── Elementos visuales ────────────────────────────────────────────────────
    private GameObject[]     flechas          = new GameObject[3];
    private SpriteRenderer[] flechaRenderers  = new SpriteRenderer[3];
    private SpriteRenderer   portalRenderer;
    private SpriteRenderer[] anillosRenderer  = new SpriteRenderer[2];
    private GameObject[]     anillos          = new GameObject[2];
    private LineRenderer[]   laseres          = new LineRenderer[2];
    private ParticleSystem   psElectrico;
    private ParticleSystem   psDestello;

    private float t; // tiempo acumulado

    // La apertura está en la parte inferior-central del sprite.
    // Estos offsets son en espacio LOCAL del sprite (antes de escala).
    // El sprite tiene la apertura centrada en X ≈ 0, Y ≈ -1.0 (parte baja del arco)
    private float aperturaY;
    private float aperturaW;
    private float aperturaH;

    private void Awake()
    {
        CalcularApertura();
        Construir();
    }

    private void Update() { t += Time.deltaTime; Animar(); }

    // ─────────────────────────────────────────────────────────────────────────
    //  DETECTAR AUTOMÁTICAMENTE LA APERTURA
    // ─────────────────────────────────────────────────────────────────────────
    private void CalcularApertura()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            // El sprite del arco: la apertura está aprox en el tercio inferior, centrada
            // Usamos los bounds del sprite para calcular
            Bounds b = sr.sprite.bounds;   // en espacio LOCAL del sprite (antes de escala)

            // Apertura: centro X = 0, centro Y = parte baja del sprite
            // Ancho de apertura ≈ 35% del ancho total del sprite
            // Alto de apertura  ≈ 40% del alto total del sprite
            aperturaW = b.size.x * 0.30f;
            aperturaH = b.size.y * 0.35f;
            aperturaY = b.center.y - b.size.y * 0.15f;  // ligeramente debajo del centro
        }
        else
        {
            // Fallback razonable
            aperturaW = 1.5f;
            aperturaH = 1.2f;
            aperturaY = -0.8f;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CONSTRUCCIÓN
    // ─────────────────────────────────────────────────────────────────────────
    private void Construir()
    {
        CrearPortalEnergia();
        CrearAnillosConcentricos();
        CrearFlechasChevron();
        CrearParticulasElectricas();
        CrearLaseresLaterales();
    }

    // ── Portal de energía verde semitransparente ─────────────────────────────
    private void CrearPortalEnergia()
    {
        GameObject obj = Hijo("PortalEnergia");
        obj.transform.localPosition = new Vector3(0f, aperturaY, -0.05f);
        obj.transform.localScale    = new Vector3(aperturaW, aperturaH, 1f);

        portalRenderer = obj.AddComponent<SpriteRenderer>();
        portalRenderer.sprite       = TexQuad(64, 64);
        portalRenderer.color        = new Color(0f, 1f, 0.5f, 0.18f);
        portalRenderer.sortingOrder = 12;
    }

    // ── Dos anillos que se expanden y desvanecen en ciclo ────────────────────
    private void CrearAnillosConcentricos()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject obj = Hijo("Anillo_" + i);
            obj.transform.localPosition = new Vector3(0f, aperturaY, -0.04f);
            obj.transform.localScale    = Vector3.one * 0.8f;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite       = TexAnillo(128, 128, 8);
            sr.color        = new Color(0f, 1f, 0.55f, 0.6f);
            sr.sortingOrder = 13;

            anillos[i]         = obj;
            anillosRenderer[i] = sr;
        }
    }

    // ── 3 Chevrones (▼) animados ─────────────────────────────────────────────
    private void CrearFlechasChevron()
    {
        float[] xOff = { -aperturaW * 0.3f, 0f, aperturaW * 0.3f };

        for (int i = 0; i < 3; i++)
        {
            GameObject obj = Hijo("Chevron_" + i);
            obj.transform.localPosition = new Vector3(xOff[i], aperturaY - aperturaH * 0.5f - 0.3f, -0.1f);
            obj.transform.localScale    = new Vector3(0.18f, 0.18f, 1f);

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite       = TexChevron(48, 28);
            sr.color        = new Color(0f, 1f, 0.45f, 1f);
            sr.sortingOrder = 15;

            flechas[i]         = obj;
            flechaRenderers[i] = sr;
        }
    }

    // ── Partículas en el borde del portal ─────────────────────────────────────
    private void CrearParticulasElectricas()
    {
        // Arcos eléctricos
        {
            GameObject obj = Hijo("ElectricoPortal");
            obj.transform.localPosition = new Vector3(0f, aperturaY, -0.08f);

            psElectrico = obj.AddComponent<ParticleSystem>();
            var m = psElectrico.main;
            m.startLifetime   = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            m.startSpeed      = new ParticleSystem.MinMaxCurve(0.15f, 0.5f);
            m.startSize       = new ParticleSystem.MinMaxCurve(0.02f, 0.07f);
            m.startColor      = new ParticleSystem.MinMaxGradient(
                                    new Color(0f, 1f, 0.6f, 1f),
                                    new Color(0.3f, 1f, 1f, 0.7f));
            m.maxParticles    = 120;
            m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.loop = true;

            var em = psElectrico.emission;   em.rateOverTime = 35f;
            var sh = psElectrico.shape;
            sh.enabled   = true;
            sh.shapeType = ParticleSystemShapeType.Rectangle;
            sh.scale     = new Vector3(aperturaW, aperturaH * 0.1f, 1f);

            ConfColorFade(psElectrico,
                new Color(1f, 1f, 1f, 1f),
                new Color(0f, 1f, 0.5f, 0.6f),
                new Color(0f, 0.5f, 1f, 0f));
            ConfRenderer(psElectrico, 14);
            psElectrico.Play();
        }

        // Destellos brillantes
        {
            GameObject obj = Hijo("Destellos");
            obj.transform.localPosition = new Vector3(0f, aperturaY, -0.09f);

            psDestello = obj.AddComponent<ParticleSystem>();
            var m = psDestello.main;
            m.startLifetime   = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            m.startSpeed      = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            m.startSize       = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
            m.startColor      = new ParticleSystem.MinMaxGradient(
                                    new Color(1f, 1f, 1f, 0.8f),
                                    new Color(0f, 1f, 0.6f, 0.5f));
            m.maxParticles    = 30;
            m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.loop = true;

            var em = psDestello.emission;   em.rateOverTime = 8f;
            var sh = psDestello.shape;
            sh.enabled   = true;
            sh.shapeType = ParticleSystemShapeType.Rectangle;
            sh.scale     = new Vector3(aperturaW * 0.6f, aperturaH * 0.6f, 1f);

            ConfColorFade(psDestello,
                new Color(1f, 1f, 1f, 0.8f),
                new Color(0f, 1f, 0.4f, 0.4f),
                new Color(0f, 0.3f, 1f, 0f));
            ConfRenderer(psDestello, 14);
            psDestello.Play();
        }
    }

    // ── Láseres de advertencia en los costados (vacío de la pantalla) ──────────
    private void CrearLaseresLaterales()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;
        Bounds b = sr.sprite.bounds;

        // Los costados del arco:
        float outerLeftX = b.min.x + 0.5f; // Un poco adentro para que parezca que sale del motor
        float outerRightX = b.max.x - 0.5f;
        float centerY = b.center.y;

        // Longitud masiva para salir de la pantalla
        float laserLength = 20f;

        for (int i = 0; i < 2; i++)
        {
            float startX = (i == 0) ? outerLeftX : outerRightX;
            float endX   = (i == 0) ? outerLeftX - laserLength : outerRightX + laserLength;

            GameObject obj = Hijo("LaserVacio_" + i);
            obj.transform.localPosition = new Vector3(0f, 0f, 0.5f); // Z atrás del arco

            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.useWorldSpace = false; // Espacio local respecto al arco
            
            lr.SetPosition(0, new Vector3(startX, centerY, 0));
            lr.SetPosition(1, new Vector3(endX, centerY, 0));

            // Un láser muy grueso para cubrir el alto del arco y justificar el hitbox mortal
            float grosorBase = b.size.y * 0.7f;
            lr.startWidth = grosorBase;
            lr.endWidth = grosorBase;

            // Material aditivo básico
            Shader sh = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Additive");
            if (sh != null) lr.material = new Material(sh);
            
            lr.material.mainTexture = Texture2D.whiteTexture; 
            
            lr.startColor = new Color(1f, 0f, 0.1f, 1f);
            lr.endColor = new Color(1f, 0.2f, 0.3f, 0.8f);
            
            lr.sortingOrder = -5; // Atrás de la nave y estructura
            
            laseres[i] = lr;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ANIMACIONES
    // ─────────────────────────────────────────────────────────────────────────
    private void Animar()
    {
        // Portal respira
        if (portalRenderer != null)
        {
            float alpha = Mathf.Lerp(0.10f, 0.28f, (Mathf.Sin(t * 3f) + 1f) * 0.5f);
            Color c = portalRenderer.color; c.a = alpha; portalRenderer.color = c;
            float sx = Mathf.Lerp(aperturaW * 0.96f, aperturaW * 1.04f, (Mathf.Sin(t * 2f) + 1f) * 0.5f);
            portalRenderer.transform.localScale = new Vector3(sx, aperturaH, 1f);
        }

        // Anillos se expanden y desvanecen
        float[] fases = { 0f, 0.5f };
        for (int i = 0; i < 2; i++)
        {
            if (anillos[i] == null) continue;
            float ciclo = (t * 0.7f + fases[i]) % 1f;
            float escala = Mathf.Lerp(0.5f, 1.4f, ciclo);
            float alpha  = Mathf.Lerp(0.6f, 0f, ciclo);
            anillos[i].transform.localScale = Vector3.one * escala;
            Color c = anillosRenderer[i].color; c.a = alpha; anillosRenderer[i].color = c;
        }

        // Flechas suben y bajan en cascada
        float[] xOff = { -aperturaW * 0.3f, 0f, aperturaW * 0.3f };
        float[] fOff = { 0f, 0.33f, 0.66f };
        for (int i = 0; i < 3; i++)
        {
            if (flechas[i] == null) continue;
            float bounce = Mathf.Sin(t * 2.5f + fOff[i] * Mathf.PI * 2f) * 0.10f;
            float alpha  = Mathf.Lerp(0.45f, 1f, (Mathf.Sin(t * 4f + fOff[i] * 8f) + 1f) * 0.5f);
            flechas[i].transform.localPosition = new Vector3(
                xOff[i], aperturaY - aperturaH * 0.5f - 0.3f + bounce, -0.1f);
            Color c = flechaRenderers[i].color; c.a = alpha; flechaRenderers[i].color = c;
        }

        // Animación intensa de los láseres laterales (pulso letal horizontal)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float baseWidth = sr != null ? sr.sprite.bounds.size.y * 0.7f : 1f;

        for (int i = 0; i < 2; i++)
        {
            if (laseres[i] == null) continue;
            
            // Grosor pulsante masivo
            float width = Mathf.Lerp(baseWidth * 0.85f, baseWidth * 1.15f, (Mathf.Sin(t * 18f) + 1f) * 0.5f);
            laseres[i].startWidth = width;
            laseres[i].endWidth = width;

            // Variación de color láser
            float alphaLaser = Mathf.Lerp(0.4f, 0.8f, (Mathf.Sin(t * 25f) + 1f) * 0.5f);
            laseres[i].startColor = new Color(1f, 0.1f, 0.1f, alphaLaser);
            laseres[i].endColor = new Color(1f, 0f, 0.4f, alphaLaser * 0.5f);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────────
    private GameObject Hijo(string nombre)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(transform, false);
        return obj;
    }

    private static Sprite TexQuad(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Color.white;
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f);
    }

    private static Sprite TexAnillo(int w, int h, int grosor)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        Color[] px = new Color[w * h];
        int cx = w / 2, cy = h / 2;
        float rE = w / 2f - 1f, rI = rE - grosor;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                px[y * w + x] = (d <= rE && d >= rI) ? Color.white : Color.clear;
            }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f);
    }

    private static Sprite TexChevron(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        int gr = 4;
        for (int x = 0; x < w / 2; x++)
        {
            int y = (int)((x / (float)(w / 2)) * (h - gr));
            for (int tt = 0; tt < gr; tt++)
            { int py = Mathf.Clamp(y + tt, 0, h - 1); px[py * w + x] = Color.white; }
        }
        for (int x = w / 2; x < w; x++)
        {
            int xR = w - 1 - x;
            int y = (int)((xR / (float)(w / 2)) * (h - gr));
            for (int tt = 0; tt < gr; tt++)
            { int py = Mathf.Clamp(y + tt, 0, h - 1); px[py * w + x] = Color.white; }
        }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f);
    }

    private static void ConfColorFade(ParticleSystem ps, Color c0, Color c1, Color c2)
    {
        var col = ps.colorOverLifetime; col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(c0, 0f), new GradientColorKey(c1, 0.5f), new GradientColorKey(c2, 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(c0.a, 0f), new GradientAlphaKey(c1.a, 0.5f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);
    }

    private static void ConfRenderer(ParticleSystem ps, int order)
    {
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard; r.sortingOrder = order;
        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive")
                 ?? Shader.Find("Particles/Additive") ?? Shader.Find("Sprites/Default");
        if (sh != null) r.material = new Material(sh);
    }
}
