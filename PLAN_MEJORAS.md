# Plan de mejoras y correcciones — Nebulax

> Documento de planificación. Ningún cambio aquí es destructivo por sí mismo;
> describe **qué** cambiar, **por qué**, **cómo hacerlo sin romper Unity** y en
> **qué orden**. Pensado para Unity 6 (6000.4.9f1) + URP + Input System.

## Estado de ejecución

| Fase | Descripción | Estado |
|------|-------------|--------|
| 0 | Red de seguridad (`.gitignore`, limpieza de generados) | ✅ Hecho |
| 1 | Limpieza (prefabs huérfanos, log de depuración) | ✅ Hecho |
| 2 | Corrección de fragilidad lógica (flags, cacheo, daño misil) | ✅ Hecho |
| 3 | Object pooling de proyectiles | ✅ Hecho |
| 4 | Soporte TextMeshPro sin romper UI legacy | ✅ Hecho |
| 5 | Asmdef + pruebas unitarias EditMode | ✅ Hecho |
| 6 | Pulido (README, tags, límites) | ✅ Hecho |

> **Validación pendiente en Unity (no automatizable sin el Editor):** abrir
> `EscenaPrincipal`, importar TMP Essentials si se desea usar TMP, compilar sin
> errores, correr el Test Runner (EditMode) y comprobar que no hay referencias
> `Missing`. Mejoras futuras sugeridas: poolear también explosiones y enemigos;
> migrar visualmente la UI a TMP arrastrando los componentes a los campos TMP.

## Regla de oro (no romper el cableado de Unity)

Las escenas (`EscenaPrincipal.unity`) y los prefabs referencian el código por:

1. **GUID del script** (en el `.meta` de cada `.cs`). → No borrar ni mover
   archivos `.cs` sin arrastrar su `.meta`. No renombrar clases `MonoBehaviour`.
2. **Nombre del campo `[SerializeField]`**. → Renombrar un campo serializado
   pierde su valor asignado en el Inspector salvo que se use `[FormerlySerializedAs]`.

**Por tanto:** todos los refactores de abajo conservan nombres de clase y de
campos serializados. Cuando haga falta renombrar, se usa
`[UnityEngine.Serialization.FormerlySerializedAs("viejoNombre")]`.

Cada fase debe **compilar en Unity y abrir `EscenaPrincipal` sin errores ni
referencias `Missing`** antes de pasar a la siguiente.

---

## Fase 0 — Red de seguridad (antes de tocar nada)

- [ ] Confirmar que el repo está limpio (`git status`) y crear rama de trabajo.
- [ ] Abrir el proyecto en Unity una vez y verificar que la escena carga sin
      errores en consola (línea base).
- [ ] Añadir `.gitignore` de Unity si falta (revisar que `Library/`, `Temp/`,
      `Logs/`, `obj/` no se versionen). Los `.csproj` y `.slnx` son generados;
      idealmente también se ignoran.
- [ ] Etiquetar el estado actual: `git tag baseline-pre-mejoras`.

Criterio de aceptación: build/escena igual que antes, commit de checkpoint.

---

## Fase 1 — Limpieza de bajo riesgo (sin cambiar comportamiento)

### 1.1 Prefabs duplicados
Existen pares `ParedesXxx` y `Paredes_Xxx` (enemigos I/II/III, proyectiles,
misil). Son ruido del generador de editor.
- [ ] Identificar **cuál set referencia la escena y el `GeneradorEnemigos`**
      (buscar el GUID del prefab en `EscenaPrincipal.unity` y en los prefabs que
      lo instancian).
- [ ] Eliminar **solo** el set huérfano (con su `.meta`). Si ambos están en uso,
      consolidar a uno y reasignar referencias.
- [ ] Verificar que `GeneradorEnemigos` sigue teniendo sus 3 prefabs asignados.

### 1.2 Encoding/caracteres
- [ ] `RecibirDaño`, `dañoAlJugador`, etc. usan `ñ`/tildes en identificadores.
      Compila, pero es frágil entre herramientas. **No renombrar ahora**
      (rompería serialización de `dañoAlJugador`, `probabilidadSoltarPoder` ya
      está bien). Dejar como deuda documentada, no tocar.

### 1.3 Logging de depuración
- [ ] `RegistroEstadoJugador` hace `Debug.Log` cada segundo (requisito académico
      del autor — "Vida Paredes"). Envolver en `#if UNITY_EDITOR ||
      DEVELOPMENT_BUILD` para que no spamee en build de release, **manteniendo**
      el comportamiento en editor.

Criterio de aceptación: la escena instancia los mismos prefabs, consola limpia
en release, sin cambios jugables.

---

## Fase 2 — Corrección de fragilidad lógica (las observaciones de fondo)

