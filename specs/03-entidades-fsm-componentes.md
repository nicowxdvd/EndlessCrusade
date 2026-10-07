# SPEC 03 — Entidades, FSM y componentes

> **Estado:** Implementado
> **Depende de:** SPEC 01
> **Fecha:** 2026-10-06
> **Objetivo:** Crear los componentes `HealthComponent`, `AttackComponent` y `MovementComponent`, una FSM genérica con los estados Idle, Move, Attack, Hurt y Dead, y el ScriptableObject `UnitDefinition` con las estadísticas.

---

## Alcance

**Dentro:**

- FSM genérica en `EC.Core`.
- Enum `EntityState` con Idle, Move, Attack, Hurt, Dead.
- Interfaz `IDamageable` y enum `Team`.
- `HealthComponent`, `AttackComponent`, `MovementComponent` en `EC.Gameplay`.
- `EntityController` en `EC.Gameplay`: dueño de la `StateMachine` de cada entidad.
- `DummyBrain` en `EC.Gameplay`: IA mínima solo para el sandbox.
- `UnitDefinition` (ScriptableObject) en `EC.Data`.
- `EventBus` genérico en `EC.Core` y los eventos `HealthChanged` y `EntityDied`.
- Tests EditMode de la FSM y de `HealthComponent`.

**Fuera de alcance (para specs futuras):**

- Héroe, enemigos y tropas concretos (SPEC 05, 06, 13).
- IA real de enemigos y tropas (SPEC 06).
- Reinicio de `HealthComponent` al reusar desde el pool: se conecta con `IPoolable` en SPEC 06.
- Animaciones.
- Pooling (SPEC 04).
- Habilidades y efectos de estado (SPEC 10, 11).

---

## Modelo de datos

```csharp
public enum Team { Player, Enemy }
public enum EntityState { Idle, Move, Attack, Hurt, Dead }

public interface IDamageable
{
    Team Team { get; }
    bool IsAlive { get; }
    void TakeDamage(int amount, GameObject source);
}

public interface IState
{
    void Enter();
    void Tick(float deltaTime);
    void Exit();
}

public readonly struct HealthChanged
{
    public readonly GameObject Source;
    public readonly int Current;
    public readonly int Max;
    public HealthChanged(GameObject source, int current, int max) { Source = source; Current = current; Max = max; }
}

public readonly struct EntityDied
{
    public readonly GameObject Source;
    public EntityDied(GameObject source) { Source = source; }
}

[CreateAssetMenu(menuName = "EC/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Team team;
    public int maxHealth = 100;
    public float moveSpeed = 2f;
    public int attackDamage = 10;
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;
    public float hurtDuration = 0.25f;
    public GameObject prefab;
}
```

Clase `StateMachine` en `EC.Core` con `Register(EntityState, IState)`, `Transition(EntityState)` y `Tick(float)`.

`EventBus<T>` estático con `Subscribe(Action<T>)`, `Unsubscribe(Action<T>)` y `Publish(T)`. Limpia sus suscriptores con `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`, para que no sobrevivan si el Domain Reload está desactivado.

`EntityController` (MonoBehaviour) crea la `StateMachine`, registra los cinco estados y llama a `Tick(Time.deltaTime)` en `Update`. Reglas de transición:

- `HealthComponent` recibe daño no letal: transición a `Hurt`. Tras `hurtDuration` vuelve a `Idle`.
- `HealthComponent` llega a 0: transición a `Dead`. `Dead` es terminal y no acepta más transiciones.
- `MovementComponent` y `AttackComponent` solo actúan si el estado es `Move` o `Attack` respectivamente.

`DummyBrain` (MonoBehaviour, solo sandbox): cada frame busca el `IDamageable` vivo de otro `Team` más cercano en X. Si está fuera de `attackRange`, pide `Move` hacia él. Si está dentro, pide `Attack`. Sin objetivo, pide `Idle`.

---

## Plan de implementación

1. Crear `Team`, `EntityState`, `IDamageable`, `IState` y `StateMachine` en `EC.Core`. Verificar: compila.
2. Crear `EventBus<T>` y los structs `HealthChanged` y `EntityDied`. Verificar: compila.
3. Crear tests EditMode para `StateMachine` (transición válida, Exit antes de Enter, estado Dead terminal). Verificar: tests en verde.
4. Crear `UnitDefinition` en `EC.Data` y el asset `Unit_TestDummy`.
5. Crear `HealthComponent` (implementa `IDamageable`, publica eventos) y `EntityController`. Test EditMode: daño letal publica `EntityDied` una sola vez.
6. Crear `MovementComponent` (mueve en X con `moveSpeed`, respeta límites de `LaneConfig`).
7. Crear `AttackComponent` (cooldown, rango, aplica daño a `IDamageable` de otro `Team`).
8. Crear `DummyBrain` y el prefab `TestDummy` con los tres componentes, `EntityController` y `DummyBrain` en `LaneSandbox`. Verificar: en Play Mode, dos dummies de equipos distintos se acercan y se matan.

---

## Criterios de aceptación

- [ ] Los tests EditMode de FSM y `HealthComponent` pasan.
- [ ] `HealthComponent.TakeDamage` no baja de 0 y publica `EntityDied` una sola vez.
- [ ] Una entidad en estado `Dead` no ataca ni se mueve.
- [ ] `AttackComponent` respeta `attackCooldown`.
- [ ] `MovementComponent` no sale de los límites de `LaneConfig`.
- [ ] Dos dummies de equipos opuestos en `LaneSandbox` se atacan hasta que uno muere.
- [ ] `EC.Core` no referencia ningún otro assembly `EC.*`.
- [x] Un golpe no letal lleva a `Hurt` y vuelve a `Idle` tras `hurtDuration`.
- [x] `EventBus<T>` no conserva suscriptores tras reiniciar el dominio o entrar a Play Mode.

---

## Decisiones

- **Sí:** FSM propia y liviana. El documento técnico fija Idle, Move, Attack, Hurt, Dead.
- **No:** paquete de FSM de terceros. Dependencia innecesaria.
- **Sí:** `EventBus<T>` estático en `EC.Core`. Permite que `EC.UI` y `EC.Services` escuchen eventos sin referenciar `EC.Gameplay`.
- **Sí:** `UnitDefinition` como base de héroe, enemigos y tropas.
- **Sí:** `EntityController` como único dueño de la FSM. Evita que cada componente cambie de estado por su cuenta.
- **Sí:** `DummyBrain` desechable. La IA real se diseña en SPEC 06.
- **Sí:** definición rápida sin aclaración detallada. Los valores por defecto de `UnitDefinition` son propuestos.

---

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| `EventBus` estático retiene suscriptores entre escenas | Todo suscriptor se desuscribe en `OnDisable`. El bus se limpia en `SubsystemRegistration`. |

---

## Lo que **no** está en esta spec

- Entidades concretas con comportamiento (héroe, enemigos, tropas).
- Animaciones y arte.
- Pooling.
