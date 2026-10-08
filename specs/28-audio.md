# SPEC 28 — Audio

> **Estado:** Implementado
> **Depende de:** SPEC 12
> **Fecha:** 2026-10-06
> **Objetivo:** Agregar música por capítulo y efectos de sonido con un `AudioManager` y mezcla por categorías.

---

## Alcance

**Dentro:**

- `AudioManager` en `EC.Core` (vive en `Boot`, `DontDestroyOnLoad`).
- `AudioCue` (ScriptableObject) con clips, volumen y variación de tono.
- `AudioMixer` con grupos `Music`, `Sfx` y `Ui`.
- Música por escena y por capítulo con fundido cruzado.
- Efectos: golpes, ataques, muertes, rugido, invocación, UI, lluvia ambiente.
- Evento `PlaySfx` por `EventBus`.
- Controles de volumen de música y efectos con persistencia en `SettingsData`.

**Fuera de alcance (para specs futuras):**

- Voces y doblaje.
- Audio espacial.
- Música original compuesta a medida.

---

## Modelo de datos

```csharp
[CreateAssetMenu(menuName = "EC/Audio Cue")]
public class AudioCue : ScriptableObject
{
    public string id;
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 1f;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    public bool loop;
}

public readonly struct PlaySfx { public readonly AudioCue Cue; }
```

Los volúmenes se guardan en `SaveData.settings.musicVolume` y `sfxVolume` (ya existen desde SPEC 14).

---

## Plan de implementación

1. Crear `AudioCue` y `AudioManager` con un pool de `AudioSource` (8 fuentes para efectos).
2. Crear el `AudioMixer` con los tres grupos y parámetros expuestos.
3. Implementar música con fundido cruzado de 1.5 segundos al cambiar de escena o capítulo.
4. Escuchar `PlaySfx` y reproducir con variación de tono.
5. Publicar `PlaySfx` desde ataques, daño, muerte, jefe, UI y habilidades.
6. Crear el panel de ajustes con dos controles de volumen que persisten.
7. Agregar música: mapa, nivel por capítulo, jefe y victoria. Registrar la licencia de cada archivo en `Assets/_Project/Audio/LICENSES.md`.

---

## Criterios de aceptación

- [ ] La música cambia con fundido de 1.5 s entre `Main`, mapa y nivel.
- [ ] Cada capítulo reproduce su pista y el jefe una pista propia.
- [ ] Los efectos de golpe, muerte y UI se escuchan en el dispositivo.
- [ ] No hay más de 8 efectos simultáneos y el 9.º no produce errores.
- [ ] Los volúmenes de música y efectos se guardan y se restauran.
- [ ] Cada archivo de audio tiene licencia registrada.
- [ ] Reproducir efectos no asigna memoria por frame.

---

## Decisiones

- **Sí:** audio de licencia libre (CC0 o equivalente) o creado por Nico. Evita problemas de derechos al publicar.
- **Sí:** efectos por evento. Mantiene a `EC.Core` sin dependencia de gameplay.
- **Sí:** límite de 8 efectos simultáneos. Protege la CPU en móviles.
- **Sí:** definición rápida sin aclaración detallada.

---

## Lo que **no** está en esta spec

- Voces, audio espacial y música original.
