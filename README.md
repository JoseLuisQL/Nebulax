# Nebulax

**Nebulax** es un *shoot 'em up* espacial vertical en 2D: una nave atraviesa una
zona de guerra cósmica, destruye enemigos, recoge poderes y debe superar un
**área de batalla** con una abertura central segura.

> *Sobrevive a la nebulosa, domina el disparo y cruza la estructura antes de ser
> destruido.*

Proyecto académico — Universidad Continental, Desarrollo de Videojuegos.

---

## Tecnología

- **Unity 6** (`6000.4.9f1`)
- **Universal Render Pipeline (URP)** 17.4
- **Input System** (paquete nuevo)
- **TextMeshPro** (incluido en `com.unity.ugui` 2.0)
- Arquitectura clásica **MonoBehaviour + Prefabs**, física 2D
  (`Rigidbody2D`, `Collider2D`)

## Cómo abrir el proyecto

1. Instalar **Unity 6 (6000.4.9f1)** mediante Unity Hub.
2. Abrir la carpeta del repositorio como proyecto.
3. (Primera vez) Si se desea usar TextMeshPro en el HUD: menú
   **Window → TextMeshPro → Import TMP Essential Resources**.
4. Abrir la escena principal:
   `Assets/_Nebulax/Escenas/EscenaPrincipal.unity`.
5. Pulsar **Play**.

## Controles

| Acción              | Tecla                          |
|---------------------|--------------------------------|
| Mover               | `WASD` o flechas               |
| Disparo normal      | `Espacio`                      |
| Doble disparo       | `N` (tras recoger el poder)    |
| Misil               | `Ctrl` izquierdo               |

- El **triple disparo** se activa al recoger su poder y se aplica al disparo
  con `Espacio`.
- El **misil** destruye a cualquier enemigo (incluido el élite Tipo III) de un
  solo impacto.

## Mecánicas

- **Poderes**: doble y triple disparo, temporales, soltados por enemigos.
- **Enemigos**:
  - **Tipo I** — desciende en línea recta.
  - **Tipo II** — zigzag horizontal; aparece en oleada tras 3 bajas.
  - **Tipo III** — élite en pares desde el centro hacia los bordes, con alerta
    sonora y visual.
- **Progresión**: a las **3** bajas aparece una oleada de Tipo II; a las **10**
  se limpia la pantalla, aparece una oleada de Tipo III (3 s) y finalmente la
  **estructura del área de batalla**, que hay que cruzar por su abertura central.

## Funcionalidades avanzadas

- **Animaciones de enemigos ligadas a eventos + audio** — cada enemigo reacciona
  a sus eventos mediante `AnimadorEnemigo` con animación profesional: idle/hover,
  **banking** (inclinación al virar), **kickback** y destello al disparar, y
  **shake** + flash al recibir daño. Se complementa con SFX de disparo, **impacto
  por colisión**, destrucción y música de fondo en bucle.
- **Items coleccionables realistas** — `Coleccionable` (Prefab) con textura de
  **gema facetada**, efectos (`EfectoColeccionable`: halo pulsante, chispas,
  estela, destello), física y animación (pop de aparición, giro, flotación e
  **imán magnético** hacia la nave). Al recogerlo, el `GestorJuego` lo registra
  en `Debug.Log` y alimenta la progresión. Los genera `GeneradorItems`.
- **Pantalla "Misión Cumplida"** — al derrotar al jefe aparece una pantalla
  profesional (`PanelMisionCumplida`) con banner dorado, resumen, **jingle de
  victoria** sintetizado y botón **"Siguiente Nivel"** que carga la 2ª escena.
- **Progresión: habilidades y dificultad** — cada **5 items** sube 1 nivel
  (`GestorProgresion`): la nave gana **velocidad** y **cadencia**, y los enemigos
  se vuelven más rápidos/frecuentes.
- **Enemigo jefe animado** — `EnemigoJefe` con vida alta, **fases** (cambian el
  patrón de disparo en abanico y la velocidad), entrada + vaivén, flotación y
  victoria al ser derrotado.
- **2ª escena con Tilemaps** — `EscenaNivel2` con un `Grid` y **tres Tilemaps**
  (fondo, muros con collider, decoración), tiles, material y texturas generados
  por código.

### Construir los assets nuevos (en Unity)

Los assets de Unity se generan desde el menú **Nebulax/**:

1. `Nebulax/Funcionalidades/Construir items, jefe y animadores` — crea prefabs de
   coleccionables y del jefe, los AnimatorControllers, añade `AnimadorEnemigo` a
   los enemigos e integra `GeneradorItems`, `GestorProgresion` y el cableado del
   jefe en `EscenaPrincipal`.
2. `Nebulax/Funcionalidades/Construir 2da escena (Tilemaps)` — crea tiles,
   material/texturas y `EscenaNivel2` con sus Tilemaps, y la registra en Build
   Settings.

## Estructura del proyecto

```
Assets/_Nebulax/
├── Arte/            Sprites, audio, fondos, UI
├── Escenas/         EscenaPrincipal.unity
├── Prefabs/         Nave, enemigos, proyectiles, poderes, gestores, área
├── Scripts/
│   ├── AreaBatalla/ Estructura final y detección de paso
│   ├── Enemigos/    EnemigoBase + tipos I/II/III + generador
│   ├── Gestores/    GestorJuego, GestorUI, GestorAudio, CamaraShake, menú
│   ├── Jugador/     Movimiento, disparo, vida, efectos
│   ├── Poderes/     Power-ups
│   ├── Proyectiles/ Proyectiles del jugador, misil y enemigos
│   ├── UI/          Botones y alertas animadas
│   └── Utilidades/  Límites de pantalla, explosiones, pool de objetos
│       └── Editor/  NebulaxConstructorEditor (construcción automática)
├── Tests/EditMode/  Pruebas unitarias (NUnit)
└── Documentacion/   One Page Document
```

### Ensamblados (asmdef)

- `Nebulax.Runtime` — código de juego.
- `Nebulax.Editor` — código solo-Editor (constructor automático de escena).
- `Nebulax.Tests.EditMode` — pruebas unitarias.

## Pruebas

Las pruebas EditMode se ejecutan desde **Window → General → Test Runner →
EditMode → Run All**. Cubren lógica determinista:

- `VidaNaveJugadorTests` — vida, daño, clamp y muerte.
- `LimitesPantallaTests` — recorte de posición a los límites de cámara.
- `GestorJuegoTests` — conteo de enemigos y fin de partida.

## Rendimiento

Los proyectiles (los objetos más numerosos) se reutilizan mediante un **pool de
objetos** (`PoolObjetos`) en lugar de crearse/destruirse continuamente, lo que
reduce la presión sobre el recolector de basura.

## Notas de mantenimiento

- Las escenas y prefabs referencian el código por **GUID** (`.meta`) y por
  **nombre de campo `[SerializeField]`**. No renombrar clases `MonoBehaviour`
  ni campos serializados sin `[FormerlySerializedAs]`, ni borrar `.cs` sin su
  `.meta`.
- Ver `PLAN_MEJORAS.md` para el detalle de las mejoras realizadas y las
  pendientes.
