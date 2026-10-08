# SPEC 26 — Anuncios con recompensa

> **Estado:** Aprobado
> **Depende de:** SPEC 15
> **Fecha:** 2026-10-06
> **Objetivo:** Integrar AdMob con video recompensado para duplicar oro, reanimar al héroe y obtener reliquias gratis.

---

## Alcance

**Dentro:**

- Plugin Google Mobile Ads para Unity.
- `IAdService` con `AdMobService` y `FakeAdService`.
- Tres ubicaciones: `ReviveHero`, `DoubleGold`, `FreeGems`.
- Formulario de consentimiento (UMP) al primer arranque.
- Límites por ubicación y por día.
- IDs de prueba de Google hasta el lanzamiento.
- Evento de Analytics `ad_reward`.

**Fuera de alcance (para specs futuras):**

- Anuncios intersticiales y banners.
- Mediación con otras redes.
- iOS.
- Ids de producción (SPEC 30).

---

## Modelo de datos

```csharp
public enum AdPlacement { ReviveHero, DoubleGold, FreeGems }

public interface IAdService
{
    bool IsReady(AdPlacement placement);
    System.Threading.Tasks.Task<bool> ShowRewardedAsync(AdPlacement placement);
}
```

Reglas propuestas:

| Ubicación | Recompensa | Límite |
| --- | --- | --- |
| `ReviveHero` | Revive al héroe con 50% de vida (usa `ReviveService` de SPEC 17) | 1 por nivel |
| `DoubleGold` | Duplica el oro de la partida al ver la victoria | 1 por nivel |
| `FreeGems` | 5 reliquias | 1 cada 4 horas, máximo 5 por día |

Los contadores diarios se guardan en el `SaveData`: se agrega `AdLimitsData { long lastFreeGemsUtc; int freeGemsToday; long dayStartUtc; }` dentro de `ProfileData` (incrementa `SaveData.version` a 2 con migración).

---

## Plan de implementación

1. Crear `AdPlacement`, `IAdService` y `FakeAdService` (muestra un panel simulado). Verificar: las tres recompensas funcionan en el editor.
2. Agregar `AdLimitsData` y migración de versión 1 a 2 con test EditMode.
3. Instalar Google Mobile Ads con IDs de prueba y crear `AdMobService` con precarga.
4. Integrar el consentimiento UMP al arranque.
5. Agregar el botón de reanimar en el panel de derrota por muerte del héroe.
6. Agregar el botón de duplicar oro en el panel de victoria.
7. Agregar el botón de reliquias gratis en el Emporium con temporizador.
8. Registrar `ad_reward` en Analytics.

---

## Criterios de aceptación

- [ ] El primer arranque muestra el formulario de consentimiento cuando corresponde.
- [ ] Los botones de anuncio se desactivan si no hay anuncio listo.
- [ ] Ver un anuncio completo entrega la recompensa; cerrarlo antes no la entrega.
- [ ] `ReviveHero` y `DoubleGold` solo se pueden usar una vez por nivel.
- [ ] `FreeGems` respeta el intervalo de 4 horas y el máximo diario.
- [ ] La migración a versión 2 conserva el progreso existente.
- [ ] Ningún anuncio aparece sin que el jugador lo pida.

---

## Decisiones

- **Sí:** solo video recompensado, voluntario. Es el modelo híbrido del documento técnico y respeta al jugador.
- **No:** intersticiales. Interrumpen el combate y dañan la experiencia.
- **Sí:** IDs de prueba hasta el lanzamiento. Evita la suspensión de la cuenta por clics propios.
- **Sí:** reloj del dispositivo para los límites. Se acepta la posibilidad de manipularlo en esta fase.
- **Sí:** definición rápida sin aclaración detallada.

---

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| Rechazo de Play por política de anuncios | Anuncios solo voluntarios y consentimiento UMP. |
| Usuario cambia la hora del dispositivo | Aceptado en esta fase. Validar con hora del servidor en una spec futura. |

---

## Lo que **no** está en esta spec

- Intersticiales, banners, mediación, iOS e IDs de producción.
