using UnityEngine;
using System.Collections;

/// <summary>
/// Script para gestionar el temblor de cámara (Screen Shake) al recibir daño.
/// </summary>
public class CamaraShake : MonoBehaviour
{
    public static CamaraShake Instancia { get; private set; }

    private Vector3 posicionOriginal;
    private Coroutine rutinaSacudida;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        posicionOriginal = transform.position;
    }

    public void Sacudir(float duracion, float magnitud)
    {
        if (rutinaSacudida != null)
        {
            StopCoroutine(rutinaSacudida);
        }
        rutinaSacudida = StartCoroutine(RutinaSacudir(duracion, magnitud));
    }

    private IEnumerator RutinaSacudir(float duracion, float magnitud)
    {
        float tiempoTranscurrido = 0.0f;

        while (tiempoTranscurrido < duracion)
        {
            float x = posicionOriginal.x + Random.Range(-1f, 1f) * magnitud;
            float y = posicionOriginal.y + Random.Range(-1f, 1f) * magnitud;

            transform.position = new Vector3(x, y, posicionOriginal.z);

            // Usamos unscaledDeltaTime para que tiemble incluso si el juego está pausado (TimeScale = 0)
            tiempoTranscurrido += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.position = posicionOriginal;
    }
}
