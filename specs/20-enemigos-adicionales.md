# SPEC 20 — Enemigos adicionales

> **Estado:** Borrador
> **Depende de:** SPEC 06
> **Fecha:** 2026-10-06
> **Objetivo:** Agregar vampiro, goblin, duende, licántropo y troll de pantano con comportamientos propios mediante módulos reutilizables.

---

## Alcance

**Dentro:**

- Cinco `EnemyDefinition` nuevas con prefabs placeholder.
- Módulos de comportamiento en `EC.Gameplay`: `RegenerationModule`, `BlinkModule`, `DodgeModule`, `LifestealModule`, `RageModule`.
- `EnemyBrain` consulta los módulos presentes en el prefab.
- Etiquetas de criatura (`CreatureTag`) correctas por enemigo.

**Fuera de alcance (para specs futuras):**

- Jefes de capítulo (SPEC 22, 23, 24).
- Niveles y oleadas que usen estos enemigos (SPEC 22, 23, 24).
- Arte final.

---

## Modelo de datos

Valores propuestos.

| Enemigo | Vida | Daño | Velocidad | Etiquetas | Módulo |
| --- | --- | --- | --- | --- | --- |
| Goblin | 25 | 5 | 3.0 | None | Aparece en grupos de 5 (definido en la oleada) |
| Duende | 20 | 7 | 4.0 | None | `DodgeModule` 25% de esquiva |
| Vampiro | 90 | 15 | 2.2 | Undead | `BlinkModule` salta 4 unidades cada 6 s, `LifestealModule` 30% |
| Licántropo | 150 | 20 | 2.8 | Beast | `RageModule` +40% de velocidad bajo 50% de vida |
| Troll de pantano | 400 | 30 | 1.2 | None | `RegenerationModule` 3 de vida por segundo |

Interfaz común:

```csharp
public interface IEnemyModule
{
    void Initialize(EnemyBrain brain);
    void Tick(float deltaTime);
}
```

---

## Plan de implementación

1. Crear `IEnemyModule` y permitir que `EnemyBrain` los descubra y ejecute.
2. Crear `RegenerationModule` y test EditMode (no supera la vida máxima).
3. Crear `DodgeModule` con una fuente aleatoria inyectable para tests deterministas.
4. Crear `BlinkModule` y `LifestealModule`.
5. Crear `RageModule`.
6. Crear las cinco `EnemyDefinition` y prefabs, con las etiquetas de `CreatureTag`.
7. Probar cada enemigo en `LaneSandbox`.

---

## Criterios de aceptación

- [ ] Los cinco enemigos existen con las estadísticas de la tabla.
- [ ] El troll regenera 3 de vida por segundo y no supera su máximo.
- [ ] El duende esquiva cerca del 25% de los golpes en una prueba de 1000 impactos (entre 22% y 28%).
- [ ] El vampiro se desplaza 4 unidades cada 6 segundos y se cura el 30% del daño que causa.
- [ ] El licántropo aumenta su velocidad un 40% al bajar del 50% de vida.
- [ ] El agua bendita inflige daño extra al vampiro (etiqueta `Undead`).
- [ ] Los enemigos nuevos usan el pool sin asignar memoria por frame.

---

## Decisiones

- **Sí:** módulos componibles en lugar de una clase por enemigo. Coincide con la arquitectura basada en componentes del documento técnico.
- **Sí:** el goblin se diferencia por cantidad, no por mecánica. Controla la presión con oleadas.
- **Sí:** definición rápida sin aclaración detallada. Valores propuestos.

---

## Lo que **no** está en esta spec

- Jefes, oleadas de capítulos y arte final.
