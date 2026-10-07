# SPEC 05 — Héroe Templario

> **Estado:** Borrador
> **Depende de:** SPEC 02, SPEC 03
> **Fecha:** 2026-10-06
> **Objetivo:** Controlar al Templario con botones táctiles de movimiento horizontal y dos ataques, espada corta y látigo.

---

## Alcance

**Dentro:**

- `HeroDefinition` (ScriptableObject) con estadísticas y dos ataques.
- Prefab `Hero` con los componentes de SPEC 03.
- `HeroController` que lee Input System y alimenta la FSM.
- Asset `Controls.inputactions` con acciones `Move`, `AttackSword` y `AttackWhip`.
- Controles táctiles con `OnScreenButton` (izquierda, derecha, espada, látigo).
- Teclado para el editor: A y D para mover, J espada, K látigo.
- Evento `HeroSpawned`.
- Prefab `TrainingDummy` para probar daño.

**Fuera de alcance (para specs futuras):**

- Habilidades con cooldown como agua bendita (SPEC 10).
- Equipo adicional: armadura, escudo, maza, ballesta (SPEC 21).
- HUD definitivo (SPEC 09).
- Animaciones finales.

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
    public float knockback;
}

[CreateAssetMenu(menuName = "EC/Hero Definition")]
public class HeroDefinition : UnitDefinition
{
    public MeleeAttackDefinition sword;
    public MeleeAttackDefinition whip;
}

public readonly struct HeroSpawned { public readonly GameObject Hero; }
```

Valores propuestos del nivel 1 (héroe sin armadura): vida 120, velocidad 3.0. Espada: daño 18, alcance 1.4, cooldown 0.5. Látigo: daño 10, alcance 3.0, cooldown 0.9.

---

## Plan de implementación

1. Crear `HeroDefinition` y el asset `Hero_Templar_Unarmored`.
2. Crear `Controls.inputactions` con las tres acciones y bindings de teclado.
3. Crear el prefab `Hero` con sprite placeholder, `BillboardSprite`, `HealthComponent`, `MovementComponent` y `HeroController`.
4. Implementar movimiento en `HeroController`. Verificar: A y D mueven al héroe en `LaneSandbox` y respeta límites.
5. Implementar ataque de espada (daño en rango frontal, cooldown). Verificar: el `TrainingDummy` pierde vida.
6. Implementar ataque de látigo con mayor alcance. Verificar: golpea al dummy a 3 unidades y la espada no.
7. Crear un Canvas temporal con cuatro `OnScreenButton`. Verificar: en el APK los botones mueven y atacan.
8. Publicar `HeroSpawned` en `Start`.

---

## Criterios de aceptación

- [ ] El héroe se mueve a izquierda y derecha con teclado y con botones táctiles.
- [ ] El héroe no sale de los límites del carril.
- [ ] La espada daña a un objetivo a 1.4 unidades y no a 2.
- [ ] El látigo daña a un objetivo a 3.0 unidades.
- [ ] Cada ataque respeta su cooldown.
- [ ] El héroe mirando a la izquierda o derecha ataca hacia ese lado.
- [ ] El héroe en estado `Dead` no responde a la entrada.
- [ ] `HeroSpawned` se publica una vez.

---

## Decisiones

- **Sí:** dos ataques desde el inicio. La historia fija espada corta y látigo para el nivel 1.
- **Sí:** Input System con acciones. Soporta táctil y teclado con el mismo código.
- **No:** joystick virtual. El carril es unidimensional, botones son más precisos.
- **Sí:** héroe sin armadura. Calza con la historia del nivel 1.
- **Sí:** definición rápida sin aclaración detallada. Valores propuestos.

---

## Lo que **no** está en esta spec

- Agua bendita y milagros.
- Armadura, escudo, maza y ballesta.
- HUD definitivo.
