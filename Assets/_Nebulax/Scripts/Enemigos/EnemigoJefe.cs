using UnityEngine;

/// <summary>
/// Enemigo JEFE de Nebulax. Hereda de <see cref="EnemigoBase"/> (vida, disparo,
/// daño por contacto, eventos y, por tanto, el AnimadorEnemigo por eventos) y
/// añade:
///  - Vida alta y FASES según el porcentaje de vida (cambian el patrón de
///    disparo y la velocidad de vaivén).
///  - Movimiento de ENTRADA desde fuera de pantalla y luego VAIVÉN horizontal.
///  - Animación procedural: flotación vertical e impulso visual al cambiar de
///    fase.
///  - Al morir, notifica la VICTORIA al GestorJuego.
///
/// La animación reacciona a los eventos heredados (disparo, daño) gracias a
/// AnimadorEnemigo; aquí se suma la animación propia de jefe (flotación y fases).
/// </summary>
public class EnemigoJefe : EnemigoBase
{
    [Header("Jefe: posición y movimiento")]
    [SerializeField] private float alturaObjetivo = 3.2f;
    [SerializeField] private float velocidadEntrada = 2.0f;
    [SerializeField] private float amplitudVaiven = 3.5f;
    [SerializeField] private float velocidadVaivenBase = 1.2f;

    [Header("Jefe: animación")]
    [SerializeField] private float amplitudFlotacion = 0.18f;
    [SerializeField] private float frecuenciaFlotacion = 2f;

    [Header("Jefe: fases (umbrales de % de vida)")]
    [SerializeField] private float umbralFase2 = 0.66f;
    [SerializeField] private float umbralFase3 = 0.33f;

    private bool entrando = true;
    private float centroX;
    private float tiempoVaiven;
    private float tiempoFlotacion;
    private int faseActual = 1;
    private float impulsoFase; // animación al cambiar de fase

    // El jefe usa su propio factor de vida por nivel (más alto que el de los
    // enemigos normales). En el Nivel 1 vale 1.0 -> vida base sin cambios.
    protected override float FactorVidaNivel => ConfiguracionNivel.FactorVidaJefe;

    protected override void Awake()
    {
        base.Awake();
        explosionFuerte = true;
        centroX = transform.position.x;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        entrando = true;
        faseActual = 1;
        tiempoVaiven = 0f;
        tiempoFlotacion = 0f;
        impulsoFase = 0f;

        AlMorir += NotificarVictoria;

        // Barra de vida profesional del jefe.
        BarraVidaJefe.Mostrar(this, "DEVASTADOR · ENEMIGO JEFE");

        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.ConfigurarAlertaEnemigoIII(true);
        }
    }

    protected void OnDisable()
    {
        AlMorir -= NotificarVictoria;
        BarraVidaJefe.Ocultar();
    }

    private void NotificarVictoria()
    {
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.RegistrarVictoria(transform.position);
        }
    }

    protected override void MoverEnemigo()
    {
        ActualizarFase();

        if (entrando)
        {
            // Desciende hasta su altura de combate.
            transform.Translate(Vector3.down * velocidadEntrada * Time.deltaTime, Space.World);
            if (transform.position.y <= alturaObjetivo)
            {
                entrando = false;
            }
            return;
        }

        // Vaivén horizontal (más rápido en fases avanzadas).
        float velVaiven = velocidadVaivenBase * (1f + (faseActual - 1) * 0.4f);
        tiempoVaiven += Time.deltaTime * velVaiven;
        tiempoFlotacion += Time.deltaTime * frecuenciaFlotacion;

        float x = centroX + Mathf.Sin(tiempoVaiven) * amplitudVaiven;
        float y = alturaObjetivo + Mathf.Sin(tiempoFlotacion) * amplitudFlotacion;

        // Impulso visual decreciente al entrar en una nueva fase.
        if (impulsoFase > 0f)
        {
            impulsoFase -= Time.deltaTime;
            y += Mathf.Max(0f, impulsoFase) * 0.5f;
        }

        transform.position = new Vector3(x, y, transform.position.z);
    }

    private void ActualizarFase()
    {
        int nuevaFase = 1;
        if (PorcentajeVida <= umbralFase3)
        {
            nuevaFase = 3;
        }
        else if (PorcentajeVida <= umbralFase2)
        {
            nuevaFase = 2;
        }

        if (nuevaFase != faseActual)
        {
            faseActual = nuevaFase;
            impulsoFase = 0.4f; // dispara la animación de cambio de fase

            if (GestorAudio.Instancia != null)
            {
                GestorAudio.Instancia.ReproducirImpactoEnemigo();
            }

            Debug.Log("[Jefe] Cambio a fase " + faseActual + " (vida " + Mathf.RoundToInt(PorcentajeVida * 100f) + "%).");
        }
    }

    protected override void DispararProyectiles(Transform origen)
    {
        if (PrefabProyectilEnemigo == null)
        {
            return;
        }

        // El patrón de disparo se intensifica con la fase. En el Nivel 2 el jefe
        // es más agresivo: abanicos más densos y amplios en cada fase.
        bool agresivo = ConfiguracionNivel.JefeAgresivo;
        switch (faseActual)
        {
            case 1:
                if (agresivo) DispararAbanico(origen, 3, 12f);
                else DispararAbanico(origen, 1, 0f);
                break;
            case 2:
                if (agresivo) DispararAbanico(origen, 5, 14f);
                else DispararAbanico(origen, 3, 12f);
                break;
            default:
                if (agresivo) DispararAbanico(origen, 7, 16f);
                else DispararAbanico(origen, 5, 14f);
                break;
        }
    }

    private void DispararAbanico(Transform origen, int cantidad, float separacionGrados)
    {
        float inicio = -separacionGrados * (cantidad - 1) * 0.5f;
        for (int i = 0; i < cantidad; i++)
        {
            float angulo = inicio + separacionGrados * i;
            Quaternion rot = Quaternion.Euler(0f, 0f, angulo);
            PoolObjetos.Crear(PrefabProyectilEnemigo, origen.position, rot);
        }
    }

    private void OnDestroy()
    {
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.ConfigurarAlertaEnemigoIII(false);
        }
    }
}
