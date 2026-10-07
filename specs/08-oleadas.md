# SPEC 08 — Oleadas

> **Estado:** Implementado
> **Depende de:** SPEC 06, SPEC 07
> **Fecha:** 2026-10-06
> **Objetivo:** Controlar las oleadas desde `LevelDefinition` y `WaveDefinition` (ScriptableObjects) con condiciones de victoria y derrota.

---

## Alcance

**Dentro:**

- `LevelDefinition`, `WaveDefinition` y `SpawnEntry` (ScriptableObjects y clases serializables).
- `WaveRunner` (clase C# pura, testeable) con el estado de las oleadas.
- `WaveController` (MonoBehaviour) que conecta `WaveRunner` con `EnemySpawner`.
- Eventos `WaveStarted` y `LevelEnded`.
- Victoria al terminar todas las oleadas con 0 enemigos vivos.
- Derrota si se destruye la base o muere el héroe.
- Nivel de prueba `Level_Test` en `LaneSandbox`.

**Fuera de alcance (para specs futuras):**

- Jefes (SPEC 11).
- Recompensas (SPEC 15).
- Selección de nivel y escena de nivel real (SPEC 12 y 16).
- Reanimación del héroe (SPEC 17 y 26).

---

## Modelo de datos

```csharp
public enum LevelOutcome { Victory, Defeat }

[System.Serializable]
public class SpawnEntry
{
    public EnemyDefinition enemy;
    public int count;
    public float interval;
    public float startDelay;
}

[CreateAssetMenu(menuName = "EC/Wave Definition")]
public class WaveDefinition : ScriptableObject
{
    public SpawnEntry[] entries;
    public float delayBeforeNext = 4f;
}

[CreateAssetMenu(menuName = "EC/Level Definition")]
public class LevelDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public BaseDefinition baseDefinition;
    public WaveDefinition[] waves;
}

public readonly struct WaveStarted { public readonly int Index; public readonly int Total; }
public readonly struct LevelEnded { public readonly LevelOutcome Outcome; }
```

`WaveRunner` recibe el tiempo por parámetro y emite órdenes de spawn. No usa `Time` de Unity, para poder testearlo.

---

## Plan de implementación

1. Crear `SpawnEntry`, `WaveDefinition`, `LevelDefinition` en `EC.Data` y los eventos en `EC.Core`.
2. Crear `WaveRunner` y tests EditMode: orden de spawns, intervalos, avance de oleada, victoria con 0 vivos.
3. Crear `WaveController` que lee `LevelDefinition`, usa `EnemySpawner` y cuenta enemigos vivos con `EntityDied`.
4. Conectar la derrota a `BaseDestroyed` y a la muerte del héroe.
5. Crear `Level_Test` con 3 oleadas de wargos y murciélagos. Verificar: en `LaneSandbox` se juega hasta victoria o derrota.
6. Al terminar, publicar `LevelEnded` y detener los spawns.

---

## Criterios de aceptación

- [x] `WaveController` inicia la oleada 1 tras una cuenta regresiva de 3 segundos.
- [x] Los enemigos aparecen según `count`, `interval` y `startDelay`.
- [x] La siguiente oleada empieza `delayBeforeNext` segundos después de matar a todos.
- [x] Se publica `LevelEnded(Victory)` al matar al último enemigo de la última oleada.
- [x] Se publica `LevelEnded(Defeat)` si `BaseDestroyed` o el héroe muere.
- [x] `LevelEnded` se publica una sola vez.
- [x] Los tests EditMode de `WaveRunner` pasan.

---

## Decisiones

- **Sí:** `WaveRunner` puro y testeable, `WaveController` fino.
- **Sí:** muerte del héroe cuenta como derrota. La resurrección se agrega en SPEC 17 y 26.
- **Sí:** oleadas y niveles como ScriptableObjects. Es el patrón fijado por el documento técnico.
- **Sí:** definición rápida sin aclaración detallada.

---

## Lo que **no** está en esta spec

- Jefes, recompensas y selección de niveles.
- Pantallas de resultado.
- Resurrección.
