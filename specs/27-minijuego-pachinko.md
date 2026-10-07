# SPEC 27 — Minijuego Pachinko

> **Estado:** Borrador
> **Depende de:** SPEC 15
> **Fecha:** 2026-10-06
> **Objetivo:** Crear el minijuego Pachinko con física 2D, consumo de boletos y tabla de premios ponderada en un ScriptableObject.

---

## Alcance

**Dentro:**

- Escena `Assets/_Project/Scenes/Pachinko.unity` accesible desde el mapa.
- Tablero con clavijas, rebote y ranuras de premio (Physics 2D).
- `PrizeTable` (ScriptableObject) con premios y pesos.
- `PachinkoService` que consume un boleto, sortea el premio y entrega la recompensa.
- Bola guiada: el premio se sortea primero y la física lleva la bola a la ranura correspondiente.
- Panel con las probabilidades visibles.
- Boleto diario gratis.
- Los boletos del nivel (SPEC 15) se usan aquí.

**Fuera de alcance (para specs futuras):**

- Compra de boletos con reliquias o dinero real.
- Boletos por anuncios.
- Más tableros o temporadas.

---

## Modelo de datos

```csharp
[System.Serializable]
public class PrizeEntry
{
    public string id;
    public PrizeKind kind;
    public int amount;
    public string itemId;
    public int weight;
}

public enum PrizeKind { Gold, Gems, Consumable }

[CreateAssetMenu(menuName = "EC/Prize Table")]
public class PrizeTable : ScriptableObject
{
    public PrizeEntry[] entries;
}
```

Tabla propuesta (pesos suman 100):

| Premio | Peso |
| --- | --- |
| 100 de oro | 40 |
| 500 de oro | 20 |
| 5 reliquias | 20 |
| 25 reliquias | 5 |
| Poma de Agua Bendita | 10 |
| Elixir de Resurrección | 5 |

Boleto diario: se usa `ProfileData.lastDailyClaimUtc` y se reinicia a las 00:00 UTC.

---

## Plan de implementación

1. Crear `PrizeEntry`, `PrizeTable` y el asset con la tabla propuesta.
2. Crear `PachinkoService` con `Draw` ponderado y `ClaimDaily`. Tests EditMode: 10 000 sorteos con semilla fija dentro de ±2 puntos de las probabilidades.
3. Entregar premios con `CurrencyService` y el inventario de consumibles.
4. Crear la escena `Pachinko` con tablero, clavijas y ranuras.
5. Implementar el lanzamiento de la bola y la guía hacia la ranura sorteada. Verificar: la bola termina siempre en la ranura del premio sorteado.
6. Crear la interfaz: boletos, botón de lanzar, resultado y panel de probabilidades.
7. Agregar el boleto diario y el acceso desde el mapa.

---

## Criterios de aceptación

- [ ] Lanzar la bola descuenta un boleto y no se puede lanzar con 0 boletos.
- [ ] El premio entregado coincide con la ranura donde cae la bola.
- [ ] El test de 10 000 sorteos respeta las probabilidades de la tabla (±2 puntos).
- [ ] El panel muestra las probabilidades reales calculadas desde `PrizeTable`.
- [ ] El boleto diario se puede reclamar una vez por día UTC.
- [ ] Boletos y premios persisten tras reiniciar.
- [ ] El tablero corre a 60 FPS en el dispositivo.

---

## Decisiones

- **Sí:** sortear primero y guiar la bola. Las probabilidades son auditables y se pueden mostrar al jugador; la física puramente caótica no las garantiza.
- **No:** premio determinado solo por física. No se puede balancear ni declarar.
- **Sí:** probabilidades visibles. Requisito habitual de las tiendas para mecánicas de azar con valor.
- **Sí:** el minijuego no se compra con dinero real en esta spec. Reduce el riesgo regulatorio hasta revisarlo.
- **Sí:** definición rápida sin aclaración detallada. Tabla propuesta.

---

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| Normativa sobre cajas de botín o azar con pago | No hay compra directa de boletos. Probabilidades visibles. |

---

## Lo que **no** está en esta spec

- Compra de boletos, anuncios por boleto y tableros adicionales.
