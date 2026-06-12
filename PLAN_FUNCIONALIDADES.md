# Plan de nuevas funcionalidades — Nebulax

Implementa los 5 requisitos del encargo siguiendo el patrón existente del
proyecto: **scripts de runtime** + **generación por código** desde el
constructor de Editor (`NebulaxConstructorEditor`). Inspirado en las buenas
prácticas 2D de J. García (2021), *Unity: Plataformas en 2D*.

> **Importante (sin Unity en el entorno de desarrollo):** los scripts de runtime
> son C# puro y quedan listos. Los assets de Unity (prefabs, clips de Animator,
> Tilemaps, materiales, 2ª escena) se generan ejecutando los menús del
> constructor de Editor dentro de Unity. Cada parte indica qué menú ejecutar y
> qué validar.

---

## Requisito 1 — Animaciones de enemigos ligadas a eventos + audio

**Objetivo (rúbrica):** animaciones que cambian con los eventos, además asociadas
a audios (fondo, colisión, disparos…).

- **Runtime** `AnimadorEnemigo.cs` (code-driven, fiable sin clips):
  - *Idle*: leve cabeceo/escala pulsante continuo.
  - *Disparo*: "squash" rápido al disparar.
  - *Daño*: flash de color al recibir impacto.
  - *Muerte*: se delega a la explosión existente.
