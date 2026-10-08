# SPEC 19 — Tropas adicionales

> **Estado:** Aprobado
> **Depende de:** SPEC 13, SPEC 16
> **Fecha:** 2026-10-06
> **Objetivo:** Agregar Campesino, Ballestero, Sacerdote y Paladín como tropas invocables con desbloqueo por capítulo.

---

## Alcance

**Dentro:**

- Cuatro `TroopDefinition` nuevas y sus prefabs.
- Comportamientos propios: ataque a distancia, curación en área, resistencia alta.
- Proyectil `Bolt` pooleado para el Ballestero.
- Desbloqueo por `TroopDefinition.unlockChapterId` según el progreso.
- La barra de invocación muestra solo las tropas desbloqueadas.

**Fuera de alcance (para specs futuras):**

- Mejoras de nivel por tropa (ya cubiertas en SPEC 17, solo se agregan los assets de cada tropa).
- Tropas en el nivel 1 (no hay tropas).
- Arte final.

---

## Modelo de datos

Valores propuestos.

| Tropa | Vida | Daño | Alcance | Costo de Fe | Cooldown | Desbloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| Escudero (SPEC 13) | 120 | 10 | 1.2 | 30 | 6 | inicio |
| Campesino | 50 | 6 | 1.0 | 15 | 3 | `ch2_aldea` |
| Ballestero | 60 | 14 | 8.0 | 45 | 8 | `ch2_aldea` |
| Sacerdote | 70 | 0 (cura 10 por 2 s, radio 4) | 4.0 | 60 | 14 | `ch3_bosque` |
| Paladín | 400 | 22 | 1.4 | 90 | 20 | `ch3_bosque` |

El desbloqueo se evalúa con `ProgressData.completedLevels`: la tropa se desbloquea al entrar a su capítulo, es decir, al completar el último nivel del capítulo anterior (`lv_1_1` para `ch2_aldea` y `lv_2_4` para `ch3_bosque`).

Nuevos componentes en `EC.Gameplay`: `RangedAttackComponent` y `HealAuraComponent`.

---

## Plan de implementación

1. Crear las cuatro `TroopDefinition` y prefabs placeholder.
2. Crear `RangedAttackComponent` y el proyectil `Bolt` pooleado. Verificar: el Ballestero dispara desde 8 unidades.
3. Crear `HealAuraComponent` con test EditMode de intervalo y tope de vida.
4. Crear la regla de desbloqueo y test EditMode.
5. Mostrar solo las tropas desbloqueadas en la barra de invocación.
6. Probar combate con todas las tropas en `Level_Test` con un progreso simulado.

---

## Criterios de aceptación

- [ ] Cada tropa tiene vida, daño, alcance, costo y cooldown iguales a la tabla.
- [ ] El Ballestero ataca desde su alcance sin acercarse.
- [ ] El Sacerdote cura a aliados heridos en radio 4 cada 2 segundos y nunca sobre su vida máxima.
- [ ] El Sacerdote no cura enemigos ni a sí mismo en exceso.
- [ ] Una tropa bloqueada no aparece en la barra.
- [ ] Al completar el último nivel del capítulo anterior, la tropa aparece.
- [ ] El pool no asigna memoria por frame con las cuatro tropas activas.

---

## Decisiones

- **Sí:** desbloquear tropas por capítulo. Da sensación de avance, igual que el equipo del héroe.
- **Sí:** Sacerdote sin ataque, rol de apoyo.
- **Sí:** definición rápida sin aclaración detallada. Tabla propuesta, se balancea en SPEC 29.

---

## Lo que **no** está en esta spec

- Mejoras de tropas, arte final y niveles de capítulos.