### 2.1 Acoplamiento de banderas en `GeneradorEnemigos`
Problema: `DetenerGeneracion()` baja `generarEnemigos`, y
`HabilitarEnemigoTipoTresTemporal()` lo vuelve a subir a `true`. El estado
"juego detenido" y "oleada temporal activa" están mezclados en un solo flag,
lo que hace el flujo difícil de razonar y propenso a reactivar el Tipo I sin
querer.

- [ ] Separar responsabilidades en flags explícitos:
  - `generacionGlobalActiva` (master on/off del generador).
  - Las oleadas temporales (II y III) ya tienen sus propios `bool` + corrutina;
    no deben tocar el flag de Tipo I.
- [ ] `HabilitarEnemigoTipoTresTemporal` **no** debe re-encender la generación
      global de Tipo I; debe encender solo su propia oleada. Hoy lo hace para
      sortear que `CrearEnemigo` chequea `generarEnemigos`; sustituir ese check
      por el flag específico de cada oleada.
- [ ] Revisar la secuencia en `GestorJuego.ActivarAreaBatallaConPreparacion`:
      hoy llama `DetenerGeneracion()` y luego `HabilitarEnemigoTipoTresTemporal`.
      Con flags separados, el Tipo I queda apagado y solo aparece la oleada III,
      que es la intención real.

Pruebas manuales: llegar a 10 bajas → no deben reaparecer Tipo I durante la
ventana de 3s; sí deben aparecer pares de Tipo III; luego aparece la estructura.

### 2.2 `FindObjectsByType` / `FindFirstObjectByType` en runtime
- [ ] `GestorJuego.ResolverReferencias()` ya cachea en `Start` — aceptable.
- [ ] La limpieza de enemigos/proyectiles en la transición al área usa
      `FindObjectsByType` una sola vez (evento puntual) — aceptable, dejar.
- [ ] `DetectorPasoEstructura` y `DetectorPasoEstructura.BuscarJugador()` llaman
      `FindWithTag` cada frame hasta encontrar al jugador. Cachear la referencia
      del jugador (p. ej. el `GestorJuego` expone `Transform Jugador`) y que el
      detector la pida una vez. Reduce coste por frame.

### 2.3 Misil con daño "mágico" 999
- [ ] `MisilJugador.daño = 999` para matar a Tipo III de un golpe. Funciona pero
      es un valor mágico. Opciones (elegir una, no urgente):
      - Mantener pero documentar con constante nombrada, o
      - Añadir un flag `destruyeDeUnGolpe` en `EnemigoBase` que `RecibirDaño`
        respete. Más limpio y explícito.

Criterio de aceptación: el flujo de oleadas es predecible; sin cambios visibles
para el jugador salvo la corrección del bug de reaparición de Tipo I.

---

## Fase 3 — Rendimiento: Object Pooling

Hoy proyectiles, enemigos, poderes y explosiones usan `Instantiate`/`Destroy`
constantemente → picos de GC y hitches, especialmente con triple disparo +
oleadas.

- [ ] Introducir un pool genérico reutilizable. Unity 6 trae
      `UnityEngine.Pool.ObjectPool<T>` — usarlo en vez de escribir uno propio.
- [ ] Candidatos a poolear (orden de impacto):
  1. **Proyectiles del jugador** (`ProyectilJugador`) — los más numerosos.
  2. **Proyectiles enemigos** (`ProyectilEnemigo`).
  3. **Explosiones** (`prefabExplosion` en `GestorJuego`).
  4. Enemigos (opcional; menos frecuentes).
- [ ] Patrón sin romper prefabs: los scripts ya tienen `OnEnable` que resetea
      estado (`EnemigoBase.OnEnable`, `ControladorExplosion.OnEnable`). Sustituir
      `Destroy(gameObject)` por `gameObject.SetActive(false)` + devolución al
      pool. Las clases de proyectil necesitan dejar de auto-`Destroy` y en su
      lugar notificar al pool (o usar un componente `DevolverAlPool`).
- [ ] **Cuidado con `TrailRenderer`/`ParticleSystem`** en proyectiles
      (`EfectoGlowProyectil`): al reusar del pool hay que limpiar la estela
      (`TrailRenderer.Clear()`) en `OnEnable` para que no "salte" del punto
      anterior al nuevo punto de disparo.
- [ ] Las partículas de explosión usan `useUnscaledTime`; conservar esa config al
      reusar.

Medición: comparar GC Alloc por frame en el Profiler antes/después con triple
disparo activo. Objetivo: 0 alloc por disparo en estado estable.

Criterio de aceptación: jugabilidad idéntica, sin asignaciones por disparo,
estelas correctas tras reuso.

