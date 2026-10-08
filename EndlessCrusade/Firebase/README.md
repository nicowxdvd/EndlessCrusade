# Firebase

Pasos manuales para activar la nube. Sin ellos el juego usa los servicios `Fake` y funciona completo sin red.

1. Crear el proyecto en la consola de Firebase y registrar la app Android `com.nicolas.endlesscrusade`.
2. Descargar `google-services.json` y copiarlo en `Assets/`. El archivo está en `.gitignore`: no se versiona.
3. Importar el SDK de Firebase para Unity (Auth, Firestore, Analytics, Crashlytics) y resolver dependencias de Android.
4. Agregar el símbolo de scripting `EC_FIREBASE` en Player Settings (Android). Eso compila el asmdef `EC.Services.Firebase`, que reemplaza los `Fake` al arrancar.
5. Publicar `firestore.rules` en Firestore (`firebase deploy --only firestore:rules`).

Limitación: una cuenta anónima se pierde al desinstalar la app. Vincular cuenta queda para una spec futura.

Documento: `users/{uid}` con `save` (JSON de `SaveData`) y `updatedAtUtc`. Gana el guardado más reciente; el perdedor queda en `save.conflict.json` junto al guardado local.
