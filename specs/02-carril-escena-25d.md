# SPEC 02 — Carril 2.5D y escena sandbox

> **Estado:** Borrador
> **Depende de:** SPEC 01
> **Fecha:** 2026-10-06
> **Objetivo:** Montar la escena `LaneSandbox` con un carril horizontal único, cámara ortográfica inclinada, sprites billboard sobre fondo 3D, parallax y lluvia con arte placeholder.

---

## Alcance

**Dentro:**

- Escena `Assets/_Project/Scenes/Sandbox/LaneSandbox.unity`.
- `LaneConfig` (ScriptableObject) con las medidas del carril.
- Cámara ortográfica inclinada con Cinemachine.
- Componente `BillboardSprite` para sprites que miran a la cámara.
- Componente `ParallaxLayer` para el fondo.
- Suelo y tres capas de fondo con arte placeholder (primitivas y sprites de color).
- Lluvia con Particle System.
- Iluminación fría (luna) y un punto de luz cálido (farol).

**Fuera de alcance (para specs futuras):**

- Héroe, enemigos y base (SPEC 05, 06, 07).
- Arte final de fondos y personajes.
- Niebla volumétrica y efectos de clima avanzados.
- Escenas de nivel reales (SPEC 12).

---

## Modelo de datos

```csharp
[CreateAssetMenu(menuName = "EC/Lane Config")]
public class LaneConfig : ScriptableObject
{
    public float laneHalfLength = 20f;
    public float baseX = -18f;
    public float spawnX = 18f;
    public float heroStartX = -12f;
    public float groundY = 0f;
    public float cameraPitch = 12f;
    public float cameraOrthoSize = 4.5f;
}
```

Convenciones:

- Eje X es el carril. La base queda a la izquierda y los enemigos entran por la derecha.
- Todas las entidades viven en `z = 0`. El fondo vive en `z > 0`.
- Unidades en metros de Unity.

Asset: `Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset`.

---

## Plan de implementación

1. Crear `LaneConfig` en `EC.Data` y el asset `LaneConfig_Default`. Verificar: el asset aparece en el menú `EC/Lane Config`.
2. Crear la escena `LaneSandbox` con un suelo (Plane escalado) y la cámara ortográfica. Verificar: Game view muestra el suelo con la inclinación de `cameraPitch`.
3. Agregar Cinemachine Camera ortográfica con límites de X según `LaneConfig`. Verificar: mover la cámara a mano no sale de los límites.
4. Crear `BillboardSprite` en `EC.Gameplay`. Verificar: un sprite de prueba rota para mirar a la cámara sin inclinarse en X.
5. Crear `ParallaxLayer` con factor por capa. Verificar: al mover la cámara en Play Mode, las tres capas se desplazan a velocidades distintas.
6. Agregar las tres capas de fondo (cielo y luna, árboles, siluetas de castillo) con placeholders.
7. Agregar lluvia (Particle System) con 800 partículas máximo y la iluminación fría más farol.
8. Reutilizar el Volume global de SPEC 01 en la escena. Verificar: Bloom y Vignette visibles.

---

## Criterios de aceptación

- [ ] `LaneSandbox` abre y entra en Play Mode con 0 errores en consola.
- [ ] La cámara es ortográfica con inclinación igual a `LaneConfig.cameraPitch`.
- [ ] La cámara no sale de `±laneHalfLength` en X.
- [ ] Un sprite con `BillboardSprite` siempre mira a la cámara.
- [ ] Las tres capas de parallax se mueven a velocidades distintas.
- [ ] La lluvia está activa y no supera 800 partículas.
- [ ] La escena corre a 60 FPS en el editor con Game view en 1920x1080.

---

## Decisiones

- **Sí:** sprites billboard sobre fondo 3D. Es el 2.5D elegido por Nico.
- **No:** modelos 3D para personajes. Costo de arte y animación.
- **Sí:** `LaneConfig` como ScriptableObject. Un solo lugar para medidas de carril, usado por specs 05, 06 y 07.
- **Sí:** inclinación de 12 grados (propuesta). Se ajusta con arte real.
- **Sí:** definición rápida sin aclaración detallada. Nico pidió generar las 30 specs de una vez; los valores propuestos se revisan antes de aprobar.

---

## Lo que **no** está en esta spec

- Héroe, enemigos, base y UI.
- Arte final.
- Niveles reales y flujo de escenas.
