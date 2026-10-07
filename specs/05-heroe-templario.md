# SPEC 05 — Héroe Templario

> **Estado:** Implementado
> **Depende de:** SPEC 02, SPEC 03
> **Fecha:** 2026-10-06
> **Objetivo:** Controlar al Templario con botones táctiles de movimiento horizontal y dos ataques, espada corta y látigo.

---

## Alcance

**Dentro:**

- `HeroDefinition` (ScriptableObject) con estadísticas y dos ataques.
- Extensión de `AttackComponent` (SPEC 03) para soportar varios ataques con cooldown independiente.
- Prefab `Hero` con los componentes de SPEC 03.
- `HeroController` que lee Input System y alimenta la FSM.
- Asset `Controls.inputactions` con acciones `MoveLeft`, `MoveRight`, `AttackSword` y `AttackWhip`.
- Controles táctiles con `OnScreenButton` (izquierda, derecha, espada, látigo).
- Teclado para el editor: A y D para mover, J espada, K látigo.
- Evento `HeroSpawned`.
- Uso de `TestDummy` (SPEC 03) como objetivo de prueba de daño.

**Fuera de alcance (para specs futuras):**

- Habilidades con cooldown como agua bendita (SPEC 10).
- Equipo adicional: armadura, escudo, maza, ballesta (SPEC 21).
- HUD definitivo (SPEC 09).
- Animaciones finales.
- Empuje (knockback). Ningún sistema de SPEC 03 lo soporta.
- Consecuencias de la muerte del héroe (derrota, reaparición). Aquí solo deja de responder a la entrada.

---

## Modelo de datos

```csharp
[System.Serializable]
public class MeleeAttackDefinition
{
    public string id;
    public int damage;
    public float range;
    public float cooldown;
}

[CreateAssetMenu(menuName = "EC/Hero Definition")]
public class HeroDefinition : UnitDefinition
{
    public MeleeAttackDefinition sword;
    public MeleeAttackDefinition whip;
}

public readonly struct HeroSpawned
{
    public readonly GameObject Hero;
    public HeroSpawned(GameObject hero) { Hero = hero; }
}
```

Valores propuestos del nivel 1 (héroe sin armadura): vida 120, velocidad 3.0. Espada: daño 18, alcance 1.4, cooldown 0.5. Látigo: daño 10, alcance 3.0, cooldown 0.9.

Reglas de ataque:

- `AttackComponent` conserva `damage`, `range` y `cooldown` como ataque por defecto (lo siguen usando enemigos y `DummyBrain`). Se agrega `TryAttack(MeleeAttackDefinition attack, IDamageable target, float now)` con un cooldown independiente por `attack.id`. El cooldown de un ataque no bloquea al otro.
- Los campos heredados `attackDamage`, `attackRange` y `attackCooldown` de `UnitDefinition` no se usan en el héroe. Se ignoran y no se borran, porque otras definiciones los necesitan.
- Un ataque golpea a un solo objetivo: el `IDamageable` vivo de otro `Team` más cercano en X, dentro del lado al que mira el héroe.
- El rango se mide en X entre centros y es inclusivo (`distancia <= range`).
- El ataque pasa por el estado `Attack` de la FSM y vuelve a `Idle` en el mismo frame del golpe. No hay duración de animación en esta spec.
- Dirección (`facing`): la fija el último input de movimiento, `-1` izquierda o `+1` derecha. El sprite se espeja con `flipX`. Al inicio mira a la derecha.
- Si se pulsan izquierda y derecha a la vez, el movimiento es cero y `facing` no cambia.

Input:

- `MoveLeft` y `MoveRight` son dos acciones de tipo botón. Cada botón táctil es un `OnScreenButton` mapeado a su acción. `HeroController` calcula la dirección como `MoveRight - MoveLeft`.
- El héroe nace en `LaneConfig.heroStartX`.

---

## Plan de implementación

1. Crear `HeroDefinition` y el asset `Hero_Templar_Unarmored`.
2. Extender `AttackComponent` con `TryAttack(MeleeAttackDefinition, IDamageable, float)` y cooldown por `id`. Test EditMode: dos ataques con distinto `id` tienen cooldowns independientes y los tests existentes de SPEC 03 siguen en verde.
3. Crear `Controls.inputactions` con las cuatro acciones y bindings de teclado.
4. Crear el prefab `Hero` con sprite placeholder, `BillboardSprite`, `HealthComponent`, `MovementComponent`, `AttackComponent` y `HeroController`. Colocarlo en `heroStartX`.
5. Implementar movimiento en `HeroController`. Verificar: A y D mueven al héroe en `LaneSandbox` y respeta límites.
6. Implementar ataque de espada (daño en rango frontal, cooldown). Verificar: `TestDummy` pierde vida.
7. Implementar ataque de látigo con mayor alcance. Verificar: golpea al dummy a 3 unidades y la espada no.
8. Publicar `HeroSpawned` en `Start`. Test PlayMode: se publica una sola vez.
9. Crear un Canvas temporal con cuatro `OnScreenButton`. Verificar con Device Simulator en el editor que los botones mueven y atacan. La prueba en APK queda para SPEC 30, porque `LaneSandbox` no está en Build Settings (SPEC 02).

---

## Criterios de aceptación

- [x] El héroe se mueve a izquierda y derecha con teclado y con botones táctiles (Device Simulator).
- [x] El héroe no sale de los límites del carril.
- [x] La espada daña a un objetivo a 1.4 unidades y no a 2.
- [x] El látigo daña a un objetivo a 3.0 unidades.
- [x] Cada ataque respeta su cooldown y los cooldowns son independientes entre sí.
- [x] El héroe mirando a la izquierda o derecha ataca hacia ese lado.
- [x] El héroe en estado `Dead` no responde a la entrada.
- [x] `HeroSpawned` se publica una vez.
- [x] Los tests existentes de `AttackComponent` siguen en verde.

---

## Decisiones

- **Sí:** dos ataques desde el inicio. La historia fija espada corta y látigo para el nivel 1.
- **Sí:** extender `AttackComponent` en lugar de crear `HeroAttack`. Decisión de Nico, evita duplicar cooldown y reglas de equipo.
- **Sí:** Input System con acciones. Soporta táctil y teclado con el mismo código.
- **Sí:** `MoveLeft` y `MoveRight` como botones. `OnScreenButton` mapea un solo control, no un eje.
- **No:** joystick virtual. El carril es unidimensional, botones son más precisos.
- **No:** knockback en esta spec. Se evalúa cuando exista un sistema de empuje.
- **Sí:** héroe sin armadura. Calza con la historia del nivel 1.
- **Sí:** definición rápida sin aclaración detallada. Valores propuestos.

---

## Lo que **no** está en esta spec

- Agua bendita y milagros.
- Armadura, escudo, maza y ballesta.
- HUD definitivo.
- Derrota por muerte del héroe.
