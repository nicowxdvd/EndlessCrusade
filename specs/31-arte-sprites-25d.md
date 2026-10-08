# SPEC 31 — Arte de sprites 2.5D del héroe

> **Estado:** Implementado
> **Depende de:** SPEC 02, SPEC 03, SPEC 05
> **Fecha:** 2026-10-07
> **Objetivo:** Definir el pipeline de arte 2D del Templario (formato, importación, atlas y animación por estado de la FSM), producir dos versiones del héroe (pixel art e ilustrada) con IA, compararlas en `LaneSandbox` y reemplazar el placeholder con la ganadora.

---

## Alcance

**Dentro:**

- Dos estilos de prueba del héroe, ambos de sprite único y vista lateral: pixel art e ilustrado.
- Convención de archivos, tamaños, PPU y filtros de importación por estilo.
- `SpriteAnimationSet` (ScriptableObject) con clips de frames por id.
- `SpriteStateAnimator`, componente que reproduce clips según `EntityState` y permite clips por id (ataques, habilidades).
- `SpriteAtlas` por estilo.
- Escena de comparación de estilos en `LaneSandbox`.
- Reemplazo de `Hero_Placeholder` en el prefab `Hero` con el estilo ganador.
- Guía de producción con IA y lista de verificación de cada frame.

**Fuera de alcance (para specs futuras):**

- Sprite en capas y cambio visual por equipo (SPEC 21).
- Arte de enemigos, tropas y jefes. Nacen con este pipeline en sus specs.
- Iluminación de sprites (shader Lit, normal maps). Los sprites siguen unlit con tinte.
- Animación por huesos (Unity 2D Animation).
- Sincronizar el momento del golpe con un frame de la animación. El daño sigue siendo instantáneo (SPEC 05).
- Fondos, base y UI.

---

## Modelo de datos

```csharp
[System.Serializable]
public class SpriteClip
{
    public string id;
    public Sprite[] frames;
    public float framesPerSecond = 10f;
    public bool loop = true;
}

[CreateAssetMenu(menuName = "EC/Sprite Animation Set")]
public class SpriteAnimationSet : ScriptableObject
{
    public SpriteClip[] clips;
    public SpriteClip Find(string id);
}
```

`SpriteStateAnimator` (en `EC.Gameplay`):

```csharp
public class SpriteStateAnimator : MonoBehaviour
{
    public void SetState(EntityState state);
    public void Play(string clipId);
    public bool IsPlayingOneShot { get; }
}
```

Mapa de estados a clips:

| `EntityState` | Clip | Bucle |
| --- | --- | --- |
| `Idle` | `idle` | sí |
| `Move` | `move` | sí |
| `Attack` | ninguno propio; lo reproduce `Play` con `attack_sword` o `attack_whip` | no |
| `Hurt` | `hurt` | no |
| `Dead` | `dead` | no, se queda en el último frame |

Clips por id, además de los estados: `attack_sword`, `attack_whip`, `summon`, `holy_water`.

Reglas:

- `SpriteStateAnimator` no usa `Animator`. Es un componente propio que avanza el índice de frame con un acumulador de tiempo y asigna `SpriteRenderer.sprite`. Evita el costo de `Animator` por entidad (hallazgo 4 de `docs/auditoria-rendimiento.md`).
- El ataque en la FSM dura un frame (SPEC 05). La animación del ataque no depende de la FSM: `HeroController` llama a `Play("attack_sword")` o `Play("attack_whip")` al lanzar el ataque, y el clip corre hasta el final aunque la FSM ya esté en `Idle` o `Move`.
- Mientras corre un clip no en bucle, `SetState(Idle)` y `SetState(Move)` se guardan y se aplican al terminar el clip.
- `Hurt` y `Dead` interrumpen cualquier clip. `Dead` es terminal.
- `flipX` lo sigue controlando `HeroController` (SPEC 05). `SpriteStateAnimator` no lo toca.
- `Play` con un id inexistente no lanza excepción: no hace nada y registra una advertencia una sola vez por id.
- Sin asignaciones de memoria por frame.