---

## Fase 4 — Migración de UI legacy a TextMeshPro

`GestorUI` usa `UnityEngine.UI.Text` (legacy). TMP es el estándar y se ve mejor.

- [ ] Esta fase **sí** toca tipos serializados (`Text` → `TMP_Text`), así que
      requiere reasignar en el Inspector. Hacerla aislada en su propio commit.
- [ ] Importar TMP Essentials (Unity lo pide al primer uso).
- [ ] Cambiar campos de `Text` a `TMP_Text` en `GestorUI`, `AlertaAnimada` usa
      `Image` (no cambia). Mantener nombres de campo
      (`textoVidaJugador`, etc.) con `[FormerlySerializedAs]` no aplica entre
      tipos distintos → habrá que **re-arrastrar** los objetos de texto.
- [ ] Reemplazar los componentes `Text` en la escena por `TextMeshProUGUI`,
      conservar el rich text ya usado (`<color>`, `<size>`).
- [ ] Verificar `MostrarGameOver` (parsea el texto del contador) sigue
      funcionando con TMP — mejor: leer el contador desde `GestorJuego`
      directamente en lugar de parsear el string de UI (más robusto).

Criterio de aceptación: HUD, alerta y game over se ven igual o mejor, sin
referencias rotas.

> Nota: si se prefiere minimizar riesgo en un proyecto académico ya entregado,
> esta fase es **opcional**. Documentar la decisión.

---

## Fase 5 — Tests (el framework ya está instalado, sin usar)

`com.unity.test-framework` 1.6 está en el manifest. Añadir cobertura de la
**lógica pura** (la que no depende de física/render).

- [ ] Crear assembly de tests EditMode (`Tests/EditMode/Nebulax.Tests.asmdef`).
      Requiere que el código de juego esté en un assembly propio (`asmdef`) para
      poder referenciarlo — añadir `Nebulax.Runtime.asmdef` en
      `Assets/_Nebulax/Scripts`. (Cambio de bajo riesgo, pero recompila todo;
      hacerlo en commit propio.)
- [ ] Tests unitarios candidatos (lógica determinista):
  - `VidaNaveJugador`: `PorcentajeVida` redondea bien; `RecibirDaño` clampa a 0;
    `MorirInstantaneamente` marca muerte una sola vez.
  - `GestorJuego`: hitos en 3 y 10 disparan los eventos una única vez
    (`eventoTresEnemigosActivado` / `eventoDiezEnemigosActivado`).
  - `LimitesPantalla.LimitarPosicion`: clamp correcto dado un mock de cámara.
- [ ] Tests PlayMode opcionales para el flujo de oleadas (más costosos).

Criterio de aceptación: `dotnet`/Unity Test Runner verde en EditMode.

---

## Fase 6 — Pulido y documentación

- [ ] README de proyecto (controles, cómo abrir, versión de Unity) — hoy solo
      hay el One Page PDF.
- [ ] Revisar valores mágicos repetidos de límites de pantalla
      (`limiteSuperior = 7`, `limiteInferior = -7` repetidos en 4 scripts) →
      centralizar o al menos documentar que dependen del tamaño de cámara.
- [ ] Verificar tags requeridos existen en `TagManager` (`Player`, `Enemy`) —
      el código depende de ellos en 7 sitios; si faltan, `CompareTag` lanza.
- [ ] Pasada final de consola: 0 warnings nuevos introducidos.

---

## Orden de ejecución recomendado y riesgo

| Fase | Cambia serialización | Riesgo | Reversible |
|------|----------------------|--------|------------|
| 0 Red de seguridad | No | Nulo | — |
| 1 Limpieza | Solo borra duplicados huérfanos | Bajo | Sí (git) |
| 2 Lógica de flags | No (solo cuerpo de métodos) | Bajo-medio | Sí |
| 3 Pooling | No (lógica interna) | Medio | Sí |
| 4 TMP | **Sí** (Text→TMP) | Medio-alto | Sí, aislado |
| 5 Tests | Añade asmdef | Bajo | Sí |
| 6 Pulido | No | Bajo | Sí |

Hacer **un commit por fase** (o sub-fase), abriendo Unity y validando la escena
entre cada uno. Las fases 2 y 3 son las que más valor técnico aportan; la 4 es
la única que obliga a re-cablear el Inspector, así que va aislada.

## Qué NO hacer

- No renombrar clases `MonoBehaviour` ni campos `[SerializeField]` sin
  `[FormerlySerializedAs]`.
- No borrar `.cs` sin su `.meta`.
- No "arreglar" los identificadores con `ñ`/tildes (rompe serialización).
- No mover archivos entre carpetas sin dejar que Unity actualice los `.meta`.
