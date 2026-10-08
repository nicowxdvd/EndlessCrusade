# SPEC 12 — Nivel 1: El Despertar en las Afueras

> **Estado:** Implementado
> **Depende de:** SPEC 10, SPEC 11
> **Fecha:** 2026-10-06
> **Objetivo:** Armar el nivel 1 completo con introducción narrativa en viñetas, tutorial, oleadas de wargos y murciélagos y el Licántropo Gigante como jefe.

---

## Alcance

**Dentro:**

- Escena genérica `Assets/_Project/Scenes/Level.unity` que se configura con `LevelSession.Current`.
- `LevelSession` en `EC.Data` y `LevelBootstrap` que instancia entorno, base, héroe y controlador de oleadas.
- Campo `environmentPrefab` en `LevelDefinition`.
- `LevelDefinition` `Level_1_1` (id `lv_1_1`).
- `StorySequence` (ScriptableObject) y `StoryPlayer` en `EC.UI`: viñetas con texto, toque para avanzar y botón Saltar.
- Intro: la noche del rapto. Outro: el Licántropo huye y el héroe parte hacia la aldea.
- `TutorialController` con cuatro pasos: mover, espada, látigo, agua bendita.
- Entorno `Env_Outskirts`: bosque barroso, lluvia intensa, penumbra, ruinas de la cabaña.
- Héroe sin armadura (espada corta y látigo).
- Pantalla `Main` con botón Jugar que carga `Level.unity` con `Level_1_1`.

**Fuera de alcance (para specs futuras):**

- Mapa de campaña y selección de niveles (SPEC 16).
- Recompensas (SPEC 15).
- Tropas en este nivel (el nivel 1 no tiene tropas).
- Arte final y audio final.

---

## Modelo de datos

```csharp
[System.Serializable]
public class StoryPanel
{
    [TextArea] public string text;
    public Sprite image;
}

[CreateAssetMenu(menuName = "EC/Story Sequence")]
public class StorySequence : ScriptableObject
{
    public string id;
    public StoryPanel[] panels;
}

public static class LevelSession
{
    public static LevelDefinition Current;
}
```

`LevelDefinition` recibe: `GameObject environmentPrefab`, `StorySequence intro`, `StorySequence outro`, `bool troopsEnabled`, `bool tutorial`.

Propuesta de oleadas de `Level_1_1`: 5 oleadas. Oleada 1, 4 wargos. Oleada 2, 6 murciélagos. Oleada 3, 5 wargos y 4 murciélagos. Oleada 4, 8 wargos y 6 murciélagos. Oleada 5, jefe Licántropo Gigante con 4 wargos.

Textos de la intro (propuesta, 5 viñetas): el retiro del Templario, la tormenta, la ebriedad, el ataque y el rapto del Bebé de la Profecía, el despertar entre la puerta destruida.

---

## Plan de implementación

1. Agregar a `LevelDefinition` los campos nuevos y crear `LevelSession`.
2. Crear `Level.unity` con `LevelBootstrap` que lee `LevelSession.Current` (en el editor, si es nulo, usa `Level_1_1`).
3. Crear `Env_Outskirts` con placeholders, lluvia intensa y la base `Base_CabinRuins`. Verificar: el nivel carga con entorno y base.
4. Crear `StorySequence` y `StoryPlayer`. Verificar: la intro se muestra, avanza con toque y se salta.
5. Crear `Level_1_1` con las 5 oleadas y el jefe. Verificar: se puede jugar hasta el final.
6. Crear `TutorialController` con los cuatro pasos y mensajes en pantalla. Se desactiva si `tutorial = false`.
7. Mostrar la outro tras la victoria, antes del panel de resultado.
8. Crear el botón Jugar en `Main` y agregar `Level.unity` a Build Settings. Verificar en el APK: Boot, Main, intro, tutorial, nivel y outro.

---

## Criterios de aceptación

- [ ] Desde `Main`, Jugar muestra la intro de 5 viñetas y luego el nivel.
- [ ] El botón Saltar omite la intro.
- [ ] El tutorial pide mover, usar espada, látigo y agua bendita, y avanza al detectar cada acción.
- [ ] El héroe comienza con espada corta y látigo, sin armadura.
- [ ] El nivel tiene 5 oleadas y la última incluye al Licántropo Gigante.
- [ ] No hay botón de invocación de tropas en este nivel.
- [ ] Al vencer al jefe se muestra la outro y luego el panel de victoria.
- [ ] El nivel completo funciona en el APK sin errores en `adb logcat`.

---

## Decisiones

- **Sí:** una escena `Level.unity` genérica configurada por datos. Evita duplicar escenas por cada nivel de los capítulos siguientes.
- **No:** una escena por nivel. Multiplica conflictos de merge y mantenimiento.
- **Sí:** el nivel 1 funciona como tutorial. Decisión de Nico: no hay tropas hasta avanzar.
- **Sí:** defender las ruinas de la cabaña como base. Es la adaptación elegida para encajar con la historia.
- **Sí:** definición rápida sin aclaración detallada. Textos de viñetas y oleadas propuestos.

---

## Lo que **no** está en esta spec

- Mapa, recompensas, tropas, arte final y audio.