Clips y frames propuestos (por estilo):

| Clip | Frames | FPS | Bucle |
| --- | --- | --- | --- |
| `idle` | 4 | 6 | sí |
| `move` | 6 | 10 | sí |
| `attack_sword` | 5 | 14 | no |
| `attack_whip` | 6 | 14 | no |
| `hurt` | 2 | 10 | no |
| `dead` | 6 | 10 | no |
| `summon` | 5 | 10 | no |
| `holy_water` | 5 | 12 | no |

Total: 39 frames por estilo.

---

## Especificación de arte

Aplica a los dos estilos. Referencia de diseño para ambos estilos: `Pre-proyecto/prota.jpeg`. El estilo pixel art se genera a partir de esa misma imagen.

| | Pixel art | Ilustrado |
| --- | --- | --- |
| Frame | 128×128 px | 512×512 px |
| Héroe dentro del frame | unos 112 px de alto | unos 450 px de alto |
| PPU | 64 | 256 |
| Filtro | Point (no filter) | Bilinear |
| Compresión | ninguna | ASTC 6x6 |
| MipMaps | no | no |
| Carpeta | `Art/Sprites/Hero/Pixel/` | `Art/Sprites/Hero/Illustrated/` |

Reglas comunes:

- El frame mide 2×2 m en el mundo. El héroe mide unos 1.75 m (el placeholder actual mide 0.8×1.8 m).
- Vista lateral, mirando a la derecha. La izquierda se obtiene con `flipX`.
- Fondo transparente. Sin suelo, sin sombra y sin fondo de la imagen de referencia.
- Pivote en `Bottom Center`, en el punto entre los pies. Todos los frames de un clip comparten la misma línea de pies.
- Un spritesheet por clip, en cuadrícula de una fila. Nombre del archivo: `Hero_<Estilo>_<clip>.png`, por ejemplo `Hero_Pixel_move.png`. Los frames cortados se llaman `Hero_<Estilo>_<clip>_00`, `_01`, etc.
- Paleta fija del héroe: capa roja, armadura negra, detalles dorados y cruz blanca (referencias).
- Shader: `Sprite-Unlit-Default` con tinte, como en SPEC 02.
- Un `SpriteAtlas` por estilo con todos los clips del héroe. Tope: 8 MB comprimido en el atlas ilustrado.

---

## Producción con IA

El arte se genera con servicios de IA. Esta spec fija el flujo y los criterios, no la herramienta definitiva.

Herramientas candidatas (verificar disponibilidad y precio al empezar):

- Pixel art: PixelLab (personajes y animaciones pixel art con spritesheet). Alternativa: Retro Diffusion.
- Ilustrado: un modelo de imagen que acepte `prota.jpeg` como referencia para generar poses, como Flux Kontext o Gemini image. Alternativa: Scenario.

Flujo por estilo:

1. Generar una pose base lateral limpia a partir de la referencia, con fondo transparente.
2. Generar cada clip a partir de la pose base, un frame a la vez o con la herramienta de animación.
3. Limpiar a mano en un editor de píxeles (Aseprite o similar): alinear los pies, igualar proporciones y paleta, corregir manos, armas y capa.
4. Exportar un spritesheet por clip y copiarlo a la carpeta del estilo.

Riesgo principal: la consistencia entre frames. Cada frame puede salir con proporciones, detalles o paleta distintos. Se asume limpieza manual.

Verificación de cada frame:

- Mismo tamaño de frame y misma línea de pies.
- Mismo alto de héroe, con tolerancia de 4% entre frames del mismo clip.
- Sin píxeles semitransparentes sobrantes del fondo.
- Espada, látigo y cruz visibles y del mismo tamaño que en la pose base.

---

## Plan de implementación

