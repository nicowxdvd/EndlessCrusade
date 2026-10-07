# SPEC 23 — Capítulo 3: El Bosque Maldito

> **Estado:** Borrador
> **Depende de:** SPEC 19, SPEC 20
> **Fecha:** 2026-10-06
> **Objetivo:** Crear los cuatro niveles del bosque maldito con el campamento como base y el Troll de Pantano Gigante como jefe.

---

## Alcance

**Dentro:**

- `ChapterDefinition` `ch3_bosque` con `lv_3_1` a `lv_3_4`.
- Base `Base_ForestCamp` (campamento).
- Entorno `Env_CursedForest` (árboles retorcidos, faroles, niebla, ojos rojos entre la maleza).
- Oleadas con licántropos, trolls de pantano, vampiros, duendes y murciélagos.
- Jefe `Boss_BogTrollGiant` con golpe de área y regeneración.
- Narrativa del capítulo (intro y outro).
- Entrega del equipo del capítulo (SPEC 21).

**Fuera de alcance (para specs futuras):**

- Capítulo 4 (SPEC 24).
- Arte final y audio final.

---

## Modelo de datos

Esta spec no introduce estructuras de datos nuevas.

Propuesta de niveles:

| Nivel | Enemigos principales | Oleadas | Notas |
| --- | --- | --- | --- |
| `lv_3_1` | licántropos, duendes | 6 | Aparece el Sacerdote |
| `lv_3_2` | trolls de pantano, goblins | 6 | Enemigos lentos y resistentes |
| `lv_3_3` | vampiros, murciélagos, licántropos | 7 | Mezcla aérea y terrestre |
| `lv_3_4` | mezcla más Troll Gigante | 7 | Jefe |

Troll de Pantano Gigante: vida 1400, regenera 5 por segundo, golpe de área de radio 3 y daño 45 con aviso de 1.2 s, velocidad 0.9. Se vuelve vulnerable (no regenera) 4 segundos tras cada golpe.

Narrativa propuesta (a confirmar por Nico): el rastro del bebé cruza el bosque maldito. El héroe monta un campamento para resistir las noches. Un troll gigante guarda el único paso hacia la fortaleza. Al vencerlo, el héroe recupera escudo y maza, escondidos en el campamento de la Orden.

---

## Plan de implementación

1. Crear `Base_ForestCamp` y `Env_CursedForest`.
2. Crear `Boss_BogTrollGiant` y probarlo en `LaneSandbox`.
3. Crear `lv_3_1` a `lv_3_3`.
4. Crear `lv_3_4` con el jefe.
5. Crear las `StorySequence`.
6. Crear `ch3_bosque` y enlazarlo en `CampaignDefinition`. Verificar: desbloqueo tras `lv_2_4` y entrega de escudo, maza y ballesta.

---

## Criterios de aceptación

- [ ] `lv_3_1` está bloqueado hasta completar `lv_2_4`.
- [ ] Los cuatro niveles son jugables de principio a fin.
- [ ] La base es el campamento en todo el capítulo.
- [ ] El jefe deja de regenerar durante 4 segundos tras cada golpe de área.
- [ ] Al completar `lv_3_4` se entregan escudo, maza, ballesta y Juicio Divino.
- [ ] Sacerdote y Paladín están disponibles desde `lv_3_1`.
- [ ] Las historias de intro y outro se muestran.

---

## Decisiones

- **Sí:** campamento como base. Decisión de Nico.
- **Sí:** jefe Troll de Pantano Gigante. Figura en el documento técnico como jefe final posible.
- **Sí:** ventana de vulnerabilidad del jefe. Evita que la regeneración lo vuelva inmatable para un jugador con poco daño.
- **Sí:** definición rápida sin aclaración detallada. Valores y narrativa propuestos.

---

## Lo que **no** está en esta spec

- Capítulo 4, arte final y audio final.
