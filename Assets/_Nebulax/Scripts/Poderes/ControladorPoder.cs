using UnityEngine;

/// <summary>
/// Power-up que cae por la pantalla y activa doble o triple disparo al ser recogido.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ControladorPoder : MonoBehaviour
{
    public enum TipoPoder
    {
        DobleDisparo,
        TripleDisparo
    }

    [SerializeField] private TipoPoder tipoPoder = TipoPoder.DobleDisparo;
    [SerializeField] private float velocidadCaida = 2.2f;
    [SerializeField] private float limiteInferior = -7f;

    private void Update()
    {
        transform.Translate(Vector3.down * velocidadCaida * Time.deltaTime, Space.World);

        if (transform.position.y < limiteInferior)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (!otro.CompareTag("Player"))
        {
            return;
        }

        DisparoNaveJugador disparo = otro.GetComponent<DisparoNaveJugador>();
        if (disparo != null)
        {
            if (tipoPoder == TipoPoder.DobleDisparo)
            {
                disparo.ActivarDobleDisparo();
            }
            else
            {
                disparo.ActivarTripleDisparo();
            }
        }

        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirPoder();
        }

        Destroy(gameObject);
    }
}