1. Crear `SpriteClip`, `SpriteAnimationSet` y `SpriteStateAnimator`, con tests EditMode del avance de frames (bucle, clip no en bucle, `Dead` terminal, id inexistente). Verificar: tests en verde.
2. Crear la prueba mínima de cada estilo: `idle` (4 frames) y `move` (6 frames). Cortar con el pivote `Bottom Center` y aplicar las reglas de importación de cada estilo.
3. Crear `Hero_Pixel_Animations.asset` y `Hero_Illustrated_Animations.asset` con esos dos clips.
4. Agregar a `LaneSandbox` una escena de comparación con dos héroes, uno por estilo, sobre el mismo fondo y con la cámara de SPEC 02. Verificar: ambos caminan con el teclado y los pies quedan sobre `groundY`.
5. Comparar en el editor y en un dispositivo Android. Decidir el estilo ganador y anotarlo en Decisiones.
6. Producir los clips restantes del estilo ganador: `attack_sword`, `attack_whip`, `hurt`, `dead`, `summon`, `holy_water`.
7. Crear el `SpriteAtlas` del estilo ganador y el asset `Hero_Animations.asset`.
8. En el prefab `Hero`: reemplazar `Hero_Placeholder`, llevar la escala a `(1, 1, 1)` y agregar `SpriteStateAnimator`. El placeholder usaba la escala `(0.8, 1.8, 1)` para dar tamaño.
9. Conectar `HeroController` y `HealthComponent`: `SetState` con los cambios de la FSM y `Play` al lanzar espada, látigo, invocación y agua bendita. Verificar: cada acción reproduce su clip y vuelve a `Idle` o `Move`.
10. Medir en un dispositivo Android: 60 FPS estables con 20 entidades en pantalla y sin asignaciones de memoria por frame del animador.
11. Archivar el estilo descartado fuera de `Assets/` (no se importa en Unity) y borrar la escena de comparación.

---

## Criterios de aceptación

- [ ] Existen los dos estilos con `idle` y `move` importados con las reglas de cada uno.
- [ ] Los pies del héroe quedan en `groundY` en todos los frames de ambos estilos.
- [ ] La escena de comparación muestra ambos estilos en Android y hay un estilo elegido registrado en Decisiones.
- [ ] El héroe del estilo ganador tiene los 8 clips de la tabla.
- [ ] Mover al héroe reproduce `move`, y detenerlo vuelve a `idle`.
- [ ] Atacar con espada y con látigo reproduce su clip completo aunque la FSM vuelva a `Idle` en el mismo frame.
- [ ] Recibir daño no letal reproduce `hurt`. La muerte reproduce `dead` y se queda en el último frame.
- [ ] El sprite se espeja con `flipX` al moverse a la izquierda y la animación sigue igual.
- [ ] `SpriteStateAnimator` no asigna memoria por frame.
- [ ] El atlas comprimido del héroe no supera 8 MB.
- [ ] El prefab `Hero` ya no usa `Hero_Placeholder`.

---

## Decisiones

- **Sí:** sprite único primero, sin capas. Las capas llegan con la SPEC 21.
- **Sí:** vista lateral única con `flipX`, como en SPEC 05.
- **Sí:** producir los dos estilos con `idle` y `move` antes de decidir. Evita pagar los 39 frames de un estilo que luego no gusta.
- **Sí:** unlit con tinte. Es lo que usa SPEC 02 y evita el costo del shader Lit en móvil.
- **Sí:** animador propio por frames en lugar de `Animator`. Menos costo por entidad y suficiente para clips de sprites.
- **Sí:** el clip de ataque corre aparte de la FSM. SPEC 05 deja el estado `Attack` en un frame y esta spec no lo cambia.
- **Sí:** PPU 64 en pixel art y 256 en ilustrado (propuestos). Dan un héroe de unos 1.75 m con frame de 2 m.
- **Sí:** la herramienta de IA se confirma al empezar el paso 2. Los servicios cambian y no se verificaron para esta spec.
- **Pendiente:** estilo ganador. Se decide en el paso 5.
- **Sí:** definición rápida sin aclaración detallada. Valores propuestos, a revisar con arte real.

---

## Lo que **no** está en esta spec

- Capas de equipo y sprite por ranura.
- Arte de enemigos, tropas y jefes.
- Fondos y arte de la base.
- Iluminación de sprites.
- Sincronizar el daño con un frame de la animación.
