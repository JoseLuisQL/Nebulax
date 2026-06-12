using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Actualiza la interfaz mínima del juego: vida, contador, alerta y Game Over.
/// </summary>
public class GestorUI : MonoBehaviour
{
    [SerializeField] private Text textoVidaJugador;
    [SerializeField] private Text textoEnemigosDestruidos;
    [SerializeField] private Text textoAlertaEnemigoIII;
    [SerializeField] private GameObject imagenGameOver;
    [SerializeField] private Image imagenRellenoVida;
    [SerializeField] private GameObject hudJuego;

    [SerializeField] private Text textoGameOverEnemigos;

    private void Awake()
    {
        // Encontrar el HUD por si la serialización falló debido a la compilación
        if (hudJuego == null)
        {
            // Busca en toda la jerarquía incluso inactivos
            Transform[] trs = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in trs)
            {
                if (t.name == "HUDJuego")
                {
                    hudJuego = t.gameObject;
                    break;
                }
            }
        }

        MostrarAlertaEnemigoIII(false);
        if (imagenGameOver != null) imagenGameOver.SetActive(false);
        
        ActualizarVida(100);
        ActualizarEnemigosDestruidos(0);
    }

    public void ActualizarVida(int porcentajeVida)
    {
        float pct = Mathf.Clamp(porcentajeVida, 0, 100);
        if (textoVidaJugador != null)
        {
            textoVidaJugador.supportRichText = true;
            textoVidaJugador.text = pct + "%";
        }
        if (imagenRellenoVida != null)
        {
            imagenRellenoVida.fillAmount = pct / 100f;
        }
    }

    public void ActualizarEnemigosDestruidos(int cantidad)
    {
        if (textoEnemigosDestruidos != null)
        {
            textoEnemigosDestruidos.text = Mathf.Max(0, cantidad).ToString();
        }
    }

    public void MostrarAlertaEnemigoIII(bool visible)
    {
        if (textoAlertaEnemigoIII != null)
        {
            Transform banner = textoAlertaEnemigoIII.transform.parent;
            if (banner != null && banner.name == "AlertaBanner")
            {
                banner.gameObject.SetActive(visible);
            }
            else
            {
                textoAlertaEnemigoIII.gameObject.SetActive(visible);
            }

            if (visible)
            {
                textoAlertaEnemigoIII.text = "<color=#ff3333>¡ A L E R T A !</color>\n<size=24><color=#ffffff>ANOMALÍA CLASE III DETECTADA</color></size>";
            }
        }
    }

    public void MostrarGameOver(bool visible)
    {
        if (imagenGameOver != null)
        {
            imagenGameOver.SetActive(visible);
            
            // Actualizar el texto final de enemigos eliminados
            if (visible && textoGameOverEnemigos != null)
            {
                int enemigos = 0;
                if (textoEnemigosDestruidos != null && int.TryParse(textoEnemigosDestruidos.text, out int cant))
                {
                    enemigos = cant;
                }
                textoGameOverEnemigos.text = "ENEMIGOS ELIMINADOS:\n<color=#ffcc00>" + enemigos + "</color>";
            }
        }
        if (hudJuego != null)
        {
            hudJuego.SetActive(!visible);
        }
    }
}
