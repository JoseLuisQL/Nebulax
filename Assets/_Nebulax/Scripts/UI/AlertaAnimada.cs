using UnityEngine;
using UnityEngine.UI;

public class AlertaAnimada : MonoBehaviour
{
    private Image bg;
    private float tiempo;

    void Awake()
    {
        bg = GetComponent<Image>();
    }

    void Update()
    {
        tiempo += Time.deltaTime * 6f; 
        if (bg != null)
        {
            Color c = bg.color;
            // Oscila el alpha entre 0.3 y 0.8
            c.a = 0.55f + 0.25f * Mathf.Sin(tiempo); 
            bg.color = c;
        }
    }
}
