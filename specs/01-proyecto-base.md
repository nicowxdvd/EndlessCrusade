# SPEC 01 — Proyecto base Unity con build Android

> **Estado:** Aprobado
> **Depende de:** ninguna
> **Fecha:** 2026-10-06
> **Objetivo:** Crear el proyecto Unity 6 LTS con URP, estructura de carpetas, assembly definitions y Force Text, con una build Android que arranca en la escena Boot y carga Main.

---

## Por qué existe esta spec

Todas las specs siguientes (02–30) escriben código y escenas dentro de este proyecto. Los assembly definitions y la estructura de carpetas se fijan acá porque cambiarlos después obliga a mover archivos y reparar referencias. El package name se fija acá porque cambiarlo después rompe Firebase (SPEC 18) y Play Console (SPEC 30).

---

## Alcance

**Dentro:**

- Proyecto Unity 6 LTS (serie 6000.0) con plantilla Universal 3D (URP), creado con Unity Hub.
- Proyecto en la subcarpeta `EndlessCrusade/` del repo.
- Serialización de assets en Force Text y metadatos visibles.
- Estructura de carpetas bajo `Assets/_Project/`.
- Assembly definitions por módulo, más dos de tests.
- Paquetes: Input System, Cinemachine, Test Framework y uGUI (incluye TextMeshPro).
- Escenas `Boot` (build index 0) y `Main` (build index 1, vacía).
- Script `Bootstrapper` en `Boot` que escribe un log y carga `Main`.
- Configuración Android: IL2CPP, ARM64, landscape, package `com.nicolas.endlesscrusade`.
- Calidad visual base en URP: espacio de color Linear, Vulkan con fallback a OpenGLES3, HDR, MSAA 4x y post-procesado (Bloom, Vignette, Color Adjustments) activo en un Volume global.
- Build APK que arranca en `Boot` y llega a `Main`.

**Fuera de alcance (para specs futuras):**

- Escena de carril, cámara 2.5D y parallax (SPEC 02).
- FSM, componentes y ScriptableObjects de datos (SPEC 03).
- Object pooling (SPEC 04).
- Menú principal, pantalla de carga y UI real.
- Firebase, IAP, AdMob (SPEC 18, 25, 26).
- Firma de release, iconos y splash (SPEC 30).
- Build para iOS.
- CI y scripts de build por línea de comandos.

---

## Modelo de datos

Esta feature no introduce estructuras de datos. Solo estructura de archivos y configuración.

Estructura de carpetas:

```
EndlessCrusade/
├── Assets/
│   └── _Project/
│       ├── Scenes/            Boot.unity, Main.unity
│       ├── Scripts/
│       │   ├── Core/          EC.Core.asmdef, Bootstrapper.cs
│       │   ├── Gameplay/      EC.Gameplay.asmdef
│       │   ├── Data/          EC.Data.asmdef
│       │   ├── UI/            EC.UI.asmdef
│       │   └── Services/      EC.Services.asmdef
│       ├── Tests/
│       │   ├── EditMode/      EC.Tests.EditMode.asmdef
│       │   └── PlayMode/      EC.Tests.PlayMode.asmdef
│       ├── Prefabs/
│       ├── ScriptableObjects/
│       ├── Art/
│       ├── Audio/
│       └── Settings/          URP assets (creados por la plantilla)
├── Packages/
└── ProjectSettings/
```

Referencias entre assemblies:

| Assembly | Referencia a |
| --- | --- |
| `EC.Core` | ninguno |
| `EC.Data` | `EC.Core` |
| `EC.Gameplay` | `EC.Core`, `EC.Data` |
| `EC.UI` | `EC.Core`, `EC.Data` |
| `EC.Services` | `EC.Core`, `EC.Data` |
| `EC.Tests.EditMode` | todos los `EC.*` |
| `EC.Tests.PlayMode` | todos los `EC.*` |

`EC.Gameplay`, `EC.UI` y `EC.Services` no se referencian entre sí. Se comunican por tipos de `EC.Core` o `EC.Data`.

Configuración de proyecto:

