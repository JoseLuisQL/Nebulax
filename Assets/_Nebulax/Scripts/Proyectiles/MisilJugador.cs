using UnityEngine;

/// <summary>
/// Misil del jugador: avanza rápido hacia arriba y destruye al Enemigo III de un solo impacto.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))] 
public class MisilJugador : MonoBehaviour
{
    [SerializeField] private float velocidad = 7.5f;
    [SerializeField] private int daño = 999;
    [SerializeField] private float limiteSuperior = 7f;

    private void Update()
    {
        transform.Translate(Vector3.up * velocidad * Time.deltaTime, Space.World);

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
