# SPEC 07 — Base defendible

> **Estado:** Implementado
> **Depende de:** SPEC 03, SPEC 06
> **Fecha:** 2026-10-06
> **Objetivo:** Crear una estructura base configurable por nivel con barra de resistencia propia que publica `BaseDestroyed` al llegar a cero.

---

## Alcance

**Dentro:**

- `BaseDefinition` (ScriptableObject).
- `BaseStructure` (componente `IDamageable` del equipo Player).
- Etapas visuales de daño (intacta, dañada, casi destruida).
- Eventos `BaseResistanceChanged` y `BaseDestroyed`.
- Primera definición: `Base_CabinRuins` (ruinas de la cabaña).
- Los enemigos de SPEC 06 atacan la base al alcanzarla, a través de `TargetFinder`.

**Fuera de alcance (para specs futuras):**

- Defensas pasivas, como ballesteros en las almenas (SPEC 17).
- Reparación y mejoras de resistencia (SPEC 17).
- Bases de otros capítulos (SPEC 22, 23, 24).
- Pantalla de derrota y consumo de `BaseDestroyed` (SPEC 09).

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

public readonly struct BaseResistanceChanged
{
    public readonly int Current;
    public readonly int Max;
    public BaseResistanceChanged(int current, int max) { Current = current; Max = max; }
}

public readonly struct BaseDestroyed { }
```

La base se coloca en `LaneConfig.baseX`. `damageStages` debe tener exactamente 3 sprites; `OnValidate` de `BaseDefinition` escribe un warning si no.

Reglas de `BaseStructure`:

- Se inicializa con una `BaseDefinition`: resistencia actual igual a `maxResistance`. Al inicializar publica `BaseResistanceChanged` con el valor de partida, para que el HUD (SPEC 09) lo conozca. En `LaneSandbox` se coloca a mano en `baseX`.
- `TakeDamage` con `amount <= 0` se ignora. La resistencia no baja de 0.
- Con resistencia 0, `IsAlive` es `false` y todo daño posterior se ignora sin publicar eventos.
- `TakeDamage` ignora el daño cuya `source` sea nula o pertenezca al `Team.Player`. Es redundante con el filtro de equipo de `TargetFinder` y `AttackComponent`, pero protege la base de daño aliado directo.
- Publica `BaseResistanceChanged` en cada daño aceptado y `BaseDestroyed` una sola vez al llegar a 0.
- Los enemigos miden el rango de ataque hasta el centro de la base (`baseX`).

Etapas visuales (función estática pura `BaseStructure.GetStage(int current, int max)`, devuelve 0, 1 o 2):

- Etapa 0 (intacta): `current > 66%` de `max`.
- Etapa 1 (dañada): `current > 33%` y `current <= 66%`.
- Etapa 2 (casi destruida): `current <= 33%`, incluido 0.
- Con `max = 500`: 330 es etapa 1 y 165 es etapa 2. Los bordes son inclusivos hacia la etapa más dañada.

Objetivo de los enemigos: la base es un `IDamageable` del equipo Player, así que `TargetFinder` (SPEC 06) ya la considera. `EnemyBrain` no se modifica: ataca al objetivo más cercano en X, sea héroe, tropa o base. Para que el héroe tenga prioridad cuando está en rango, basta con que esté más cerca que la base.

---

## Plan de implementación

1. Crear `BaseDefinition` y el asset `Base_CabinRuins` con prefab placeholder.
2. Crear `BaseStructure` implementando `IDamageable` (equipo Player) con resistencia, sin FSM.
3. Publicar `BaseResistanceChanged` al inicializar y en cada daño, y `BaseDestroyed` al llegar a 0.
4. Implementar `GetStage` y el cambio de sprite por etapa.
5. Crear tests EditMode: umbrales de `GetStage` (330, 329, 165, 164, 0 con `max = 500`), `BaseDestroyed` una sola vez, daño ignorado de equipo Player, de `source` nula, de `amount <= 0` y después de destruida.
6. Colocar la base en `LaneSandbox` y verificar: un wargo llega a la base, la ataca vía `TargetFinder` y baja su resistencia.

---

## Criterios de aceptación

- [x] La base muestra resistencia inicial igual a `maxResistance` y publica `BaseResistanceChanged` al inicializar.
- [x] Un enemigo en rango ataca la base y baja su resistencia.
- [x] `BaseResistanceChanged` se publica en cada daño aceptado.
- [x] La etapa visual cambia al cruzar 66% y 33%.
- [x] Al llegar a 0 se publica `BaseDestroyed` una sola vez.
- [x] La base no acepta daño del equipo Player ni de una `source` nula.
- [x] La base destruida ignora todo daño posterior.
- [x] `EnemyBrain` no necesita cambios para atacar la base.

---

## Decisiones

- **Sí:** la base cambia por nivel mediante `BaseDefinition`. Decisión de Nico para encajar con la historia.
- **Sí:** primera base, ruinas de la cabaña, para el nivel 1.
- **Sí:** la base entra como objetivo por `TargetFinder`, sin cambios en `EnemyBrain`. Decisión de Nico, evita lógica duplicada.
- **No:** reparación durante el combate. Se evalúa con la tienda (SPEC 17).
- **No:** pantalla de derrota en esta spec. Esta spec solo publica `BaseDestroyed`.
- **Sí:** definición rápida sin aclaración detallada. Valor de 500 de resistencia propuesto.

---

## Lo que **no** está en esta spec

- Defensas pasivas y mejoras.
- Pantalla de derrota (SPEC 09).
- Bases de otros capítulos.
