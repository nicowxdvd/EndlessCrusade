# SPEC 29 — Optimización móvil

> **Estado:** Aprobado
> **Depende de:** SPEC 24
> **Fecha:** 2026-10-06
> **Objetivo:** Perfilar memoria y tiempo de frame en un dispositivo Android de gama baja y lograr 60 FPS estables con niveles de calidad automáticos.

---

## Alcance

**Dentro:**

- Dispositivo de referencia de gama baja (definido por Nico antes de empezar).
- Nivel de estrés `Level_Stress` con 60 enemigos y 8 tropas simultáneas.
- Perfilado con Unity Profiler y Memory Profiler conectados por USB.
- Niveles de calidad `Low`, `Medium` y `High` con selección automática por hardware.
- Ajustes: resolución de render, MSAA, sombras, post-procesado, partículas.
- Revisión de asignaciones de memoria por frame y de tamaño de texturas.
- Balance de economía y dificultad con datos de partidas reales.

**Fuera de alcance (para specs futuras):**

- Optimización para iOS.
- Compresión avanzada de arte (se hace al llegar el arte final).
- Soporte de dispositivos anteriores a Android 8.

---

## Modelo de datos

```csharp
public enum QualityTier { Low, Medium, High }
```

Objetivos medibles propuestos, sobre el dispositivo de referencia en `Level_Stress`:

| Métrica | Objetivo |
| --- | --- |
| Frame time p95 | 16.6 ms o menos |
| Asignaciones de memoria en combate | 0 bytes por frame |
| Memoria total de la app | 800 MB o menos |
| Tiempo de carga de `Level.unity` | 4 s o menos |
| Temperatura y batería | 15 minutos de juego sin estrangulamiento térmico |

Perfiles de calidad:

| Perfil | Escala de render | MSAA | Post-procesado |
| --- | --- | --- | --- |
| High | 1.0 | 4x | Bloom, Vignette, Color Adjustments |
| Medium | 0.85 | 2x | Vignette, Color Adjustments |
| Low | 0.7 | Off | Color Adjustments |

---

## Plan de implementación

1. Nico define el dispositivo de referencia y lo documenta en `docs/` (no versionado).
2. Crear `Level_Stress` y un script de edición para lanzarlo.
3. Medir línea base con Profiler y registrar los valores en una tabla en la spec al implementarla.
4. Crear `QualityTier`, tres URP Assets y un `QualityBootstrapper` que elige por `SystemInfo` (memoria, nivel de gráficos) con opción manual en ajustes.
5. Eliminar asignaciones por frame detectadas (cadenas, LINQ, eventos).
6. Reducir memoria: compresión de texturas ASTC, atlas de sprites, descarga de assets no usados.
7. Optimizar partículas y luces según el perfil.
8. Rebalancear economía y dificultad con las tablas de SPEC 15, 17 y 19.
9. Repetir la medición y registrar los resultados finales.

---

## Criterios de aceptación

- [ ] `Level_Stress` corre con frame time p95 de 16.6 ms o menos en el dispositivo de referencia.
- [ ] El Profiler muestra 0 bytes asignados por frame durante 60 segundos de combate.
- [ ] La memoria total no supera 800 MB en el Memory Profiler.
- [ ] El perfil `Low` se elige automáticamente en dispositivos con 3 GB de RAM o menos.
- [ ] El jugador puede forzar el perfil de calidad en ajustes.
- [ ] 15 minutos de juego continuo no provocan caída de FPS por temperatura.
- [ ] Los resultados quedan registrados en la spec al implementarla.

---

## Decisiones

- **Sí:** perfiles de calidad automáticos. Nico prioriza el aspecto visual: High queda como valor por defecto en equipos capaces y solo se baja donde hace falta.
- **No:** bajar la calidad global para todos. Sacrifica el look que se busca.
- **Sí:** medir en dispositivo real. El editor y el emulador no son representativos.
- **Sí:** definición rápida sin aclaración detallada. Objetivos propuestos.

---

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| No hay dispositivo de gama baja disponible | Nico define uno antes del paso 1, o se compra uno usado. Sin él no se cumplen los criterios. |
| El arte final pesa más que los placeholders | Repetir mediciones al integrar el arte final. |

---

## Lo que **no** está en esta spec

- iOS, compresión avanzada de arte final y Android anterior a 8.
