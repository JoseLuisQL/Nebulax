using UnityEngine;

/// <summary>
/// Reproduce una explosión de 8 frames y destruye el objeto al finalizar.
/// </summary>
public class ControladorExplosion : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] framesExplosion;
    [SerializeField] private float cuadrosPorSegundo = 14f;
    [SerializeField] private bool destruirAlFinalizar = true;

    private int indiceFrame;
    private float acumulador;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        indiceFrame = 0;
        acumulador = 0f;
        AplicarFrameActual();
    }

    private void Update()
    {
        if (framesExplosion == null || framesExplosion.Length == 0 || cuadrosPorSegundo <= 0f)
        {
            return;
        }

        acumulador += Time.unscaledDeltaTime;
        float duracionFrame = 1f / cuadrosPorSegundo;

        while (acumulador >= duracionFrame)
        {
            acumulador -= duracionFrame;
            indiceFrame++;

            if (indiceFrame >= framesExplosion.Length)
            {
                if (destruirAlFinalizar)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
                return;
            }

            AplicarFrameActual();
        }
    }

    private void AplicarFrameActual()
    {
        if (spriteRenderer != null && framesExplosion != null && framesExplosion.Length > 0)
        {
            spriteRenderer.sprite = framesExplosion[Mathf.Clamp(indiceFrame, 0, framesExplosion.Length - 1)];
        }
    }
}
