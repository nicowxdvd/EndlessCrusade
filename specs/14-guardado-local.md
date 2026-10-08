# SPEC 14 — Guardado local

> **Estado:** Implementado
> **Depende de:** SPEC 01
> **Fecha:** 2026-10-06
> **Objetivo:** Guardar progreso, monedas y mejoras en un JSON local versionado detrás de la interfaz `ISaveService`.

---

## Alcance

**Dentro:**

- `SaveData` y sus estructuras en `EC.Data`.
- `ISaveService` en `EC.Data`.
- `JsonSaveService` en `EC.Services` con escritura atómica y copia de respaldo.
- Migración por versión.
- Tests EditMode.

**Fuera de alcance (para specs futuras):**

- Sincronización en la nube (SPEC 18).
- Lógica de economía, tienda y progreso que usa estos datos (SPEC 15, 16, 17).
- Cifrado del archivo.

---

## Modelo de datos

```csharp
[System.Serializable]
public class SaveData
{
    public int version = 1;
    public string playerId;
    public long updatedAtUtc;
    public ProfileData profile = new ProfileData();
    public ProgressData progress = new ProgressData();
    public List<UpgradeLevel> upgrades = new List<UpgradeLevel>();
    public List<ItemCount> consumables = new List<ItemCount>();
    public EquipmentData equipment = new EquipmentData();
    public SettingsData settings = new SettingsData();
}

[System.Serializable] public class ProfileData { public int gold; public int gems; public int tickets; public long lastDailyClaimUtc; public List<string> processedTransactionIds = new List<string>(); }
[System.Serializable] public class ProgressData { public List<string> completedLevels = new List<string>(); }
[System.Serializable] public class UpgradeLevel { public string id; public int level; }
[System.Serializable] public class ItemCount { public string id; public int count; }
[System.Serializable] public class EquipmentData { public List<string> owned = new List<string>(); public List<string> equipped = new List<string>(); }
[System.Serializable] public class SettingsData { public float musicVolume = 0.8f; public float sfxVolume = 1f; }

public interface ISaveService
{
    SaveData Current { get; }
    void Load();
    void Save();
    void Reset();
}
```

Archivos en `Application.persistentDataPath`: `save.json` y `save.json.bak`. Se usa `JsonUtility` (sin diccionarios).

Los campos de monedas, mejoras, consumibles y equipo se declaran ahora para evitar migraciones antes de que las specs 15, 17 y 21 los usen.

---

## Plan de implementación

1. Crear las clases de datos y `ISaveService` en `EC.Data`.
2. Crear `JsonSaveService` con `Load` que crea un `SaveData` nuevo (con `playerId` GUID) si no hay archivo.
3. Implementar `Save` atómico: escribir `save.json.tmp`, rotar el anterior a `.bak` y renombrar.
4. Implementar la recuperación: si `save.json` está corrupto, cargar `.bak`; si ambos fallan, crear uno nuevo y escribir un warning.
5. Crear `SaveMigrator` con una lista ordenada de migraciones por versión.
6. Crear tests EditMode: ida y vuelta, archivo corrupto, ausencia de archivo, migración de versión 0 a 1.
7. Registrar el servicio en `Boot` y guardar al pausar y al salir de la aplicación (`OnApplicationPause`).

---

## Criterios de aceptación

- [ ] Primer arranque sin archivo crea un `SaveData` con `version = 1` y un `playerId` único.
- [ ] Guardar y cargar devuelve los mismos datos (test de ida y vuelta).
- [ ] Un `save.json` corrupto se recupera desde `save.json.bak`.
- [ ] Si ambos archivos fallan, el juego arranca con datos nuevos sin lanzar excepción.
- [ ] `updatedAtUtc` se actualiza en cada `Save`.
- [ ] Salir de la app en Android dispara un `Save`.
- [ ] `EC.Data` no depende de `EC.Services`.

---

## Decisiones

- **Sí:** JSON con `JsonUtility`. Legible, depurable y sin dependencias.
- **No:** PlayerPrefs. No escala y no se versiona.
- **No:** SQLite. Sobredimensionado para este volumen de datos.
- **Sí:** campos futuros declarados desde el inicio. Evita migraciones tempranas.
- **No:** cifrado. La protección frente a trampas llega con la sincronización en la nube (SPEC 18).
- **Sí:** definición rápida sin aclaración detallada.

---

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| Corte de energía durante la escritura | Escritura atómica con archivo temporal y respaldo. |
| El jugador edita el JSON | Aceptado en esta fase. Se evalúa validación en la nube en SPEC 18. |

---

## Lo que **no** está en esta spec

- Nube, cifrado, economía, tienda y progreso de campaña.
