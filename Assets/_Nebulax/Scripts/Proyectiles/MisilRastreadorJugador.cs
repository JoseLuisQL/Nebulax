using UnityEngine;

/// <summary>
/// Misil que busca automáticamente a un objetivo enemigo.
/// Instanciado por el Enjambre de Misiles.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MisilRastreadorJugador : MonoBehaviour
{
    [SerializeField] private float velocidadVel = 12f;
    [SerializeField] private float velocidadGiro = 250f;
    [SerializeField] private float tiempoVidaMaximo = 5f;

    private Transform objetivo;
    private float tiempoNacido;

    public void AsignarObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
    }

    private void OnEnable()
    {
        tiempoNacido = Time.time;
    }

    private void Update()
    {
        if (Time.time > tiempoNacido + tiempoVidaMaximo)
        {
            // Opcional: Instanciar explosion
            PoolObjetos.Liberar(gameObject);
            return;
        }

        if (objetivo != null && objetivo.gameObject.activeInHierarchy)
        {
            // Calcular dirección hacia el objetivo
            Vector2 direccionObjetivo = (Vector2)objetivo.position - (Vector2)transform.position;
            direccionObjetivo.Normalize();

            // Rotar suavemente hacia el objetivo
            float anguloRotacion = Vector3.Cross(transform.up, direccionObjetivo).z;
            transform.Rotate(0, 0, anguloRotacion * velocidadGiro * Time.deltaTime);
        }

        // Mover siempre hacia el frente local (hacia donde mira)
        transform.Translate(Vector3.up * velocidadVel * Time.deltaTime, Space.Self);
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
            // Misil letal de enjambre (mata o hace muchísimo daño)
            enemigo.RecibirDañoLetal();
        }

        // Podríamos generar explosión aquí (opcional)
        PoolObjetos.Liberar(gameObject);
    }
}
