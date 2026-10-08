# SPEC 22 — Capítulo 2: La Aldea

> **Estado:** Implementado
> **Depende de:** SPEC 19, SPEC 20
> **Fecha:** 2026-10-06
> **Objetivo:** Crear los cuatro niveles del capítulo de la aldea con la capilla como base y un jefe propio.

---

## Alcance

**Dentro:**

- `ChapterDefinition` `ch2_aldea` con `lv_2_1` a `lv_2_4`.
- Base `Base_VillageChapel` (capilla de la aldea).
- Entorno `Env_Village` (calles empedradas, casas góticas, luna, castillo al fondo).
- Oleadas con wargos, murciélagos, goblins, duendes y vampiros.
- Jefe `Boss_VampireCaptain`: se desplaza con salto (blink) e invoca murciélagos.
- Narrativa del capítulo en `StorySequence` (intro y outro).
- Recompensas de nivel y entrega de equipo del capítulo (SPEC 21).

**Fuera de alcance (para specs futuras):**

- Otros capítulos (SPEC 23, 24).
- Arte final y audio final.

---

## Modelo de datos

Esta spec no introduce estructuras de datos nuevas. Reutiliza `LevelDefinition`, `WaveDefinition`, `BossDefinition` y `StorySequence`.

Propuesta de niveles:

| Nivel | Enemigos principales | Oleadas | Notas |
| --- | --- | --- | --- |
| `lv_2_1` | wargos, goblins | 5 | Se habilita la invocación de tropas |
| `lv_2_2` | murciélagos, duendes | 6 | Enemigos rápidos |
| `lv_2_3` | goblins, vampiros | 6 | Primer vampiro |
| `lv_2_4` | mezcla más Capitán Vampiro | 7 | Jefe |

Narrativa propuesta (a confirmar por Nico): el héroe llega a la aldea sitiada. Los aldeanos se refugian en la capilla. Un capitán de las criaturas sabe hacia dónde llevaron al bebé. Al vencerlo, el héroe recupera su armadura templaria, guardada en la capilla, y sigue al bosque maldito.

El Capitán Vampiro: vida 700, daño 25, salto cada 6 s, invoca 3 murciélagos cada 15 s.

---

## Plan de implementación

1. Crear `Base_VillageChapel` y `Env_Village` con placeholders. Verificar: carga en `Level.unity`.
2. Crear `Boss_VampireCaptain` y probarlo en `LaneSandbox`.
3. Crear `lv_2_1` y probarlo hasta victoria.
4. Crear `lv_2_2` y `lv_2_3`.
5. Crear `lv_2_4` con el jefe.
6. Crear las `StorySequence` de intro y outro.
7. Crear `ch2_aldea` y enlazarlo en `CampaignDefinition`. Verificar: desbloqueo secuencial en el mapa y entrega de la armadura al final.

---

## Criterios de aceptación

- [ ] `lv_2_1` está bloqueado hasta completar `lv_1_1`.
- [ ] Los cuatro niveles son jugables de principio a fin.
- [ ] La base es la capilla en todos los niveles del capítulo.
- [ ] `lv_2_4` termina con un jefe que invoca murciélagos.
- [ ] Al completar `lv_2_4` se entrega la armadura y Bendición.
- [ ] Campesino y Ballestero están disponibles desde `lv_2_1`.
- [ ] Las historias de intro y outro se muestran.

---

## Decisiones

- **Sí:** la capilla como base del capítulo. Decisión de Nico: la base cambia por nivel.
- **Sí:** narrativa y jefe propuestos por esta spec. La historia original solo detalla el nivel 1.
- **Sí:** cuatro niveles por capítulo. Longitud razonable para un primer lanzamiento.
- **Sí:** definición rápida sin aclaración detallada.

---

## Lo que **no** está en esta spec

- Capítulos 3 y 4, arte final y audio final.
