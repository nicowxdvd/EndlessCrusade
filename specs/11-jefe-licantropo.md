# SPEC 11 — Jefe Licántropo Gigante

> **Estado:** Borrador
> **Depende de:** SPEC 06, SPEC 08
> **Fecha:** 2026-10-06
> **Objetivo:** Crear al Licántropo Gigante con embestida devastadora, rugido que desorienta y barra de vida de jefe.

---

## Alcance

**Dentro:**

- `BossDefinition` (ScriptableObject) que extiende `EnemyDefinition`.
- `BossBrain` con estados: Avance, Aviso de embestida, Embestida, Aturdido, Rugido.
- Fase de furia al 50% de vida.
- Efecto de estado `Disoriented` que invierte el movimiento del héroe.
- Sacudida de cámara en el rugido y en el impacto de la embestida.
- Barra de vida del jefe en el HUD.
- Soporte de jefe en `WaveDefinition` (entrada de jefe).

**Fuera de alcance (para specs futuras):**

- Otros jefes (SPEC 22, 23, 24).
- Arte y animación final del jefe.
- Música de jefe (SPEC 28).

---

## Modelo de datos

```csharp
[CreateAssetMenu(menuName = "EC/Boss Definition")]
public class BossDefinition : EnemyDefinition
{
    public float chargeWindup = 1f;
    public int chargeDamage = 35;
    public float stunAfterCharge = 1.5f;
    public float roarInterval = 12f;
    public float roarRadius = 8f;
    public float disorientDuration = 3f;
    public float enrageThreshold = 0.5f;
    public float enrageSpeedMultiplier = 1.4f;
}

public readonly struct BossSpawned { public readonly GameObject Boss; public readonly string DisplayName; }
```

Valores propuestos: vida 600, daño base 20, velocidad 1.6.

`StatusEffectComponent` (en `EC.Gameplay`) aplica `Disoriented`. `HeroController` consulta `IsDisoriented` e invierte el eje de movimiento.

---

## Plan de implementación

1. Crear `BossDefinition` y el asset `Boss_GiantLycanthrope`.
2. Crear `StatusEffectComponent` con `Disoriented` y test EditMode de duración.
3. Hacer que `HeroController` invierta el movimiento si está desorientado.
4. Crear `BossBrain` con Avance y Embestida (aviso visible en el suelo de 1 segundo). Verificar: la embestida daña al héroe y a la base.
5. Agregar Aturdido tras la embestida para dar ventana de ataque.
6. Agregar Rugido periódico: desorienta al héroe si está en `roarRadius` y sacude la cámara.
7. Agregar la fase de furia al 50% de vida.
8. Publicar `BossSpawned` y agregar la barra de jefe al HUD.
9. Agregar la entrada de jefe en `WaveDefinition`. Verificar en `Level_Test`.

---

## Criterios de aceptación

- [ ] La embestida muestra un aviso de 1 segundo antes de ejecutarse.
- [ ] La embestida inflige `chargeDamage` al héroe o a la base si los alcanza.
- [ ] Tras la embestida, el jefe queda aturdido `stunAfterCharge` segundos.
- [ ] El rugido invierte los controles del héroe `disorientDuration` segundos.
- [ ] Al 50% de vida el jefe se mueve `enrageSpeedMultiplier` veces más rápido.
- [ ] La barra de jefe aparece al aparecer el jefe y refleja su vida.
- [ ] Matar al jefe y a todos los enemigos termina el nivel con victoria.

---

## Decisiones

- **Sí:** rugido que desorienta mediante inversión de controles. Es la forma más directa de cumplir "rugir para desorientar" de la historia.
- **No:** ceguera o niebla en pantalla. Cuesta más rendimiento y arte.
- **Sí:** ventana de aturdimiento tras embestir. Da al jugador una forma de contrarrestar al jefe.
- **Sí:** definición rápida sin aclaración detallada. Valores propuestos.

---

## Lo que **no** está en esta spec

- Otros jefes, arte y música de jefe.
