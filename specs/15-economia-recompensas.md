# SPEC 15 — Economía y recompensas

> **Estado:** Borrador
> **Depende de:** SPEC 12, SPEC 14
> **Fecha:** 2026-10-06
> **Objetivo:** Introducir oro (moneda blanda) y reliquias (moneda dura) con recompensas al terminar cada nivel.

---

## Alcance

**Dentro:**

- `CurrencyService` en `EC.Services` que opera sobre `ISaveService`.
- Oro (`gold`) y Reliquias Sacras (`gems`, mostradas como "Reliquias").
- Boletos (`tickets`) como tercer saldo para el minijuego.
- Oro obtenido al matar enemigos durante el nivel (`EnemyDefinition.goldDrop`).
- `LevelReward` por nivel: primera vez y repetición.
- Panel de resultado con el desglose de recompensas.
- Evento `CurrencyChanged`.
- Saldo de oro visible en el HUD.

**Fuera de alcance (para specs futuras):**

- Compras con dinero real (SPEC 25).
- Anuncios con recompensa (SPEC 26).
- Gasto en la tienda (SPEC 17).
- Boletos de Pachinko y su uso (SPEC 27).

---

## Modelo de datos

```csharp
[System.Serializable]
public class LevelReward
{
    public int goldFirstClear;
    public int goldReplay;
    public int gemsFirstClear;
    public int ticketsFirstClear;
}

public enum CurrencyType { Gold, Gems, Tickets }

public readonly struct CurrencyChanged { public readonly CurrencyType Type; public readonly int Balance; }
```

`LevelDefinition` recibe `LevelReward reward`. `CurrencyService` expone `Balance(type)`, `Add(type, amount)` y `TrySpend(type, amount)`.

Valores propuestos para `Level_1_1`: oro primera vez 150, repetición 60, reliquias 5, boletos 1. Al perder se conserva el 50% del oro obtenido en la partida.

---

## Plan de implementación

1. Crear `CurrencyType`, `LevelReward` y el evento `CurrencyChanged`.
2. Crear `CurrencyService` con `Add`, `TrySpend` y persistencia. Tests EditMode: saldo no negativo, `TrySpend` falla sin fondos.
3. Sumar `goldDrop` al morir cada enemigo en un acumulador de partida (`RunRewards`).
4. Aplicar la recompensa al terminar el nivel (victoria o derrota) y marcar el nivel como completado si ganó.
5. Mostrar el desglose en `ResultPanel`: oro por enemigos, bono de nivel, reliquias, boletos.
6. Mostrar el oro en el HUD durante la partida.
7. Cargar la recompensa de `Level_1_1` y verificarla en el APK.

---

## Criterios de aceptación

- [ ] Matar enemigos suma `goldDrop` al acumulador de la partida.
- [ ] La primera victoria otorga `goldFirstClear`, `gemsFirstClear` y `ticketsFirstClear`.
- [ ] Repetir el nivel otorga `goldReplay` y ninguna reliquia ni boleto.
- [ ] Una derrota conserva el 50% del oro de la partida y no marca el nivel como completado.
- [ ] `TrySpend` con saldo insuficiente devuelve falso y no modifica el saldo.
- [ ] Los saldos persisten tras reiniciar la aplicación.
- [ ] `ResultPanel` muestra el desglose correcto.

---

## Decisiones

- **Sí:** economía dual definida por el documento técnico: oro blando y reliquias duras.
- **Sí:** las reliquias solo se ganan por primera vez en cada nivel. Evita farmeo de moneda dura.
- **Sí:** conservar el 50% del oro al perder. Suaviza la frustración sin eliminar el costo de perder.
- **Sí:** definición rápida sin aclaración detallada. Valores propuestos, se balancean en SPEC 29.

---

## Lo que **no** está en esta spec

- Dinero real, anuncios, tienda y Pachinko.
