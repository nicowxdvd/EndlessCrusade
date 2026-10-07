# SPEC 18 — Firebase: autenticación, nube y analítica

> **Estado:** Borrador
> **Depende de:** SPEC 14
> **Fecha:** 2026-10-06
> **Objetivo:** Integrar Firebase con Auth anónima, sincronización del guardado en Firestore, Analytics y Crashlytics.

---

## Alcance

**Dentro:**

- SDK de Firebase para Unity (Auth, Firestore, Analytics, Crashlytics).
- `AuthService`: inicio de sesión anónimo.
- `CloudSaveService`: sube y descarga el `SaveData` del jugador.
- Sincronización al iniciar y al terminar cada nivel, sin bloquear el juego.
- Resolución de conflictos por `updatedAtUtc`.
- Reglas de Firestore versionadas en `EndlessCrusade/Firebase/firestore.rules`.
- Eventos de Analytics.
- Crashlytics activo en builds de Android.
- Servicios simulados (`Fake`) para el editor y para jugar sin red.

**Fuera de alcance (para specs futuras):**

- Vincular cuenta con Google o correo.
- Ranking y multijugador.
- Validación de compras en servidor.
- iOS.

---

## Modelo de datos

```csharp
public interface IAuthService
{
    string UserId { get; }
    System.Threading.Tasks.Task<bool> SignInAnonymouslyAsync();
}

public interface ICloudSaveService
{
    System.Threading.Tasks.Task<bool> SyncAsync();
}
```

Documento de Firestore: `users/{uid}` con el campo `save` (JSON de `SaveData` como string) y `updatedAtUtc`.

Reglas de seguridad:

```
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /users/{uid} {
      allow read, write: if request.auth != null && request.auth.uid == uid;
    }
  }
}
```

Eventos de Analytics: `level_start`, `level_end` (con resultado y oleada alcanzada), `upgrade_purchased`, `tutorial_complete`.

Archivos de configuración: `google-services.json` en `Assets/` (no se versiona; se describe en el README del directorio `Firebase/`).

---

## Plan de implementación

1. Nico crea el proyecto en la consola de Firebase, registra la app Android `com.nicolas.endlesscrusade` y descarga `google-services.json`. Agregar ese archivo a `.gitignore`.
2. Instalar el SDK de Firebase para Unity (módulos Auth, Firestore, Analytics, Crashlytics) y resolver dependencias de Android.
3. Crear `IAuthService`, `ICloudSaveService` y sus implementaciones `Fake` en `EC.Services`. Verificar: el juego corre en el editor con los `Fake`.
4. Implementar `AuthService` con Auth anónima. Verificar: aparece un usuario en la consola de Firebase.
5. Implementar `CloudSaveService.SyncAsync`: descarga, compara `updatedAtUtc`, el más nuevo gana y el perdedor se guarda como `save.conflict.json`.
6. Llamar a `SyncAsync` tras el arranque y tras cada `LevelEnded`, con tiempo máximo de 10 segundos y sin bloquear la UI.
7. Publicar las reglas en Firestore y guardar `firestore.rules` en el repo.
8. Agregar los eventos de Analytics y activar Crashlytics.
9. Probar en el dispositivo: jugar un nivel, verificar el documento en la consola, borrar `save.json` local y confirmar que el siguiente arranque restaura el progreso desde la nube.

---

## Criterios de aceptación

- [ ] El juego arranca y funciona completo sin conexión.
- [ ] Con conexión, el primer arranque crea un usuario anónimo y un documento `users/{uid}`.
- [ ] Terminar un nivel actualiza el documento en la nube.
- [ ] Si la nube tiene un `updatedAtUtc` más nuevo, el guardado local se reemplaza y el anterior queda en `save.conflict.json`.
- [ ] La sincronización nunca bloquea la interfaz más de un frame.
- [ ] Las reglas de Firestore rechazan leer el documento de otro usuario.
- [ ] Los eventos de Analytics aparecen en DebugView.
- [ ] Un crash de prueba aparece en Crashlytics.

---

## Decisiones

- **Sí:** Firebase, decidido por Nico.
- **Sí:** autenticación anónima. Sin fricción de registro para un juego móvil.
- **No:** vincular cuenta ahora. Con Auth anónima, desinstalar pierde el acceso a la nube; se acepta en esta fase y se documenta como limitación.
- **Sí:** el juego es offline primero. La nube nunca bloquea el juego.
- **Sí:** el guardado más reciente gana y el otro se conserva. Pierde menos progreso que descartarlo.
- **Sí:** definición rápida sin aclaración detallada.

---

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| Pérdida de cuenta anónima al desinstalar | Documentado. Vincular cuenta en una spec futura. |
| Conflictos del SDK de Firebase con el resolver de Android | Usar la versión del SDK compatible con Unity 6 y verificar en la build. |
| Costos de Firestore | Un solo documento por usuario y sincronización solo en eventos clave. |

---

## Lo que **no** está en esta spec

- Vinculación de cuentas, ranking, validación de compras en servidor e iOS.