- **Enganches de eventos** en `EnemigoBase`: hooks `AlDisparar()`, `AlRecibirDaño()`
  que el animador escucha (eventos C#), sin acoplar clases.
- **Audio**: ampliar `GestorAudio` con SFX de impacto a enemigo
  (`sfxImpactoEnemigo`) y asegurar música de fondo en bucle (ya existe
  `fondomusical.mp3`). Disparos/explosiones ya suenan.
- **Editor**: además, generar un `AnimatorController` con clips por estado
  (Idle/Hit) para los enemigos, análogo al de la explosión, para cubrir la
  variante con clips de animación.

## Requisito 2 — Recolección de Items (coleccionables) con Prefabs y Debug.Log

**Objetivo (rúbrica):** recolectar coleccionables en la cantidad necesaria como
parte de la mecánica, reflejado en el `Debug.Log` del GameManager, con Prefabs.

- **Runtime** `Coleccionable.cs`: cae por la pantalla; al tocar al `Player`
  notifica al `GestorJuego`, reproduce audio y se libera (pool).
  - Enum `TipoColeccionable { Cristal, NucleoEnergia }`.
- **GestorJuego**: nuevo contador `itemsRecolectados` y método
  `RegistrarItemRecolectado(TipoColeccionable)` que hace `Debug.Log`
  (p. ej. *"[GameManager] Item recolectado: Cristal (3/10)"*).
- **Cantidad necesaria**: meta configurable (p. ej. 10) que dispara recompensa
  y se enlaza con la progresión (Req. 4).
- **Editor**: generar 2 prefabs de coleccionable (con sprite, trigger, tag
  `PowerUp`/nuevo tag `Collectible`) y soltarlos desde enemigos y/o un generador
  de items.

## Requisito 3 — 2ª escena con Tilesets / Tile Palette + materiales y texturas

**Objetivo (rúbrica):** aplicar más de un Tilemap que identifica y caracteriza
el nivel.

- **Editor** `NebulaxTilemapEditor.cs` (constructor dedicado):
  - Generar texturas de tile por código (PNG) y un **material** propio.
  - Crear `Tile` assets (ScriptableObjects) desde esas texturas.
  - Crear escena `EscenaNivel2.unity` con un **Grid** y **varios Tilemaps**:
    1. `Tilemap_Fondo` (suelo/relleno),
    2. `Tilemap_Muros` (límites/obstáculos con `TilemapCollider2D`),
    3. `Tilemap_Decoracion` (detalles).
  - Pintar los tilemaps por código (`SetTile`) para caracterizar el nivel.
  - Registrar la escena en *Build Settings*.
- **Runtime** `ControladorNivel2.cs`: lógica mínima del nivel (cámara, fin de
  nivel) y transición desde el nivel 1.

## Requisito 4 — Incremento de habilidades (Items) y niveles (Velocidad) + dificultad

**Objetivo (rúbrica):** el juego considera incremento de dificultad y habilidades.

- **Runtime** `GestorProgresion.cs` (singleton):
  - Sube de **nivel** cada N items recolectados.
  - Por nivel: **+velocidad** de la nave y mejora de cadencia/habilidad.
  - Aumenta la **dificultad**: cadencia de generación y velocidad de enemigos.
- **Enganches**:
  - `ControladorNaveJugador.AumentarVelocidad(factor)`.
  - `DisparoNaveJugador.MejorarCadencia(factor)`.
  - `GeneradorEnemigos.AumentarDificultad(factor)`.
- **UI**: mostrar nivel e items en el HUD (reutiliza `GestorUI`).

## Requisito 5 — Enemigo jefe animado

**Objetivo (rúbrica):** animar un enemigo jefe adecuadamente.

- **Runtime** `EnemigoJefe.cs`:
  - Vida alta, **fases** (p. ej. 100%/50% cambian patrón de disparo y velocidad).
  - Patrón de movimiento (entrada + vaivén horizontal).
  - Animación: entrada, idle flotante, flash al recibir daño, animación de
    fases; al morir, explosión gigante y evento de victoria.
- **GestorJuego**: invoca al jefe en un hito (p. ej. tras X items o al cruzar el
  área) y maneja la victoria.
- **Editor**: generar prefab del jefe (sprite escalado del Tipo III u otro),
  `AnimatorController` con clips de fases, y cablear todo.

---

## Orden de implementación (una fase por commit, subida a `main`)

| Fase | Contenido | Tipo | Estado |
|------|-----------|------|--------|
| A | Progresión + recolección de items (GestorProgresion, Coleccionable, GestorJuego, enganches) | Runtime | ✅ |
| B | Animación de enemigos por eventos + audio de impacto | Runtime | ✅ |
| C | Enemigo jefe con fases y animación | Runtime | ✅ |
| D | Constructor de Editor: prefabs (items, jefe), AnimatorControllers | Editor | ✅ |
| E | Constructor de Editor: 2ª escena con Tilemaps, tiles, materiales/texturas | Editor | ✅ |
| F | GeneradorItems en escena, tests, HUD (nivel/items), README y documentación | Mixto | ✅ |

## Cobertura de la rúbrica

| Criterio (Sobresaliente) | Implementación |
|--------------------------|----------------|
| Animaciones que cambian con eventos + audios | `AnimadorEnemigo` (idle/disparo/daño) + eventos de `EnemigoBase` + SFX de impacto/disparo/explosión y música de fondo |
| Recolecta coleccionables en cantidad necesaria | `Coleccionable` + `GeneradorItems` + conteo y `Debug.Log` en `GestorJuego`; 5 items por nivel |
| 2ª escena con más de un Tilemap | `EscenaNivel2`: Tilemaps de Fondo, Muros (collider) y Decoración, con material y texturas |
| Incremento de habilidades y dificultad | `GestorProgresion`: +velocidad nave, +cadencia disparo, +dificultad enemigos por nivel |
| Enemigo jefe animado | `EnemigoJefe` con fases, patrones de disparo, entrada/vaivén/flotación y victoria |

## Validación pendiente en Unity (no automatizable sin el Editor)

1. Ejecutar los dos menús `Nebulax/Funcionalidades/...`.
2. Abrir `EscenaPrincipal` y `EscenaNivel2`, comprobar que cargan sin errores.
3. Correr el Test Runner (EditMode) — incluye las pruebas de progresión/items.
4. Jugar: recoger items (ver `Debug.Log`), subir de nivel, enfrentar al jefe.

## Reglas de seguridad (igual que el plan anterior)

- No renombrar clases `MonoBehaviour` ni campos `[SerializeField]` existentes.
- Todo lo nuevo es **aditivo**; no rompe la serialización actual.
- Cada `.cs` nuevo lleva su `.meta` con GUID válido.
- Validación final en Unity: ejecutar los menús del constructor y comprobar la
  escena 1 y la escena 2, el Test Runner y la consola sin errores.
