using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Pruebas de la lógica de vida de la nave del jugador (sin física ni render).
/// </summary>
public class VidaNaveJugadorTests
{
    private GameObject go;
    private VidaNaveJugador vida;

    [SetUp]
    public void Preparar()
    {
        go = new GameObject("JugadorTest");
        vida = go.AddComponent<VidaNaveJugador>();
    }

    [TearDown]
    public void Limpiar()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void VidaInicial_EsCien()
    {
        Assert.AreEqual(100, vida.VidaActual);
        Assert.AreEqual(100, vida.PorcentajeVida);
    }

    [Test]
    public void RecibirDano_RestaLaVida()
    {
        vida.RecibirDaño(30);
        Assert.AreEqual(70, vida.VidaActual);
        Assert.AreEqual(70, vida.PorcentajeVida);
    }

    [Test]
    public void RecibirDano_NoBajaDeCero()
    {
        vida.RecibirDaño(500);
        Assert.AreEqual(0, vida.VidaActual);
        Assert.AreEqual(0, vida.PorcentajeVida);
    }

    [Test]
    public void RecibirDano_IgnoraValoresNoPositivos()
    {
        vida.RecibirDaño(0);
        vida.RecibirDaño(-50);
        Assert.AreEqual(100, vida.VidaActual);
    }

    [Test]
    public void RecibirDano_TrasMuerteNoSigueRestando()
    {
        vida.MorirInstantaneamente();
        Assert.AreEqual(0, vida.VidaActual);

        // Ya muerta: cualquier daño posterior no debe alterar el valor.
        vida.RecibirDaño(10);
        Assert.AreEqual(0, vida.VidaActual);
    }

    [Test]
    public void MorirInstantaneamente_DejaVidaEnCero()
    {
        vida.MorirInstantaneamente();
        Assert.AreEqual(0, vida.VidaActual);
        Assert.AreEqual(0, vida.PorcentajeVida);
    }
}
