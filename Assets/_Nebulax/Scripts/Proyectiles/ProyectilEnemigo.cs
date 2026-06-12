using UnityEngine;

/// <summary>
/// Proyectil enemigo: baja por la pantalla y daña al jugador al impactar.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProyectilEnemigo : MonoBehaviour
{
    [SerializeField] private float velocidad = 2.8f;
    [SerializeField] private int daño = 15;
    [SerializeField] private float limiteInferior = -7f;

    private void Update()
    {
        // Se mueve según su orientación local: con rotación identity (enemigos
        // normales) equivale a bajar recto; rotado (abanico del jefe) sigue su
        // ángulo de disparo.
        transform.Translate(Vector3.down * velocidad * Time.deltaTime, Space.Self);

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

        VidaNaveJugador vida = otro.GetComponent<VidaNaveJugador>();
        if (vida != null)
        {
            vida.RecibirDaño(daño);
        }

        PoolObjetos.Liberar(gameObject);
    }
}
