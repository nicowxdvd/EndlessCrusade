# SPEC 10 — Habilidades con cooldown

> **Estado:** Implementado
> **Depende de:** SPEC 05, SPEC 09
> **Fecha:** 2026-10-06
> **Objetivo:** Agregar habilidades del héroe con cooldown, empezando por el agua bendita, y su botón en el HUD.

---

## Alcance

**Dentro:**

- `AbilityDefinition` (ScriptableObject).
- `AbilityComponent` con hasta 3 ranuras y cooldown por ranura.
- Habilidad `HolyWater`: frasco lanzado en parábola con daño de área.
- Etiquetas de criatura (`CreatureTag`) en `UnitDefinition` para daño extra contra no-muertos.
- Botón de habilidad con relleno radial de cooldown en el HUD.
- Proyectil y explosión desde el pool.

**Fuera de alcance (para specs futuras):**

- Milagros del héroe (SPEC 21).
- Consumibles de la tienda que recargan agua bendita (SPEC 17).
- Árbol de mejoras de cooldown (SPEC 17).

---

## Modelo de datos

```csharp
[System.Flags]
public enum CreatureTag { None = 0, Undead = 1, Beast = 2, Flying = 4 }

public enum AbilityKind { ThrownArea, Buff, Heal }

[CreateAssetMenu(menuName = "EC/Ability Definition")]
public class AbilityDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public AbilityKind kind;
    public float cooldown = 12f;
    public int damage = 40;
    public float radius = 2.5f;
    public float undeadMultiplier = 2f;
    public float throwDistance = 6f;
    public Sprite icon;
}

public readonly struct AbilityRequested { public readonly int Slot; }
public readonly struct AbilityCooldownChanged { public readonly int Slot; public readonly float Normalized; }
```

`UnitDefinition` recibe un campo `public CreatureTag tags;` (modifica SPEC 03). Los vampiros y licántropos de specs 06 y 20 usan las etiquetas correspondientes.

---

## Plan de implementación

1. Agregar `CreatureTag` a `EC.Core` y el campo `tags` a `UnitDefinition`. Verificar: los assets existentes siguen cargando.
2. Crear `AbilityDefinition` y el asset `Ability_HolyWater`.
3. Crear `AbilityComponent` con cooldown, y test EditMode del temporizador.
4. Implementar `HolyWaterProjectile` (pooleado): trayectoria parabólica y explosión con daño de área, con multiplicador contra `Undead`.
5. Agregar la acción `UseAbility1` en `Controls.inputactions` (tecla L) y publicar `AbilityRequested` desde el botón.
6. Crear el botón de habilidad en el HUD que escucha `AbilityCooldownChanged` y bloquea el toque en cooldown.
7. Probar en `LaneSandbox` con enemigos agrupados.

---

## Criterios de aceptación

- [x] El agua bendita se lanza a `throwDistance` frente al héroe.
- [x] Daña a todos los enemigos en `radius` y daña `undeadMultiplier` veces más a los `Undead`.
- [x] Tras usarla, el botón se bloquea `cooldown` segundos.
- [x] El relleno radial del botón refleja el cooldown restante.
- [x] Usar la habilidad en cooldown no hace nada.
- [x] Proyectil y explosión no asignan memoria por frame (usan el pool).

---

## Decisiones

- **Sí:** agua bendita primero. La historia la menciona como equipo que el héroe recupera, y es un área simple de probar.
- **Sí:** daño mayor contra no-muertos. Refuerza la fantasía templaria.
- **Sí:** habilidades por ranura y evento `AbilityRequested`. `EC.UI` no llama a `EC.Gameplay` directamente.
- **Sí:** definición rápida sin aclaración detallada.

---

## Lo que **no** está en esta spec

- Milagros, consumibles y mejoras de cooldown.
