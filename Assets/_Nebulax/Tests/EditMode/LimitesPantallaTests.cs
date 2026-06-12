using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Pruebas del recorte de posición a los límites visibles de la cámara.
/// </summary>
public class LimitesPantallaTests
{
    private GameObject camGo;
    private Camera camara;
    private GameObject go;
    private LimitesPantalla limites;

    [SetUp]
    public void Preparar()
    {
        camGo = new GameObject("CamaraTest");
        camara = camGo.AddComponent<Camera>();
        camara.orthographic = true;
        camara.orthographicSize = 5f;
        camara.tag = "MainCamera"; // para que Camera.main lo resuelva

        go = new GameObject("LimitesTest");
        limites = go.AddComponent<LimitesPantalla>();
    }

    [TearDown]
    public void Limpiar()
    {
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(camGo);
    }

    [Test]
    public void LimitarPosicion_DentroDePantalla_NoSeModifica()
    {
        Vector3 dentro = new Vector3(0f, 0f, 0f);
        Vector3 resultado = limites.LimitarPosicion(dentro);
        Assert.AreEqual(dentro, resultado);
    }

    [Test]
    public void LimitarPosicion_FueraPorArriba_SeRecorta()
    {
        // Altura ortográfica = 5, así que Y nunca debe superar ~5.
        Vector3 fuera = new Vector3(0f, 100f, 0f);
        Vector3 resultado = limites.LimitarPosicion(fuera);
        Assert.Less(resultado.y, 5f);
    }

    [Test]
    public void LimitarPosicion_FueraPorLosLados_SeRecorta()
    {
        float anchoMaximo = camara.orthographicSize * camara.aspect;

        Vector3 derecha = limites.LimitarPosicion(new Vector3(1000f, 0f, 0f));
        Assert.LessOrEqual(derecha.x, anchoMaximo);

        Vector3 izquierda = limites.LimitarPosicion(new Vector3(-1000f, 0f, 0f));
        Assert.GreaterOrEqual(izquierda.x, -anchoMaximo);
    }
}