- `ProjectSettings/EditorSettings.asset`: `Asset Serialization Mode = Force Text`, `Version Control Mode = Visible Meta Files`.
- Player Settings Android: `Scripting Backend = IL2CPP`, `Target Architectures = ARM64`, `Minimum API Level = 26`, `Default Orientation = Landscape Left`, `Application Identifier = com.nicolas.endlesscrusade`.
- `Product Name = Endless Crusade`.
- Player Settings: `Color Space = Linear`, `Auto Graphics API = off`, orden `Vulkan`, `OpenGLES3`.
- URP Asset de calidad alta (`Assets/_Project/Settings/`): `HDR = on`, `MSAA = 4x`, `Render Scale = 1.0`, `Post Processing = on`.
- Volume global en `Main` con `Bloom`, `Vignette` y `Color Adjustments` para el tono gótico (valores finos en SPEC 02).

Contrato del `Bootstrapper`:

```csharp
// Assets/_Project/Scripts/Core/Bootstrapper.cs
// Awake/Start: Debug.Log("[Boot] Endless Crusade iniciado"); SceneManager.LoadScene("Main");
```

---

## Plan de implementación

1. Instalar Unity Hub y Unity 6 LTS (serie 6000.0) con módulos Android Build Support, OpenJDK y Android SDK & NDK Tools. Verificar: `Unity Hub > Installs` muestra la versión con módulo Android.
2. Crear el proyecto con plantilla Universal 3D en `EndlessCrusade/` dentro del repo. Verificar: Unity abre el proyecto y la escena de ejemplo se renderiza sin errores en consola.
3. Fijar `Force Text` y `Visible Meta Files` en `Project Settings > Editor`. Verificar: `git diff` muestra `m_SerializationMode: 2` y `m_Mode: Visible Meta Files` en `ProjectSettings/VersionControlSettings.asset`.
4. Instalar Input System, Cinemachine, Test Framework y uGUI desde Package Manager. Aceptar el reinicio del editor al activar el Input System. Verificar: `Packages/manifest.json` lista los cuatro paquetes.
5. Crear las carpetas de `Assets/_Project/` según el modelo de datos, con un archivo `.gitkeep` en las carpetas vacías (`Prefabs`, `ScriptableObjects`, `Art`, `Audio`).
6. Crear los cinco asmdef de módulo con las referencias de la tabla. Verificar: el proyecto compila sin errores en consola.
7. Crear los dos asmdef de tests con las referencias de la tabla, `Editor` como única plataforma en `EC.Tests.EditMode` y todas las plataformas en `EC.Tests.PlayMode`, y `UNITY_INCLUDE_TESTS` en constraints. Verificar: `Window > General > Test Runner` abre ambas pestañas sin errores.
8. Crear las escenas `Boot` y `Main` en `Assets/_Project/Scenes/`. Agregarlas a `Build Settings` con `Boot` en índice 0 y `Main` en índice 1.
9. Crear `Bootstrapper.cs` en `EC.Core` y colocarlo en un GameObject de `Boot`. Verificar: Play Mode desde `Boot` imprime `[Boot] Endless Crusade iniciado` y carga `Main`.
10. Configurar Player Settings Android y calidad URP según el modelo de datos, y agregar el Volume global a `Main`. Verificar: `Build Settings > Android > Build` no muestra errores de configuración y el Game view de `Main` muestra Bloom y Vignette.
11. Generar el APK con `Build Settings > Android > Build`, instalarlo con `adb install` en un dispositivo o emulador y leer `adb logcat -s Unity`. Verificar: aparece `[Boot] Endless Crusade iniciado`.
12. Confirmar que `git status` solo muestra archivos esperados (sin `Library/`, `Temp/`, `Logs/`, `UserSettings/`) y hacer commit.

---

## Criterios de aceptación

- [x] `EndlessCrusade/ProjectSettings/ProjectVersion.txt` contiene una versión `6000.0.x`.
- [x] `EndlessCrusade/Packages/manifest.json` incluye `com.unity.render-pipelines.universal`, `com.unity.inputsystem`, `com.unity.cinemachine`, `com.unity.test-framework` y `com.unity.ugui`.
- [x] `EditorSettings.asset` tiene `m_SerializationMode: 2` (Force Text).
- [x] Existe un archivo `.meta` junto a cada archivo y carpeta de `Assets/`.
- [x] Los siete archivos `.asmdef` existen en las rutas del modelo de datos con las referencias de la tabla.
- [ ] El proyecto abre en el editor y la consola muestra 0 errores de compilación. Los warnings esperados son los de assemblies vacíos (`will not be compiled, because no scripts`) y `HDRP-Editor-ref.asmref has no target assembly definition` de Cinemachine.
- [x] `Build Settings` lista `Boot` en índice 0 y `Main` en índice 1.
- [ ] Play Mode desde `Boot` imprime `[Boot] Endless Crusade iniciado` y la escena activa pasa a `Main`.
- [x] Player Settings Android muestran IL2CPP, ARM64, Min API 26, Landscape Left y el identificador `com.nicolas.endlesscrusade`.
- [x] Player Settings muestran `Color Space = Linear` y Graphics APIs Android en orden Vulkan, OpenGLES3.
- [ ] El URP Asset activo tiene HDR on, MSAA 4x y Post Processing on.
- [ ] `Main` contiene un Volume global con Bloom, Vignette y Color Adjustments.
- [x] La build Android genera un `.apk` sin errores.
- [x] El APK instalado en un dispositivo o emulador arranca en landscape y `adb logcat -s Unity` muestra `[Boot] Endless Crusade iniciado`.
- [x] `git status` no lista `Library/`, `Temp/`, `Logs/`, `UserSettings/` ni archivos `.apk`.

