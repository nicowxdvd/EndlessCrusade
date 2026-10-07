# SPEC 07 — Base defendible

> **Estado:** Borrador
> **Depende de:** SPEC 03
> **Fecha:** 2026-10-06
> **Objetivo:** Crear una estructura base configurable por nivel con barra de resistencia propia que provoca la derrota al llegar a cero.

---

## Alcance

**Dentro:**

- `BaseDefinition` (ScriptableObject).
- `BaseStructure` (componente `IDamageable` del equipo Player).
- Etapas visuales de daño (intacta, dañada, casi destruida).
- Eventos `BaseResistanceChanged` y `BaseDestroyed`.
- Primera definición: `Base_CabinRuins` (ruinas de la cabaña).
- Los enemigos de SPEC 06 atacan la base al alcanzarla.

**Fuera de alcance (para specs futuras):**

- Defensas pasivas, como ballesteros en las almenas (SPEC 17).
- Reparación y mejoras de resistencia (SPEC 17).
- Bases de otros capítulos (SPEC 22, 23, 24).

---

## Modelo de datos

```csharp
[CreateAssetMenu(menuName = "EC/Base Definition")]
public class BaseDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public int maxResistance = 500;
    public GameObject prefab;
    public Sprite[] damageStages;
}

public readonly struct BaseResistanceChanged { public readonly int Current; public readonly int Max; }
public readonly struct BaseDestroyed { }
```

La base se coloca en `LaneConfig.baseX`. Las etapas visuales cambian al 66% y 33% de resistencia.

---

## Plan de implementación

1. Crear `BaseDefinition` y el asset `Base_CabinRuins` con prefab placeholder.
2. Crear `BaseStructure` implementando `IDamageable` (equipo Player) con resistencia, sin FSM.
3. Publicar `BaseResistanceChanged` en cada daño y `BaseDestroyed` al llegar a 0.
4. Implementar el cambio de etapa visual por porcentaje.
5. Hacer que `EnemyBrain` elija la base como objetivo cuando no hay héroe en rango. Verificar: en `LaneSandbox`, un wargo llega a la base y la daña.
6. Crear un test EditMode de umbrales de etapa (66%, 33%, 0%).

---

## Criterios de aceptación

- [ ] La base muestra resistencia inicial igual a `maxResistance`.
- [ ] Un enemigo en rango ataca la base y baja su resistencia.
- [ ] `BaseResistanceChanged` se publica en cada daño.
- [ ] La etapa visual cambia al cruzar 66% y 33%.
- [ ] Al llegar a 0 se publica `BaseDestroyed` una sola vez.
- [ ] La base no acepta daño del equipo Player.

---

## Decisiones

- **Sí:** la base cambia por nivel mediante `BaseDefinition`. Decisión de Nico para encajar con la historia.
- **Sí:** primera base, ruinas de la cabaña, para el nivel 1.
- **No:** reparación durante el combate. Se evalúa con la tienda (SPEC 17).
- **Sí:** definición rápida sin aclaración detallada. Valor de 500 de resistencia propuesto.

---

## Lo que **no** está en esta spec

- Defensas pasivas y mejoras.
- Pantalla de derrota (SPEC 09).
- Bases de otros capítulos.
