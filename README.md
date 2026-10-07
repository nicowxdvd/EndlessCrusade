# Endless Crusade

Juego móvil de defensa de carril por oleadas, en 2.5D y ambientado en una fantasía oscura medieval gótica. Un Caballero Templario retirado cruza la aldea, el bosque maldito y la catedral del Señor Vampiro para rescatar al Bebé de la Profecía.

## La historia

El protagonista sirvió años a la Sagrada Orden y presenció los horrores de la guerra contra las criaturas de la noche. Se retiró, marcado por la culpa, y vive apartado en las afueras de la aldea. Allí protege al Bebé de la Profecía, la única salvación frente al Señor Vampiro.

Una noche de tormenta, ebrio y desmayado sobre la mesa, el Templario no puede evitar que un comando de criaturas ataque su cabaña y rapte al niño. Despierta entre los restos de la puerta y sale tras él, sin armadura, con una espada corta y un látigo.

A medida que avanza, recupera su equipo sagrado: armadura pesada, escudo, agua bendita y milagros. Su búsqueda desesperada se convierte en una cruzada de redención.

## El juego

- **Carril único:** el héroe se mueve a izquierda y derecha y defiende una base que cambia en cada capítulo (ruinas de la cabaña, capilla, campamento, portón del monasterio).
- **Combate:** espada, látigo, maza, escudo y ballesta, más agua bendita y milagros con tiempo de recarga.
- **Tropas:** se invocan en tiempo real con el recurso Liderazgo (Fe): Escudero, Campesino, Ballestero, Sacerdote y Paladín.
- **Enemigos:** wargos, murciélagos, goblins, duendes, vampiros, licántropos y trolls de pantano, con un jefe por capítulo.
- **Progresión:** tienda Emporium con mejoras de héroe, tropas y base, y consumibles.
- **Minijuego:** Pachinko con boletos y premios.
- **Monetización:** oro (moneda blanda), reliquias (moneda dura), compras in-app y anuncios con recompensa.

## Capítulos

| Capítulo | Base | Jefe |
| --- | --- | --- |
| 1. Las Afueras | Ruinas de la cabaña | Licántropo Gigante |
| 2. La Aldea | Capilla | Capitán Vampiro |
| 3. El Bosque Maldito | Campamento | Troll de Pantano Gigante |
| 4. La Catedral | Portón del monasterio | Señor Vampiro |

Solo el capítulo 1 está definido en la historia original. Los capítulos 2 a 4 son una propuesta que se confirma en sus specs.

## Tecnología

- Unity 6 LTS (serie 6000.0), C# y Universal Render Pipeline.
- Android primero (IL2CPP, ARM64, API mínima 26). iOS queda para más adelante.
- Patrones: máquinas de estados finitos, object pooling, arquitectura por componentes y ScriptableObjects para los datos.
- Servicios: Firebase (Auth anónima, Firestore, Analytics, Crashlytics), Unity IAP y AdMob.
- Arquitectura modular con assembly definitions: `EC.Core`, `EC.Data`, `EC.Gameplay`, `EC.UI`, `EC.Services`.

## Fases de desarrollo

El trabajo se divide en 30 especificaciones en `specs/`. Cada una se define con `/spec` y se implementa con `/spec-impl`.

### Fase 0 — Fundaciones

| Spec | Contenido |
| --- | --- |
| [01](specs/01-proyecto-base.md) | Proyecto Unity, URP, carpetas, assemblies y build Android |
| [02](specs/02-carril-escena-25d.md) | Carril 2.5D, cámara, parallax y lluvia |
| [03](specs/03-entidades-fsm-componentes.md) | FSM, componentes y datos de unidades |
| [04](specs/04-object-pooling.md) | Object pooling |

### Fase 1 — Prototipo MVP

| Spec | Contenido |
| --- | --- |
| [05](specs/05-heroe-templario.md) | Héroe Templario con espada y látigo |
| [06](specs/06-enemigos-nivel-1.md) | Wargo y murciélago |
| [07](specs/07-base-defendible.md) | Base con resistencia |
| [08](specs/08-oleadas.md) | Oleadas, victoria y derrota |
| [09](specs/09-hud-combate.md) | HUD, pausa y resultados |

### Fase 2 — Core gameplay y nivel 1

| Spec | Contenido |
| --- | --- |
| [10](specs/10-habilidades-cooldown.md) | Habilidades con cooldown, agua bendita |
| [11](specs/11-jefe-licantropo.md) | Jefe Licántropo Gigante |
| [12](specs/12-nivel-1-despertar.md) | Nivel 1 completo con historia y tutorial |
| [13](specs/13-liderazgo-tropas.md) | Liderazgo e invocación de tropas |

### Fase 3 — Meta-game

| Spec | Contenido |
| --- | --- |
| [14](specs/14-guardado-local.md) | Guardado local versionado |
| [15](specs/15-economia-recompensas.md) | Oro, reliquias y recompensas |
| [16](specs/16-mapa-campana.md) | Mapa de campaña |
| [17](specs/17-emporium-mejoras.md) | Tienda Emporium |
| [18](specs/18-firebase-sync.md) | Firebase: nube, analítica y errores |

### Fase 4 — Contenido de campaña

| Spec | Contenido |
| --- | --- |
| [19](specs/19-tropas-adicionales.md) | Campesino, Ballestero, Sacerdote y Paladín |
| [20](specs/20-enemigos-adicionales.md) | Vampiro, goblin, duende, licántropo y troll |
| [21](specs/21-equipo-recuperado.md) | Equipo y milagros recuperados |
| [22](specs/22-capitulo-aldea.md) | Capítulo 2: La Aldea |
| [23](specs/23-capitulo-bosque-maldito.md) | Capítulo 3: El Bosque Maldito |
| [24](specs/24-capitulo-catedral.md) | Capítulo 4: La Catedral |

### Fase 5 — Monetización y minijuego

| Spec | Contenido |
| --- | --- |
| [25](specs/25-iap.md) | Compras in-app |
| [26](specs/26-rewarded-ads.md) | Anuncios con recompensa |
| [27](specs/27-minijuego-pachinko.md) | Minijuego Pachinko |

### Fase 6 — Pulido y release

| Spec | Contenido |
| --- | --- |
| [28](specs/28-audio.md) | Música y efectos |
| [29](specs/29-optimizacion-movil.md) | Optimización y calidad automática |
| [30](specs/30-release-android.md) | Release en Google Play (testing interno) |

## Flujo de trabajo

El proyecto usa Gitflow.

- `main`: versiones publicadas.
- `develop`: integración.
- `feature/<nombre>` y `bugfix/<nombre>`: salen de `develop`.
- `release/<version>` y `hotfix/<nombre>`: salen de `main`.

Ningún commit va directo a `main` ni a `develop`.

Para cada spec: definirla con `/spec`, revisarla y marcarla como Aprobado, e implementarla con `/spec-impl NN-slug` en su rama `feature/`.

## Estructura del repositorio

```
.
├── Pre-proyecto/      documentos de diseño e imágenes de inspiración
├── specs/             especificaciones 01 a 30
├── EndlessCrusade/    proyecto Unity (se crea en la spec 01)
└── README.md
```

Las imágenes de `Pre-proyecto/` son solo inspiración visual.

## Requisitos para desarrollar

- Unity Hub y Unity 6 LTS con módulos Android Build Support, OpenJDK y Android SDK & NDK Tools.
- Un dispositivo Android con depuración USB o un emulador.
- Cuenta de Firebase y de Google Play Console (a partir de las specs 18 y 30).

## Estado

Fase de especificación: las 30 specs están en estado Borrador. Todavía no hay código.