---

## Decisiones

- **Sí:** proyecto en subcarpeta `EndlessCrusade/`. Mantiene la raíz limpia con `specs/`, `Pre-proyecto/` y `.gitignore`.
- **No:** proyecto en la raíz del repo. Mezcla archivos de Unity con documentación.
- **Sí:** plantilla Universal 3D. El juego es 2.5D con fondos 3D y cámara inclinada.
- **No:** plantilla Universal 2D. No encaja con la perspectiva 2.5D.
- **No:** generar el proyecto a mano. Unity regenera metadatos y puede romper el proyecto al abrirlo.
- **Sí:** asmdef por módulo (`Core`, `Gameplay`, `Data`, `UI`, `Services`). Calza con el diagrama Core Gameplay / Game Data / Services del documento técnico y fuerza dependencias en una dirección.
- **No:** un solo asmdef. Compila todo junto y no impide dependencias cruzadas.
- **Sí:** `Boot` en índice 0 y `Main` vacía. Valida el flujo de carga de escenas para las specs siguientes.
- **No:** escenas de menú y gameplay ahora. Exceden el alcance.
- **Sí:** IL2CPP + ARM64. Google Play exige 64-bit y la build de desarrollo queda igual a la de release.
- **No:** Mono + ARMv7. Play Store rechaza 32-bit solo.
- **Sí:** Min API 26 (Android 8), confirmado por Nico. Soporta Vulkan y cubre gama media y baja actual.
- **Sí:** calidad visual alta desde el inicio (Linear, HDR, MSAA 4x, post-procesado). Nico prioriza que el juego se vea bonito. El costo en gama baja se mide en SPEC 29.
- **No:** bajar calidad por defecto para gama baja. Se agregan niveles de calidad en SPEC 29 si el perfilado lo exige.
- **Sí:** landscape y `com.nicolas.endlesscrusade`. El carril es horizontal. Cambiar el package después rompe Firebase y Play Console.
- **Sí:** Input System, Cinemachine, Test Framework y uGUI. Se instalan ahora para no reiniciar el editor ni migrar después.
- **Sí:** verificación en Play Mode y en APK real con `adb logcat`.
- **No:** script de build por línea de comandos. Se hará en otra spec si se agrega CI.

---

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| Unity no está instalado en la Mac de Nico | El paso 1 instala Unity Hub y el módulo Android antes de crear el proyecto. |
| Fallan SDK/NDK/JDK de Android en la build | Usar los módulos instalados por Unity Hub (OpenJDK, SDK & NDK Tools), no instalaciones externas. |
| No hay dispositivo Android ni emulador | Crear un AVD en Android Studio, o usar un teléfono con depuración USB. Sin uno de los dos no se cumple el criterio del APK. |
| Cambiar de serie de Unity (6000.1+) rompe compatibilidad | `ProjectVersion.txt` queda versionado. Subir de serie requiere su propia spec. |
| Conflictos YAML en escenas con worktrees paralelos | Force Text (esta spec) más UnityYAMLMerge y un dueño por escena en specs siguientes. |

---

## Lo que **no** está en esta spec

- Escena de carril, cámara 2.5D y arte (SPEC 02).
- Cualquier lógica de gameplay, FSM, pooling o datos (SPEC 03, 04).
- Menú principal, pantalla de carga y UI.
- Firebase, compras in-app y anuncios.
- Firma de release, iconos, splash y Play Console.
- iOS y CI.

Cada uno de esos puntos, si se hace, va en su propia spec.
