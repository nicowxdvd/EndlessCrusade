# SPEC 02 — Carril 2.5D y escena sandbox

> **Estado:** Implementado
> **Depende de:** SPEC 01
> **Fecha:** 2026-10-06
> **Objetivo:** Montar la escena `LaneSandbox` con un carril horizontal único, cámara ortográfica inclinada, sprites billboard sobre fondo 3D, parallax y lluvia con arte placeholder.

---

## Alcance

**Dentro:**

- Escena `Assets/_Project/Scenes/Sandbox/LaneSandbox.unity`.
- `LaneConfig` (ScriptableObject) con las medidas del carril.
- Cámara ortográfica inclinada con Cinemachine 3 (`CinemachineCamera`).
- Extensión de Cinemachine que limita la cámara en X según `LaneConfig` y su semiancho visible.
- Componente `BillboardSprite` para sprites que copian la rotación de la cámara.
- Componente `ParallaxLayer` para el fondo.
- Script de prueba `SandboxCameraDriver` que mueve el objeto seguido por la cámara con el teclado.
- Suelo y tres capas de fondo con arte placeholder (primitivas y sprites de color).
- Lluvia con Particle System hija de la cámara.
- Iluminación fría (luna) y un punto de luz cálido (farol).
- Volume global con valores finos de Bloom, Vignette y Color Adjustments.

**Fuera de alcance (para specs futuras):**

