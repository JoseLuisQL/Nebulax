using UnityEngine;

/// <summary>
/// Administra la vida de la nave del jugador y notifica la derrota al gestor del juego.
/// </summary>
public class VidaNaveJugador : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 100;
    [SerializeField] private int vidaActual = 100;

    private bool estaMuerta;
    private bool escudoActivo;
    private GameObject escudoVisual;
    private Coroutine rutinaEscudo;

    public int VidaActual => vidaActual;
    public int PorcentajeVida => vidaMaxima <= 0 ? 0 : Mathf.RoundToInt((vidaActual / (float)vidaMaxima) * 100f);

    private void Awake()
    {
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);
    }

    public void RecibirDaño(int cantidadDaño)
    {
        if (estaMuerta || cantidadDaño <= 0)
        {
            return;
        }

        if (escudoActivo)
        {
            // El escudo absorbe todo el daño
            if (escudoVisual != null)
            {
                // Un pequeño parpadeo para indicar impacto
                SpriteRenderer sr = escudoVisual.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.9f);
            }
            return;
        }

        vidaActual = Mathf.Max(0, vidaActual - cantidadDaño);

        if (CamaraShake.Instancia != null)
        {
            CamaraShake.Instancia.Sacudir(0.25f, 0.15f);
        }

        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.ActualizarVidaJugador(PorcentajeVida);
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    public void MorirInstantaneamente()
    {
        if (estaMuerta)
        {
            return;
        }

        vidaActual = 0;
        Morir();
    }

    private void Morir()
    {
        estaMuerta = true;

        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.ActualizarVidaJugador(0);
            GestorJuego.Instancia.RegistrarJugadorMuerto(transform.position);
        }

        gameObject.SetActive(false);
    }

    // ── MECÁNICA DE ESCUDO PROTECTOR ─────────────────────────────────────────

    public void ActivarEscudo(float duracion)
    {
        if (rutinaEscudo != null)
        {
            StopCoroutine(rutinaEscudo);
        }
        rutinaEscudo = StartCoroutine(RutinaEscudo(duracion));
    }

    private System.Collections.IEnumerator RutinaEscudo(float duracion)
    {
        escudoActivo = true;

        if (escudoContenedor == null)
        {
            CrearEscudoVisual();
        }
        
        escudoContenedor.SetActive(true);
        SpriteRenderer sr = escudoContenedor.GetComponentInChildren<SpriteRenderer>();

        float tiempoRestante = duracion;
        while (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            
            if (textoContadorEscudo != null)
            {
                textoContadorEscudo.text = Mathf.CeilToInt(tiempoRestante).ToString() + "s";
            }

            // Animación de respiración y recuperación de color del impacto
            if (sr != null)
            {
                float baseAlpha = Mathf.Lerp(0.6f, 0.9f, (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f);
                sr.color = Color.Lerp(sr.color, new Color(1f, 1f, 1f, baseAlpha), Time.deltaTime * 5f);
                
                // Parpadeo final cuando se está acabando
                if (tiempoRestante < 2f)
                {
                    sr.enabled = (Mathf.Repeat(Time.time * 10f, 1f) > 0.5f);
                }
            }

            yield return null;
        }

        escudoActivo = false;
        if (sr != null) sr.enabled = true;
        if (escudoContenedor != null) escudoContenedor.SetActive(false);
    }

    private GameObject escudoContenedor;
    private TextMesh textoContadorEscudo;

    private void CrearEscudoVisual()
    {
        escudoContenedor = new GameObject("ContenedorEscudo");
        escudoContenedor.transform.SetParent(transform, false);
        escudoContenedor.transform.localPosition = Vector3.zero;

        // Gráfico del escudo que rota
        GameObject graficoEscudo = new GameObject("GraficoEscudo");
        graficoEscudo.transform.SetParent(escudoContenedor.transform, false);
        graficoEscudo.transform.localPosition = Vector3.zero;
        
        SpriteRenderer sr = graficoEscudo.AddComponent<SpriteRenderer>();
        
        // Intentar cargar la textura profesional
        Sprite spriteBurbuja = Resources.Load<Sprite>("EscudoBurbuja");
        if (spriteBurbuja != null)
        {
            sr.sprite = spriteBurbuja;
            graficoEscudo.transform.localScale = Vector3.one * 0.45f; // Ajustado para cubrir la nave
            sr.color = new Color(1f, 1f, 1f, 0.85f); // Color natural de la textura
        }
        else
        {
            sr.sprite = GenerarTexturaCirculo(128);
            graficoEscudo.transform.localScale = Vector3.one * 1.5f;
            sr.color = new Color(0f, 0.8f, 1f, 0.5f); // Cian transparente fallback
        }

        sr.sortingOrder = 15;

        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Additive");
        if (sh != null) sr.material = new Material(sh);
        
        // Agregar rotación lenta solo al gráfico
        RotadorVisual rotador = graficoEscudo.AddComponent<RotadorVisual>();

        // Texto del contador
        GameObject objTexto = new GameObject("TextoContador");
        objTexto.transform.SetParent(escudoContenedor.transform, false);
        objTexto.transform.localPosition = new Vector3(0, 1.2f, 0); // Arriba de la nave
        textoContadorEscudo = objTexto.AddComponent<TextMesh>();
        textoContadorEscudo.characterSize = 0.1f;
        textoContadorEscudo.fontSize = 60;
        textoContadorEscudo.anchor = TextAnchor.MiddleCenter;
        textoContadorEscudo.alignment = TextAlignment.Center;
        textoContadorEscudo.color = Color.cyan;
        textoContadorEscudo.fontStyle = FontStyle.Bold;
    }

    private Sprite GenerarTexturaCirculo(int tam)
    {
        Texture2D tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        Color[] px = new Color[tam * tam];
        float radioMax = tam / 2f;
        float center = tam / 2f;

        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                // Círculo difuminado tipo campo de fuerza
                float alpha = 1f - Mathf.Clamp01(dist / radioMax);
                alpha = Mathf.Pow(alpha, 1.5f); // Curva para que el borde sea suave
                
                // Hacer el borde más brillante (efecto burbuja)
                if (dist > radioMax * 0.7f && dist < radioMax)
                {
                    alpha += Mathf.Sin((dist - radioMax * 0.7f) / (radioMax * 0.3f) * Mathf.PI) * 0.5f;
                }

                if (dist > radioMax) alpha = 0f;

                px[y * tam + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tam, tam), new Vector2(0.5f, 0.5f));
    }
}

// Rotador para darle vida al escudo
public class RotadorVisual : MonoBehaviour
{
    public float velocidad = 30f;
    private void Update()
    {
        transform.Rotate(0, 0, velocidad * Time.deltaTime);
    }
}
