# SPEC 13 — Liderazgo e invocación de tropas

> **Estado:** Implementado
> **Depende de:** SPEC 08, SPEC 09
> **Fecha:** 2026-10-06
> **Objetivo:** Agregar el recurso Liderazgo que se regenera con el tiempo, la interfaz de invocación y el Escudero como primera tropa.

---

## Alcance

**Dentro:**

- `LeadershipComponent` con regeneración por segundo y máximo.
- `TroopDefinition` (ScriptableObject) que extiende `UnitDefinition`.
- `TroopSummoner` que invoca tropas desde el pool junto a la base.
- Tropa Escudero (combate cuerpo a cuerpo, resistente).
- `TroopBrain` con la FSM: avanza hacia enemigos, ataca, vuelve cerca de la base si no hay enemigos.
- Barra de invocación en HUD con costo y cooldown por tropa.
- Límite de tropas activas.
- Nivel de prueba `Level_Test` con tropas habilitadas.

**Fuera de alcance (para specs futuras):**

- Campesino, Ballestero, Sacerdote y Paladín (SPEC 19).
- Mejoras de tropas (SPEC 17).
- Desbloqueo por capítulo (SPEC 19).

---

## Modelo de datos

```csharp
[CreateAssetMenu(menuName = "EC/Troop Definition")]
public class TroopDefinition : UnitDefinition
{
    public int leadershipCost = 30;
    public float summonCooldown = 6f;
    public string unlockChapterId;
}

public readonly struct LeadershipChanged { public readonly float Current; public readonly float Max; }
public readonly struct TroopSummonRequested { public readonly string TroopId; }
```

Valores propuestos. Liderazgo máximo 100, regeneración 4 por segundo, tropas activas máximo 8. Escudero: vida 120, daño 10, velocidad 2.2, costo 30, cooldown 6.

`LevelDefinition.troopsEnabled` (agregado en SPEC 12) activa o desactiva la barra de invocación. En la interfaz el recurso se muestra como "Fe".

---

## Plan de implementación

1. Crear `LeadershipComponent` y test EditMode de regeneración y tope.
2. Crear `TroopDefinition` y el asset `Troop_Squire`.
3. Crear `TroopBrain` reutilizando `TargetFinder` y el prefab `Troop_Squire` pooleado.
4. Crear `TroopSummoner`: valida costo, cooldown y límite, descuenta y crea la tropa.
5. Publicar `LeadershipChanged` y `TroopSummonRequested`.
6. Crear la barra de invocación en el HUD con botón por tropa, costo y cooldown.
7. Crear `Level_Test` con `troopsEnabled = true` y verificar combate conjunto.

---

## Criterios de aceptación

- [x] El Liderazgo se regenera a 4 por segundo y no supera 100.
- [x] Invocar un Escudero descuenta 30 y lo crea junto a la base.
- [x] No se puede invocar con Liderazgo insuficiente, en cooldown o con 8 tropas activas.
- [x] El Escudero avanza, ataca enemigos y muere al llegar a vida 0.
- [x] La barra de invocación no aparece si `troopsEnabled = false`.
- [x] Invocar y morir tropas no asigna memoria por frame.

---

## Decisiones

- **Sí:** el recurso se llama Liderazgo en código y Fe en la interfaz. Sigue la nomenclatura del documento técnico.
- **Sí:** tropas pooleadas y limitadas a 8. Protege el rendimiento en móviles.
- **Sí:** definición rápida sin aclaración detallada. Valores propuestos.

---

## Lo que **no** está en esta spec

- El resto de las tropas, mejoras y desbloqueos.
