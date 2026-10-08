# SPEC 21 — Equipo recuperado

> **Estado:** Implementado
> **Depende de:** SPEC 10, SPEC 16
> **Fecha:** 2026-10-06
> **Objetivo:** Hacer que el héroe recupere armadura, escudo, maza, ballesta y milagros al completar capítulos, con cambio visual del sprite.

---

## Alcance

**Dentro:**

- `EquipmentDefinition` y `MiracleDefinition` (ScriptableObjects).
- Ranuras: Armadura, Escudo, Arma pesada (maza), Arma a distancia (ballesta).
- `EquipmentService` que entrega equipo al completar un capítulo y lo guarda en `SaveData.equipment`.
- Sprite del héroe en capas, con intercambio por ranura.
- Efectos: la armadura suma vida y reduce daño; el escudo bloquea; la maza aturde; la ballesta dispara a distancia.
- Milagros: Bendición y Juicio Divino como habilidades de SPEC 10.
- Pantalla de equipo con equipar y desequipar.

**Fuera de alcance (para specs futuras):**

- Mejoras de eficacia de milagros por tienda.
- Equipo comprable en la tienda.
- Arte final de cada pieza.

---

## Modelo de datos

```csharp
public enum EquipmentSlot { Armor, Shield, HeavyWeapon, RangedWeapon }

[CreateAssetMenu(menuName = "EC/Equipment Definition")]
public class EquipmentDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public EquipmentSlot slot;
    public int bonusHealth;
    public float damageReduction;
    public MeleeAttackDefinition attackOverride;
    public Sprite[] layerSprites;
}
```

Entrega propuesta:

| Cuándo | Equipo y milagros |
| --- | --- |
| Completar `ch2_aldea` | Armadura templaria (vida +80, reducción 20%), milagro Bendición |
| Completar `ch3_bosque` | Escudo (bloqueo 70% mientras se mantiene), Maza (daño 35, aturde 1 s), Ballesta (daño 22, alcance 9, cooldown 1.4), milagro Juicio Divino |

Bendición: cura 25% de vida al héroe y elimina la desorientación. Cooldown 25 s. Juicio Divino: daña a todos los enemigos en pantalla, el doble contra `Undead`. Cooldown 45 s.

El equipo se identifica por `id` en `SaveData.equipment.owned` y `equipped`.

---

## Plan de implementación

1. Crear `EquipmentDefinition` y `MiracleDefinition` con los assets de la tabla.
2. Crear `EquipmentService` con entrega por capítulo (usa `ChapterDefinition.rewardEquipmentIds`) y test EditMode de entrega única.
3. Convertir el sprite del héroe en capas (cuerpo, armadura, escudo, arma) y crear `HeroVisualController` que las intercambia.
4. Aplicar bonos de vida y reducción de daño de la armadura.
5. Implementar escudo (mantener botón) y reducción de daño al bloquear.
6. Implementar la maza como ataque pesado con aturdimiento.
7. Implementar la ballesta con `Bolt` pooleado.
8. Implementar Bendición y Juicio Divino como `AbilityDefinition`.
9. Crear la pantalla de equipo con ranuras.

---

## Criterios de aceptación

- [ ] Al iniciar el juego, el héroe no tiene armadura ni escudo.
- [ ] Completar `ch2_aldea` entrega la armadura y Bendición, una sola vez.
- [ ] Completar `ch3_bosque` entrega escudo, maza, ballesta y Juicio Divino, una sola vez.
- [ ] Equipar una pieza cambia el sprite del héroe.
- [ ] El escudo reduce el daño recibido un 70% mientras se mantiene el botón.
- [ ] La maza aturde al enemigo golpeado 1 segundo.
- [ ] La ballesta daña desde 9 unidades.
- [ ] El equipo entregado persiste tras reiniciar.

---

## Decisiones

- **Sí:** el equipo se recupera por capítulo. La historia plantea al héroe recuperando su equipo templario a lo largo del viaje.
- **Sí:** sprites en capas. Permite combinar piezas sin dibujar cada combinación.
- **Sí:** entrega y tabla propuestas. El documento de historia no fija en qué capítulo llega cada pieza.
- **Sí:** definición rápida sin aclaración detallada.

---

## Lo que **no** está en esta spec

- Compra de equipo, mejoras de milagros y arte final.
