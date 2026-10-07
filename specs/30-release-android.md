# SPEC 30 — Release en Android

> **Estado:** Borrador
> **Depende de:** SPEC 29
> **Fecha:** 2026-10-06
> **Objetivo:** Preparar firma, iconos, splash, política de privacidad y subida a Play Console en testing interno con un bundle firmado.

---

## Alcance

**Dentro:**

- Keystore de release creado por Nico y guardado fuera del repositorio.
- Configuración de firma en Player Settings mediante variables de entorno o archivo local no versionado.
- Build `.aab` con IL2CPP ARM64.
- Esquema de versiones: `bundleVersion` semántico y `bundleVersionCode` incremental.
- Iconos adaptativos y pantalla de inicio (splash).
- Método de editor `BuildScript.BuildRelease` para generar el `.aab` por línea de comandos.
- Política de privacidad publicada en una URL.
- Ficha en Play Console: datos de la app, clasificación de contenido, formulario de seguridad de datos, productos de SPEC 25.
- Reemplazo de IDs de prueba de AdMob por IDs de producción.
- Subida a la pista de testing interno.

**Fuera de alcance (para specs futuras):**

- Publicación en producción y marketing.
- iOS.
- Integración continua.

---

## Modelo de datos

```csharp
public static class BuildScript
{
    public static void BuildRelease();
}
```

Convenciones:

- `bundleVersionCode`: formato `MMmmpp` (mayor, menor, parche), por ejemplo versión 1.0.3 es 10003.
- Variables de entorno: `EC_KEYSTORE_PATH`, `EC_KEYSTORE_PASS`, `EC_KEY_ALIAS`, `EC_KEY_PASS`.
- El keystore, `google-services.json` y las contraseñas no se versionan.

---

## Plan de implementación

1. Nico genera el keystore de release y lo respalda en dos lugares fuera del repo. Perderlo impide actualizar la app.
2. Activar Play App Signing al crear la app en Play Console.
3. Crear `BuildScript.BuildRelease`, que lee las variables de entorno y genera el `.aab`.
4. Fijar `bundleVersion` y `bundleVersionCode` según la convención.
5. Crear el icono adaptativo (primer plano y fondo) y el splash con el logo.
6. Redactar y publicar la política de privacidad (menciona Firebase, AdMob y compras). Anotar la URL.
7. Reemplazar los IDs de AdMob por los de producción.
8. Completar la ficha en Play Console y el formulario de seguridad de datos.
9. Subir el `.aab` a testing interno e instalarlo desde Google Play en un dispositivo.
10. Ejecutar las compras de prueba de SPEC 25 con cuentas de licencia.

---

## Criterios de aceptación

- [ ] `BuildScript.BuildRelease` genera un `.aab` firmado sin intervención manual.
- [ ] El `.aab` usa IL2CPP y ARM64, y apunta al nivel de API que Play exige a la fecha de la subida.
- [ ] El keystore, las contraseñas y `google-services.json` no aparecen en `git status`.
- [ ] El icono adaptativo y el splash se ven bien en el dispositivo.
- [ ] La política de privacidad es accesible por una URL pública.
- [ ] La app se instala desde la pista de testing interno de Google Play.
- [ ] Las compras de SPEC 25 funcionan con una cuenta de prueba.
- [ ] Los anuncios usan IDs de producción solo en el build de release.

---

## Decisiones

- **Sí:** Play App Signing. Si se pierde la clave de subida, Google permite recuperarla.
- **Sí:** `.aab` en lugar de `.apk`. Es obligatorio en Play Store.
- **Sí:** secretos por variables de entorno y fuera del repo. Evita filtrar credenciales.
- **No:** integración continua ahora. Se evalúa en una spec futura.
- **Sí:** definición rápida sin aclaración detallada.

---

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| Cuenta de desarrollador de Play no creada | Se crea antes del paso 2. Tiene costo único y verificación de identidad. |
| Rechazo por política de datos o anuncios | Política de privacidad y formulario de datos completos antes de subir. |
| Pérdida del keystore | Respaldo en dos lugares y Play App Signing. |

---

## Lo que **no** está en esta spec

- Publicación en producción, marketing, iOS e integración continua.
