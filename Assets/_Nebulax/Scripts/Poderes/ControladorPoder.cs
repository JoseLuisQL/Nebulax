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
        TripleDisparo,
        Escudo,
        EnjambreMisiles
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
        VidaNaveJugador vida = otro.GetComponent<VidaNaveJugador>();
        
        if (tipoPoder == TipoPoder.DobleDisparo && disparo != null)
        {
            disparo.ActivarDobleDisparo();
        }
        else if (tipoPoder == TipoPoder.TripleDisparo && disparo != null)
        {
            disparo.ActivarTripleDisparo();
        }
        else if (tipoPoder == TipoPoder.Escudo && vida != null)
        {
            vida.ActivarEscudo(8f);
        }
        else if (tipoPoder == TipoPoder.EnjambreMisiles && disparo != null)
        {
            disparo.ActivarEnjambreMisiles();
        }

        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirPoder();
        }

        Destroy(gameObject);
    }
}
