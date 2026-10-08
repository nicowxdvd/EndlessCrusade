# SPEC 17 — Emporium de mejoras

> **Estado:** Implementado
> **Depende de:** SPEC 13, SPEC 15
> **Fecha:** 2026-10-06
> **Objetivo:** Crear la tienda Emporium con mejoras de héroe, tropas y base, y con consumibles comprables con oro o reliquias.

---

## Alcance

**Dentro:**

- Escena `Assets/_Project/Scenes/Emporium.unity` accesible desde el mapa.
- `UpgradeDefinition` y `ConsumableDefinition` (ScriptableObjects).
- `UpgradeService` en `EC.Services` que compra y consulta niveles.
- Mejoras de héroe: vida máxima, daño de espada, daño de látigo, reducción de cooldown.
- Mejoras de tropas: nivel de cada tropa (vida y daño).
- Mejoras de base: resistencia máxima.
- Defensa pasiva: ballesteros en las almenas como mejora de base.
- Consumibles: Poma de Agua Bendita y Elixir de Resurrección.
- Aplicación de modificadores al iniciar cada nivel (`StatModifierSet`).
- `ReviveService` que usa el Elixir una vez por nivel.

**Fuera de alcance (para specs futuras):**

- Reliquias compradas con dinero real (SPEC 25).
- Consumibles de otras fuentes (SPEC 27).
- Mejoras de eficacia de milagros (SPEC 21 define los milagros; la mejora se agrega ahí).

---

## Modelo de datos

```csharp
public enum UpgradeCategory { Hero, Troop, Base }

[CreateAssetMenu(menuName = "EC/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public UpgradeCategory category;
    public int maxLevel = 10;
    public int baseCost = 100;
    public float costGrowth = 1.35f;
    public float valuePerLevel;
    public CurrencyType currency = CurrencyType.Gold;
}

[CreateAssetMenu(menuName = "EC/Consumable Definition")]
public class ConsumableDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public CurrencyType currency;
    public int cost;
    public int maxStack = 9;
}
```

Costo del nivel `n`: `round(baseCost * costGrowth^n)`. Los niveles se guardan en `SaveData.upgrades` y los consumibles en `SaveData.consumables`.

`StatModifierSet` agrupa los multiplicadores y sumas resultantes. `LevelBootstrap` lo aplica al héroe, tropas y base al comenzar el nivel.

---

## Plan de implementación

1. Crear `UpgradeDefinition` y `ConsumableDefinition` y los assets iniciales.
2. Crear `UpgradeService` con `GetLevel`, `Cost`, `TryBuy`. Tests EditMode: curva de costo, tope de nivel, fondos insuficientes.
3. Crear `StatModifierSet` y calcularlo desde el `SaveData`.
4. Aplicar los modificadores en `LevelBootstrap` para héroe, tropas y base. Verificar: comprar vida máxima sube la vida en el nivel.
5. Crear la escena `Emporium` con pestañas Héroe, Tropas, Base y Consumibles.
6. Implementar las compras de consumibles con tope de stack.
7. Implementar `ReviveService` y el uso del Elixir al morir el héroe, con 50% de vida. Una vez por nivel.
8. Implementar la mejora de base "Ballesteros en las almenas": dos ballesteros automáticos que disparan a enemigos en rango.
9. Agregar el botón Emporium al mapa.

---

## Criterios de aceptación

- [ ] Comprar una mejora descuenta el costo y sube el nivel en 1.
- [ ] El costo de cada nivel sigue la fórmula `round(baseCost * costGrowth^n)`.
- [ ] No se puede comprar sobre `maxLevel` ni sin fondos.
- [ ] Las mejoras compradas se reflejan en las estadísticas dentro del nivel.
- [ ] Los niveles de mejoras y consumibles persisten.
- [ ] Con un Elixir, la muerte del héroe lo revive una vez con 50% de vida; sin Elixir, produce derrota.
- [ ] Los ballesteros de las almenas disparan solo si la mejora está comprada.

---

## Decisiones

- **Sí:** costo exponencial con crecimiento 1.35. Estándar para progresión de mejoras.
- **Sí:** modificadores aplicados al inicio del nivel. Evita tocar los ScriptableObjects originales.
- **No:** modificar los assets de definición en tiempo de ejecución. Persiste cambios por error en el editor.
- **Sí:** `ReviveService` compartido con anuncios (SPEC 26).
- **Sí:** definición rápida sin aclaración detallada. Costos propuestos.

---

## Lo que **no** está en esta spec

- Compras con dinero real, mejoras de milagros y equipo.