- Héroe, enemigos y base (SPEC 05, 06, 07).
- Arte final de fondos y personajes.
- Niebla volumétrica y efectos de clima avanzados.
- Escenas de nivel reales (SPEC 12).
- Agregar `LaneSandbox` a `Build Settings`. La escena es solo de desarrollo.
- Sprites iluminados (shader Lit para sprites).

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
    public float laneDepth = 6f;
    public float cameraPitch = 12f;
    public float cameraOrthoSize = 4.5f;
}
```

Convenciones:

- Eje X es el carril. La base queda a la izquierda y los enemigos entran por la derecha.
- Todas las entidades viven en `z = 0`. El fondo vive en `z > 0`.
- Unidades en metros de Unity.
- El suelo es plano en `y = groundY` y mide `2 * laneHalfLength` en X por `laneDepth` en Z. Lo que se inclina es la cámara (`cameraPitch` en el eje X), no el suelo.

Límite de cámara en X:

- Semiancho visible = `cameraOrthoSize * aspect`. En 16:9 con `cameraOrthoSize = 4.5` vale 8 m.
- Rango del centro de la cámara = `±(laneHalfLength - semiancho visible)`.
- El semiancho se recalcula cuando cambia el aspect ratio.

Billboard:

- `BillboardSprite` copia `rotation` de la cámara principal en `LateUpdate`. El sprite se inclina con la cámara y no se deforma.

Parallax:

| Capa | Contenido | Factor |
| --- | --- | --- |
| Lejana | Cielo y luna | 0.2 |
| Media | Siluetas de castillo | 0.5 |
| Cercana | Árboles | 0.8 |

- Cada capa se desplaza `cameraDeltaX * factor` en X. Un factor más alto sigue más a la cámara, así que se ve más lenta en pantalla.
- El ancho de cada capa cubre `2 * laneHalfLength` más el rango de cámara, para que nunca se vea el borde.

Materiales:

- Sprites con `Sprite-Unlit-Default` y tinte de color. La luna y el farol iluminan solo el suelo y las primitivas.

Volume global (`GlobalVolumeProfile.asset` de SPEC 01):

| Efecto | Parámetros |
| --- | --- |
| Bloom | Threshold 0.9, Intensity 0.6, Scatter 0.6 |
| Vignette | Intensity 0.35, Smoothness 0.4 |
| Color Adjustments | Saturation -15, Post Exposure -0.2 |

Los valores son punto de partida y se ajustan a ojo con arte real.

Asset: `Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset`.

---

## Plan de implementación

1. Agregar `Unity.Cinemachine` a las referencias de `EC.Gameplay.asmdef`. Crear `LaneConfig` en `EC.Data`, el asset `LaneConfig_Default` y las carpetas `ScriptableObjects/Lane` y `Scenes/Sandbox` con `.gitkeep` si quedan vacías. Verificar: el asset aparece en el menú `EC/Lane Config` y el proyecto compila sin errores.
2. Crear la escena `LaneSandbox` con un suelo plano (Plane escalado según `laneHalfLength` y `laneDepth`) y la cámara ortográfica con `cameraPitch` en el eje X. Verificar: Game view muestra el suelo visto desde arriba con 12 grados de inclinación.
3. Agregar `CinemachineCamera` ortográfica con `Follow` a un objeto objetivo y la extensión de límites que calcula el rango de X según `LaneConfig` y el semiancho visible. Crear `SandboxCameraDriver` para mover el objetivo con el teclado. Verificar: en Play Mode, el centro de la cámara no sale de `±(laneHalfLength - semiancho visible)` y el borde del suelo no deja ver vacío.
4. Crear `BillboardSprite` en `EC.Gameplay`. Verificar: un sprite de prueba mantiene `Quaternion.Angle` con la cámara menor a 0.1° mientras la cámara se mueve.
5. Crear `ParallaxLayer` con factor por capa. Verificar: al mover la cámara en Play Mode, las tres capas se desplazan a velocidades distintas.
6. Agregar las tres capas de fondo según la tabla de parallax, con placeholders unlit. Verificar: no se ve el borde de ninguna capa en todo el rango de cámara.
7. Agregar lluvia (Particle System, `maxParticles = 800`) como hija de la cámara, y la iluminación fría (luz direccional azul) más el farol (punto cálido). Verificar: la lluvia sigue a la cámara y `particleCount` no supera 800.
8. Agregar el Volume global a `LaneSandbox` referenciando `GlobalVolumeProfile.asset` y fijar los valores de la tabla. Verificar: Bloom y Vignette visibles en Game view.
9. Agregar tests EditMode: defaults de `LaneConfig` y cálculo del rango de cámara para 16:9 y 18:9. Verificar: pasan en Test Runner.
10. Medir FPS con el overlay de stats en el editor (1920x1080) y en el emulador. Verificar: 60 FPS en el editor. El valor del emulador solo se registra.

---

## Criterios de aceptación

- [x] `LaneSandbox` abre y entra en Play Mode con 0 errores en consola.
- [x] La cámara es ortográfica con inclinación igual a `LaneConfig.cameraPitch`.
- [x] El centro de la cámara no sale de `±(laneHalfLength - semiancho visible)` en X y nunca se ve el vacío más allá del suelo.
- [x] Un sprite con `BillboardSprite` mantiene menos de 0.1° de diferencia con la rotación de la cámara.
- [x] Las tres capas de parallax se mueven a velocidades distintas, con factores 0.2, 0.5 y 0.8.
- [x] La lluvia es hija de la cámara, está activa y no supera 800 partículas.
- [x] La escena tiene luz fría y farol cálido, y el farol ilumina el suelo.
- [x] El Volume global de `LaneSandbox` muestra Bloom, Vignette y Color Adjustments con los valores de la tabla.
- [x] Los tests EditMode de `LaneConfig` y del rango de cámara pasan.
- [x] La escena corre a 60 FPS en el editor con Game view en 1920x1080.

---

## Decisiones

- **Sí:** sprites billboard sobre fondo 3D. Es el 2.5D elegido por Nico.
- **No:** modelos 3D para personajes. Costo de arte y animación.
- **Sí:** `LaneConfig` como ScriptableObject. Un solo lugar para medidas de carril, usado por specs 05, 06 y 07.
- **Sí:** inclinación de 12 grados (propuesta). Se ajusta con arte real.
- **Sí:** definición rápida sin aclaración detallada. Nico pidió generar las 30 specs de una vez; los valores propuestos se revisan antes de aprobar.
- **Sí:** sprites unlit con tinte. Más barato en móvil. La iluminación de sprites se decide con arte real.
- **Sí:** `BillboardSprite` copia la rotación completa de la cámara. Evita deformar el sprite con la inclinación.
- **Sí:** límite de cámara por semiancho visible. Evita ver el vacío en los extremos del carril.
- **Sí:** clamp propio en una extensión de Cinemachine. El Confiner de Cinemachine 3 no cubre cámara ortográfica 3D.

---

## Lo que **no** está en esta spec

- Héroe, enemigos, base y UI.
- Arte final.
- Niveles reales y flujo de escenas.
