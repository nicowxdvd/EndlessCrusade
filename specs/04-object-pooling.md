# SPEC 04 — Object pooling

> **Estado:** Implementado
> **Depende de:** SPEC 01
> **Fecha:** 2026-10-06
> **Objetivo:** Implementar un servicio de pooling genérico para enemigos, tropas, proyectiles y efectos visuales sin asignaciones de memoria durante el combate.

---

## Alcance

**Dentro:**

- `PoolService` en `EC.Core` basado en `UnityEngine.Pool.ObjectPool<GameObject>`.
- Interfaz `IPoolable` con `OnSpawn` y `OnDespawn`.
- Prewarm por prefab.
- Test PlayMode que mide asignaciones de memoria.

**Fuera de alcance (para specs futuras):**

- Pools concretos de enemigos, tropas y proyectiles (se usan desde SPEC 06 en adelante).
- Pooling de elementos de UI.
- Carga por Addressables.

---

## Modelo de datos

```csharp
public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
}

public class PoolService : MonoBehaviour
{
    public void Prewarm(GameObject prefab, int count);
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation);
    public void Release(GameObject instance);
}
```

Reglas:

- Cada instancia lleva un componente `PooledObject` con referencia a su prefab de origen.
- `Release` de un objeto no creado por el pool lo destruye y escribe un warning.
- `Get` con el pool vacío instancia una instancia nueva. Prewarm reduce esos casos, no los prohíbe.
- `PoolService` vive en la escena `Boot` con `DontDestroyOnLoad`.

---

## Plan de implementación

1. Crear `IPoolable` y `PooledObject` en `EC.Core`.
2. Crear `PoolService` con un diccionario `prefab -> ObjectPool`. Verificar: compila.
3. Implementar `Prewarm`, `Get` y `Release`, llamando `OnSpawn` y `OnDespawn` en los `IPoolable` del objeto.
4. Agregar `PoolService` a la escena `Boot` con `DontDestroyOnLoad`.
5. Crear el test PlayMode: el propio test crea el `PoolService` por código, sin cargar `Boot`. 1000 ciclos `Get`/`Release` de un prefab de prueba. Medir con `ProfilerRecorder` la categoría `GC Allocated In Frame`. Verificar: 0 bytes en el bucle.
6. Agregar un contador de instancias activas por prefab para depuración (solo editor).

---

## Criterios de aceptación

- [x] `Get` tras `Prewarm(prefab, 10)` no instancia objetos nuevos en las primeras 10 llamadas.
- [x] `Release` desactiva el objeto y lo devuelve al pool.
- [x] `OnSpawn` y `OnDespawn` se llaman una vez por ciclo.
- [x] El test PlayMode de 1000 ciclos registra 0 bytes asignados por frame.
- [x] `Release` de un objeto ajeno al pool no lanza excepción y escribe un warning.
- [x] El pool sobrevive a la carga de `Boot` a `Main`.

---

## Decisiones

- **Sí:** `UnityEngine.Pool.ObjectPool<T>` incluido en Unity. Cero dependencias nuevas.
- **No:** pool propio desde cero. Reinventa algo que el motor ya ofrece.
- **Sí:** un `PoolService` único en `Boot`. Todos los sistemas lo comparten.
- **Sí:** definición rápida sin aclaración detallada.

---

## Lo que **no** está en esta spec

- Pools de enemigos, tropas, proyectiles y VFX concretos.
- Pooling de UI.
- Addressables.
