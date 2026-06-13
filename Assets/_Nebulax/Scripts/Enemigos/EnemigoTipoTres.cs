using UnityEngine;

/// <summary>
/// Enemigo élite que aparece en pares, se desplaza desde el centro superior hacia los bordes y activa alerta.
/// </summary>
public class EnemigoTipoTres : EnemigoBase
{
    [SerializeField] private float direccionHorizontal = 1f;
    [SerializeField] private float velocidadHorizontal = 0.5f;

    public void ConfigurarDireccionHorizontal(float nuevaDireccion)
    {
        direccionHorizontal = Mathf.Sign(nuevaDireccion == 0f ? 1f : nuevaDireccion);
    }

    protected override void Awake()
    {
        base.Awake();
        explosionFuerte = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.ConfigurarAlertaEnemigoIII(true);
        }
    }

    protected override void MoverEnemigo()
    {
        Vector3 movimiento = new Vector3(direccionHorizontal * velocidadHorizontal, -VelocidadMovimiento) * Time.deltaTime;
        transform.Translate(movimiento, Space.World);
    }

    private void OnDestroy()
    {
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.ConfigurarAlertaEnemigoIII(false);
        }
    }

    protected override void DispararProyectiles(Transform origen)
    {
        if (PrefabProyectilEnemigo == null) return;
        
        Vector3 offsetIzq = new Vector3(-0.45f, 0f, 0f);
        Vector3 offsetDer = new Vector3(0.45f, 0f, 0f);
        
        PoolObjetos.Crear(PrefabProyectilEnemigo, origen.position + offsetIzq, Quaternion.identity);
        PoolObjetos.Crear(PrefabProyectilEnemigo, origen.position, Quaternion.identity);
        PoolObjetos.Crear(PrefabProyectilEnemigo, origen.position + offsetDer, Quaternion.identity);
    }
}
