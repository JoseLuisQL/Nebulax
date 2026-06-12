using UnityEngine;

/// <summary>
/// Proyectil normal del jugador: avanza hacia arriba, daña enemigos y se destruye al salir de pantalla.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProyectilJugador : MonoBehaviour
{
    [SerializeField] private float velocidad = 9f;
    [SerializeField] private int daño = 50;
    [SerializeField] private float limiteSuperior = 7f;

    private void Update()
    {
        // Mover en la dirección local (respeta el ángulo del triple disparo en abanico)
        transform.Translate(Vector3.up * velocidad * Time.deltaTime, Space.Self);

        if (transform.position.y > limiteSuperior)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (!otro.CompareTag("Enemy"))
        {
            return;
        }

        EnemigoBase enemigo = otro.GetComponent<EnemigoBase>();
        if (enemigo != null)
        {
            enemigo.RecibirDaño(daño);
        }

        Destroy(gameObject);
    }
}
