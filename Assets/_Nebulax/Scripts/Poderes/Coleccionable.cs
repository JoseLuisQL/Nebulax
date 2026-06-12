using UnityEngine;

/// <summary>
/// Item coleccionable de Nebulax. Cae por la pantalla y, al ser recogido por la
/// nave del jugador, notifica al <see cref="GestorJuego"/> (que lo registra en
/// su Debug.Log y alimenta la progresión) y reproduce un efecto de sonido.
///
/// Se instancia desde un Prefab y se reutiliza mediante el pool de objetos.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Coleccionable : MonoBehaviour
{
    public enum TipoColeccionable
    {
        Cristal,
        NucleoEnergia
    }

    [SerializeField] private TipoColeccionable tipo = TipoColeccionable.Cristal;
    [SerializeField] private float velocidadCaida = 2.0f;
    [SerializeField] private float velocidadGiro = 90f;
    [SerializeField] private float limiteInferior = -7f;

    public TipoColeccionable Tipo => tipo;

    private void Update()
    {
        if (GestorJuego.Instancia != null && GestorJuego.Instancia.JuegoTerminado)
        {
            return;
        }

        transform.Translate(Vector3.down * velocidadCaida * Time.deltaTime, Space.World);
        // Giro visual sobre su eje para dar vida al item (animación por código).
        transform.Rotate(0f, 0f, velocidadGiro * Time.deltaTime, Space.Self);

        if (transform.position.y < limiteInferior)
        {
            PoolObjetos.Liberar(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (!otro.CompareTag("Player"))
        {
            return;
        }

        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.RegistrarItemRecolectado(tipo);
        }

        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirItemRecolectado();
        }

        PoolObjetos.Liberar(gameObject);
    }
}
