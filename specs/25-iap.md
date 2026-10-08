# SPEC 25 — Compras dentro de la aplicación

> **Estado:** Aprobado
> **Depende de:** SPEC 15, SPEC 18
> **Fecha:** 2026-10-06
> **Objetivo:** Integrar compras in-app en Google Play para paquetes de reliquias con Unity IAP.

---

## Alcance

**Dentro:**

- Paquete Unity In-App Purchasing.
- `IIapService` con `UnityIapService` y `FakeIapService` (editor y pruebas).
- Tres productos consumibles: `gems_pack_small`, `gems_pack_medium`, `gems_pack_large`.
- `IapProductDefinition` (ScriptableObject) con la cantidad de reliquias por producto.
- Tienda de reliquias dentro del Emporium con precios localizados desde la tienda.
- Entrega de reliquias con `CurrencyService` y registro de transacciones procesadas.
- Recuperación de compras pendientes al iniciar.
- Evento de Analytics `iap_purchase`.

**Fuera de alcance (para specs futuras):**

- Validación de recibos en servidor.
- iOS.
- Suscripciones y paquetes de inicio.
- Creación de la ficha en Play Console (SPEC 30).

---

## Modelo de datos

```csharp
[CreateAssetMenu(menuName = "EC/IAP Product")]
public class IapProductDefinition : ScriptableObject
{
    public string productId;
    public string displayName;
    public int gemsGranted;
}

public interface IIapService
{
    System.Threading.Tasks.Task InitializeAsync();
    System.Threading.Tasks.Task<bool> PurchaseAsync(string productId);
}
```

Cantidades propuestas: pequeño 80, mediano 450, grande 1000 reliquias. Los precios se definen en Play Console, no en el código.

`SaveData.profile.processedTransactionIds` evita entregar dos veces la misma compra.

---

## Plan de implementación

1. Crear `IIapService`, `IapProductDefinition` y `FakeIapService`. Verificar: en el editor, comprar entrega reliquias.
2. Instalar Unity IAP y crear `UnityIapService` con inicialización y catálogo de los tres productos.
3. Implementar la compra: confirmar con la tienda, entregar reliquias, guardar el id de transacción y recién entonces confirmar la compra a la tienda.
4. Implementar la recuperación de compras pendientes al arrancar.
5. Crear la interfaz de tienda de reliquias con precio localizado y estado de carga y error.
6. Registrar el evento `iap_purchase` en Analytics.
7. Probar con cuentas de prueba de licencia en un dispositivo, con la app subida a testing interno (depende de SPEC 30).

---

## Criterios de aceptación

- [ ] En el editor, `FakeIapService` entrega las reliquias del producto.
- [ ] En el dispositivo, la tienda muestra los precios localizados de los tres productos.
- [ ] Una compra exitosa suma las reliquias exactas del producto y se refleja en el HUD y la tienda.
- [ ] La misma transacción no entrega reliquias dos veces.
- [ ] Una compra cancelada no modifica el saldo y muestra un mensaje.
- [ ] Una compra pendiente al cerrar la app se entrega al reiniciar.
- [ ] Sin conexión, los botones de compra se desactivan.

---

## Decisiones

- **Sí:** Unity IAP. El documento técnico lo permite como opción (junto a RevenueCat). Evita una dependencia externa y una cuenta adicional.
- **No:** RevenueCat. Aporta validación en servidor y analítica de ingresos, pero agrega costo y complejidad; se reevalúa con tracción.
- **Sí:** solo consumibles de reliquias. Es lo que fija el documento técnico.
- **Sí:** confirmar la compra a la tienda después de entregar. Evita reembolsos automáticos por entregas fallidas.
- **Sí:** definición rápida sin aclaración detallada. Cantidades propuestas.

---

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| No se pueden probar compras reales sin una build en Play Console | Se prueba con testing interno al completar SPEC 30. |
| Fraude por recibos falsificados | Aceptado en esta fase. Validación en servidor queda para una spec futura. |

---

## Lo que **no** está en esta spec

- Validación en servidor, iOS, suscripciones y alta en Play Console.
