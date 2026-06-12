using UnityEngine;

/// <summary>
/// Enemigo mediano con movimiento zigzag horizontal mientras desciende.
/// </summary>
public class EnemigoTipoDos : EnemigoBase
{
    [SerializeField] private float amplitudZigzag = 1.6f;
    [SerializeField] private float frecuenciaZigzag = 3f;

    protected override void MoverEnemigo()
    {
        float desplazamientoX = Mathf.Sin(Time.time * frecuenciaZigzag) * amplitudZigzag * Time.deltaTime;
        Vector3 movimiento = new Vector3(desplazamientoX, -VelocidadMovimiento * Time.deltaTime, 0f);
        transform.Translate(movimiento, Space.World);
    }

    protected override void DispararProyectiles(Transform origen)
    {
        if (PrefabProyectilEnemigo == null) return;
        
        Vector3 offsetIzq = new Vector3(-0.35f, 0f, 0f);
        Vector3 offsetDer = new Vector3(0.35f, 0f, 0f);
        
        Instantiate(PrefabProyectilEnemigo, origen.position + offsetIzq, Quaternion.identity);
        Instantiate(PrefabProyectilEnemigo, origen.position + offsetDer, Quaternion.identity);
    }
}
