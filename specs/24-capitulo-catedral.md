# SPEC 24 — Capítulo 4: La Catedral

> **Estado:** Borrador
> **Depende de:** SPEC 22, SPEC 23
> **Fecha:** 2026-10-06
> **Objetivo:** Crear los cinco niveles finales con el portón del monasterio como base y el Señor Vampiro como jefe final, con epílogo.

---

## Alcance

**Dentro:**

- `ChapterDefinition` `ch4_catedral` con `lv_4_1` a `lv_4_5`.
- Base `Base_MonasteryGate` (portón del monasterio).
- Entorno `Env_Cathedral` (catedral negra, vitrales rojos, gárgolas, cielo con luna).
- Oleadas con todos los enemigos del juego.
- Jefe final `Boss_VampireLord` en tres fases.
- Narrativa final (intro, outro y epílogo con el rescate del bebé).

**Fuera de alcance (para specs futuras):**

- Modo infinito o dificultades adicionales.
- Arte final y audio final.

---

## Modelo de datos

Esta spec no introduce estructuras de datos nuevas.

Propuesta de niveles:

| Nivel | Enemigos principales | Oleadas | Notas |
| --- | --- | --- | --- |
| `lv_4_1` | vampiros, goblins | 7 | Asedio al monasterio |
| `lv_4_2` | licántropos, murciélagos | 7 | |
| `lv_4_3` | trolls, vampiros | 8 | |
| `lv_4_4` | todos los tipos | 8 | Prueba final antes del jefe |
| `lv_4_5` | refuerzos más Señor Vampiro | 4 | Jefe final |

Señor Vampiro: vida 2500, tres fases.

- Fase 1 (100% a 66%): espada con tajo en arco (daño 40) y orbe de sangre a distancia (daño 25, proyectil pooleado).
- Fase 2 (66% a 33%): salto (blink) cada 5 s e invoca 4 vampiros y 6 murciélagos cada 20 s.
- Fase 3 (33% a 0%): velocidad x1.3 y orbe doble.

Narrativa propuesta (a confirmar por Nico): el Señor Vampiro asedia el monasterio donde la Orden resiste. El héroe sostiene el portón hasta que sus aliados abren paso. Entra a la catedral, vence al Señor Vampiro y rescata al Bebé de la Profecía. El epílogo cierra la redención del Templario.

---

## Plan de implementación

1. Crear `Base_MonasteryGate` y `Env_Cathedral`.
2. Crear `lv_4_1` a `lv_4_4`.
3. Crear `Boss_VampireLord` con las tres fases y probarlo en `LaneSandbox`.
4. Crear `lv_4_5`.
5. Crear las `StorySequence` de intro, outro y epílogo.
6. Crear `ch4_catedral` y enlazarlo. Verificar: desbloqueo tras `lv_3_4` y epílogo al completar `lv_4_5`.

---

## Criterios de aceptación

- [ ] `lv_4_1` está bloqueado hasta completar `lv_3_4`.
- [ ] Los cinco niveles son jugables de principio a fin.
- [ ] La base es el portón del monasterio en todo el capítulo.
- [ ] El jefe cambia de fase al 66% y 33% de vida.
- [ ] El epílogo se muestra una sola vez, al completar `lv_4_5` por primera vez.
- [ ] Completar `lv_4_5` deja la campaña completa en el mapa.

---

## Decisiones

- **Sí:** portón del monasterio como base. Figura en el documento técnico como estructura a defender.
- **Sí:** jefe final con tres fases y orbe de sangre. Aprovecha el diseño visual del Señor Vampiro de las imágenes de referencia.
- **Sí:** narrativa propuesta. La historia original no cubre este tramo.
- **Sí:** definición rápida sin aclaración detallada.

---

## Lo que **no** está en esta spec

- Modo infinito, dificultades extra, arte final y audio final.
