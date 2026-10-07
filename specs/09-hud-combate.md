# SPEC 09 — HUD de combate

> **Estado:** Implementado
> **Depende de:** SPEC 05, SPEC 07, SPEC 08
> **Fecha:** 2026-10-06
> **Objetivo:** Mostrar vida del héroe, resistencia de la base, oleada actual, controles táctiles y pantallas de pausa, victoria y derrota con estilo gótico.

---

## Alcance

**Dentro:**

- Canvas uGUI con Canvas Scaler a 1920x1080 y soporte de safe area.
- `UiTheme` (ScriptableObject) con colores y fuentes del estilo gótico.
- `HudPresenter` en `EC.UI` que escucha el `EventBus`.
- Barras de vida del héroe y de resistencia de la base.
- Indicador de oleada ("Oleada 2 / 5").
- Controles táctiles definitivos que reemplazan el Canvas temporal de SPEC 05.
- Botón de pausa y panel de pausa.
- `ResultPanel` de victoria y derrota con botones Reintentar y Salir.

**Fuera de alcance (para specs futuras):**

- Botones de habilidades (SPEC 10) e invocación (SPEC 13).
- Barra de jefe (SPEC 11).
- Recompensas y monedas en pantalla (SPEC 15).
- Arte final de UI.

---

## Modelo de datos

```csharp
[CreateAssetMenu(menuName = "EC/UI Theme")]
public class UiTheme : ScriptableObject
{
    public Color background = new Color32(0x0B, 0x0B, 0x10, 0xFF);
    public Color blood = new Color32(0x8B, 0x00, 0x00, 0xFF);
    public Color gold = new Color32(0xC9, 0xA2, 0x4B, 0xFF);
    public Color text = new Color32(0xD8, 0xD8, 0xDC, 0xFF);
    public TMPro.TMP_FontAsset titleFont;
    public TMPro.TMP_FontAsset bodyFont;
}
```

`EC.UI` solo escucha eventos de `EC.Core`: `HealthChanged`, `BaseResistanceChanged`, `WaveStarted`, `LevelEnded`, `HeroSpawned`. No referencia `EC.Gameplay`.

---

## Plan de implementación

1. Crear `UiTheme` y el asset `UiTheme_Gothic`. Elegir dos fuentes TMP de licencia libre y anotar su licencia en `Assets/_Project/Art/Fonts/LICENSES.md`.
2. Crear el prefab `HudCanvas` con scaler, safe area y barras vacías.
3. Crear `HudPresenter`: filtra `HealthChanged` por el héroe usando `HeroSpawned` y actualiza la barra. Verificar: dañar al héroe baja la barra.
4. Conectar `BaseResistanceChanged` y `WaveStarted`.
5. Mover los cuatro botones táctiles al HUD, con tamaño mínimo de 96 px y zonas separadas del borde.
6. Crear panel de pausa que pone `Time.timeScale = 0` y lo restaura.
7. Crear `ResultPanel` que aparece con `LevelEnded`. Reintentar recarga la escena y Salir carga `Main`.

---

## Criterios de aceptación

- [x] La barra del héroe baja al recibir daño y llega a 0 al morir.
- [x] La barra de la base refleja `BaseResistanceChanged`.
- [x] El texto de oleada muestra el índice y el total correctos.
- [x] Pausa detiene el juego y reanudar lo restaura a `timeScale = 1`.
- [x] Victoria y derrota muestran el panel correcto y solo una vez.
- [x] Reintentar reinicia el nivel con vida y oleadas desde cero.
- [x] Ningún botón táctil queda cubierto por la muesca ni por el borde en un emulador de 20:9.
- [x] `EC.UI` no referencia `EC.Gameplay`.

---

## Decisiones

- **Sí:** uGUI con TextMeshPro. Es lo incluido en Unity 6 y el equipo ya lo instaló en SPEC 01.
- **No:** UI Toolkit. Menos soporte táctil probado para HUD en tiempo real.
- **Sí:** `UiTheme` centralizado. Nico prioriza que se vea bonito, el estilo se cambia en un solo lugar.
- **Sí:** definición rápida sin aclaración detallada.

---

## Lo que **no** está en esta spec

- Habilidades, invocación, jefe y monedas en HUD.
- Arte final de UI.
