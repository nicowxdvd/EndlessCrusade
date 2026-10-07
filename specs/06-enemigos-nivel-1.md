# SPEC 06 — Enemigos del nivel 1

> **Estado:** Borrador
> **Depende de:** SPEC 03, SPEC 04
> **Fecha:** 2026-10-06
> **Objetivo:** Agregar el wargo (embestida rápida por tierra) y el murciélago (bandada aérea) con IA por FSM y aparición desde el pool.

---

## Alcance

**Dentro:**

- `EnemyDefinition` (ScriptableObject) que extiende `UnitDefinition`.
- Prefabs `Enemy_Warg` y `Enemy_Bat`.
- `EnemyBrain` con la FSM de SPEC 03: avanza hacia la base, ataca a lo que esté en rango.
- `TargetFinder` para elegir objetivos por distancia en X y equipo.
- `EnemySpawner` que usa `PoolService`.
- Embestida del wargo.
- Vuelo ondulado del murciélago.

**Fuera de alcance (para specs futuras):**

- Base como objetivo con vida (SPEC 07). Hasta entonces el objetivo es un `Transform` en `LaneConfig.baseX`.
- Oleadas (SPEC 08).
- Jefes (SPEC 11) y enemigos adicionales (SPEC 20).

---

## Modelo de datos

```csharp
public enum EnemyKind { Ground, Flying }

[CreateAssetMenu(menuName = "EC/Enemy Definition")]
public class EnemyDefinition : UnitDefinition
{
    public EnemyKind kind;
    public int goldDrop = 5;
    public float chargeRange = 5f;
    public float chargeSpeedMultiplier = 2.2f;
    public float chargeDuration = 0.8f;
    public float flightHeight = 2.2f;
    public float bobAmplitude = 0.4f;
    public float bobFrequency = 2f;
}
```

Valores propuestos. Wargo: vida 40, velocidad 2.5, daño 12, embestida x2.2 a 5 unidades. Murciélago: vida 15, velocidad 3.5, daño 6, vuela a 2.2 de altura y desciende para atacar.

`TargetFinder.FindNearest(Vector3 from, Team enemyOf, float maxDistance)` devuelve el `IDamageable` más cercano de otro equipo.

---

## Plan de implementación

1. Crear `EnemyDefinition` y los assets `Enemy_Warg` y `Enemy_Bat`.
2. Crear `TargetFinder` con test EditMode (elige el más cercano, ignora el mismo equipo y los muertos).
3. Crear `EnemyBrain` con estados Move y Attack. Verificar: un enemigo de prueba camina hacia la base y ataca al héroe en rango.
4. Agregar la embestida del wargo: al estar a `chargeRange` del objetivo, acelera durante `chargeDuration`.
5. Agregar el vuelo del murciélago: altura fija más movimiento senoidal, desciende al atacar.
6. Crear los prefabs, implementar `IPoolable` (reinicia vida y estado) y registrarlos en el pool.
7. Crear `EnemySpawner` con método `Spawn(EnemyDefinition)` en `LaneConfig.spawnX`. Verificar: 30 spawns y despawns no asignan memoria.

---

## Criterios de aceptación

- [ ] El wargo avanza hacia la izquierda y embiste al llegar a 5 unidades del objetivo.
- [ ] El murciélago vuela a 2.2 de altura con movimiento ondulado.
- [ ] Un enemigo ataca al héroe si está en rango, y si no, sigue hacia la base.
- [ ] Un enemigo muerto vuelve al pool y reaparece con vida completa.
- [ ] `TargetFinder` ignora entidades del mismo equipo y entidades muertas.
- [ ] 30 ciclos de spawn y muerte no asignan memoria por frame.

---

## Decisiones

- **Sí:** un solo `EnemyBrain` configurado por `EnemyDefinition`. Evita una clase por enemigo.
- **Sí:** enemigos aéreos pelean en el mismo carril. Mantiene el diseño de un único carril.
- **Sí:** definición rápida sin aclaración detallada. Valores propuestos.

---

## Lo que **no** está en esta spec

- Base con vida.
- Oleadas y condiciones de victoria.
- Jefes y el resto del bestiario.
