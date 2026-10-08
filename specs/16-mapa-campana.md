# SPEC 16 — Mapa de campaña

> **Estado:** Aprobado
> **Depende de:** SPEC 12, SPEC 14
> **Fecha:** 2026-10-06
> **Objetivo:** Crear el mapa de campaña con cuatro capítulos y desbloqueo secuencial de niveles según el progreso guardado.

---

## Alcance

**Dentro:**

- `CampaignDefinition` y `ChapterDefinition` (ScriptableObjects).
- Escena `Assets/_Project/Scenes/CampaignMap.unity`.
- `SceneFlow` en `EC.Core` para cargar escenas con fundido.
- `LevelNode` (botón de nivel) con estados: bloqueado, disponible, completado.
- Capítulos: Afueras, Aldea, Bosque Maldito, Catedral. Solo Afueras tiene nivel jugable.
- Desbloqueo secuencial usando `ProgressData.completedLevels`.
- `Main` pasa a cargar `CampaignMap` con Jugar.
- Al elegir un nivel se asigna `LevelSession.Current` y se carga `Level.unity`.

**Fuera de alcance (para specs futuras):**

- Niveles de Aldea, Bosque y Catedral (SPEC 22, 23, 24).
- Tienda y minijuego accesibles desde el mapa (botones en SPEC 17 y 27).
- Arte final del mapa.

---

## Modelo de datos

```csharp
[CreateAssetMenu(menuName = "EC/Chapter Definition")]
public class ChapterDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public LevelDefinition[] levels;
    public string[] rewardEquipmentIds;
    public Sprite mapArt;
}

[CreateAssetMenu(menuName = "EC/Campaign Definition")]
public class CampaignDefinition : ScriptableObject
{
    public ChapterDefinition[] chapters;
}
```

Ids de capítulo: `ch1_afueras`, `ch2_aldea`, `ch3_bosque`, `ch4_catedral`. Ids de nivel: `lv_1_1`, `lv_2_1` a `lv_2_4`, `lv_3_1` a `lv_3_4`, `lv_4_1` a `lv_4_5`.

Regla: un nivel está disponible si es el primero de la campaña o si el nivel anterior está en `completedLevels`. Un capítulo sin niveles muestra "Próximamente".

---

## Plan de implementación

1. Crear `SceneFlow` con fundido y reemplazar las cargas directas de escena existentes.
2. Crear `ChapterDefinition`, `CampaignDefinition` y los cuatro capítulos (solo `ch1_afueras` con `Level_1_1`).
3. Crear la escena `CampaignMap` con un fondo por capítulo y desplazamiento horizontal.
4. Crear `LevelNode` con los tres estados y test EditMode de la regla de desbloqueo.
5. Al tocar un nivel disponible, asignar `LevelSession.Current` y cargar `Level.unity`.
6. Cambiar el botón Jugar de `Main` para cargar `CampaignMap`, y Salir del resultado para volver al mapa.
7. Verificar en el APK: Main, mapa, nivel, victoria, mapa con nivel completado.

---

## Criterios de aceptación

- [ ] El mapa muestra los cuatro capítulos.
- [ ] `lv_1_1` está disponible en una instalación nueva.
- [ ] Los capítulos sin niveles muestran "Próximamente" y no son tocables.
- [ ] Completar `lv_1_1` lo marca como completado y persiste.
- [ ] Un nivel cuyo anterior no está completado aparece bloqueado y no se puede iniciar.
- [ ] Salir del panel de resultado vuelve al mapa.
- [ ] Las transiciones de escena usan `SceneFlow`.

---

## Decisiones

- **Sí:** los cuatro capítulos aparecen desde el inicio. Comunica la longitud de la campaña.
- **Sí:** desbloqueo secuencial lineal. Es lo más simple y calza con una historia lineal.
- **No:** niveles libres. Rompe la progresión del equipo recuperado.
- **Sí:** definición rápida sin aclaración detallada.

---

## Lo que **no** está en esta spec

- Contenido de los capítulos 2, 3 y 4.
- Tienda, minijuego y arte final del mapa.
