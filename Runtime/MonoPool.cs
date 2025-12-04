using System.Collections.Generic;
using UnityEngine;

namespace Shafir.MonoPool
{
    /*
     * ┌───────────────────────────────────┐
     * │     MonoPool by Jafarov Jafar     │
     * ├───────────────────────────────────┤
     * │ For poolable object:              │
     * │ 1. Implement IPoolable interface  │
     * │ 2. ???                            │
     * │ 3. PROFIT!!!                      │
     * │ For pooling:                      │
     * │ 1. Use "Get" method to get        │
     * │     object from pool              │
     * │ 2. User ReturnItem method to      │
     * │     return object to pool         │
     * ├──────────────────────────────────-┤
     * │               @2023               │
     * └───────────────────────────────────┘
    */



    /// <summary>
    /// Pool of objects
    /// </summary>
    public static class ShafirMonoPool
    {
        // Containers for each pooled type
        private static Dictionary<IPoolable, Transform> _containers = new Dictionary<IPoolable, Transform>();

        // Dictionary with all pooled objects
        private static Dictionary<IPoolable, List<IPoolable>> _objectsDict = new Dictionary<IPoolable, List<IPoolable>>();

        // Root container with all containers for each prefab
        private static Transform _rootContainer;

        // Dictionary for connecting instantiated items and prefab
        private static Dictionary<IPoolable, IPoolable> _itemToPrefabLink = new Dictionary<IPoolable, IPoolable>();

        private static bool _isInitialized;

        public static void Initialize()
        {
            if (_isInitialized == true)
                return;

            _rootContainer = new GameObject("----- Shafir MonoPool -----").transform;
            _rootContainer.gameObject.AddComponent<DontDestroyOnLoadComponent>();
            _isInitialized = true;
        }

        /// <summary>
        /// Fill pool with objects
        /// </summary>
        /// <param name="prefab">Goal prefab</param>
        /// <param name="count">Count of prefabs to instantiate</param>
        public static void Fill<T>(T prefab, int count) where T : MonoBehaviour, IPoolable
        {
            // if there is no item in dictionary - creates new list
            if (!_objectsDict.ContainsKey(prefab))
            {
                _objectsDict.Add(prefab, new List<IPoolable>());
            }

            for (int i = 0; i < count; i++)
            {
                // create new item and deactivates it
                var item = Get(prefab);
                Return(item);
            }
        }

        /// <summary>
        /// Return all objects to pool
        /// </summary>
        public static void ReturnAll()
        {
            foreach (var objectsList in _objectsDict.Values)
            {
                foreach (var poolable in objectsList)
                {
                    poolable.DeActivate();
                }
            }
        }

        /// <summary>
        /// Get object from pool
        /// </summary>
        /// <param name="prefab">Goal prefab</param>
        /// <returns>Object from pool</returns>
        public static T Get<T>(T prefab) where T : MonoBehaviour, IPoolable => Get(prefab, null);

        /// <summary>
        /// Get object from pool
        /// </summary>
        /// <param name="prefab">Goal prefab</param>
        /// <returns>Object from pool</returns>
        public static T Get<T>(T prefab, Transform container) where T : MonoBehaviour, IPoolable
        {
            T result;

            // if where is no item in dictionary - create new list
            if (!_objectsDict.ContainsKey(prefab))
            {
                _objectsDict.Add(prefab, new List<IPoolable>());
            }

            // finds inactive object in list
            result = _objectsDict[prefab].Find(x => !x.IsActive) as T;

            // if where is no inactive object - creates a new one
            if (result is null)
            {
                result = CreateItem(prefab);
            }

            if (container != null)
            {
                result.transform.SetParent(container);
            }

            // activates object
            result.Activate();

            return result;
        }

        /// <summary>
        /// Return object to pool
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">Object to return</param>
        public static void Return<T>(T item) where T : MonoBehaviour, IPoolable
        {
            T prefab = _itemToPrefabLink[item] as T;

            // generates new container if there is no one for prefab
            if (!_containers.TryGetValue(prefab, out var container))
            {
                Debug.LogWarning("Incorrent return attempt!");

                container = new GameObject().transform;
                container.name = prefab.name;

                _containers.Add(prefab, container);
            }

            item.DeActivate();
            item.transform.SetParent(container);
        }

        // internal method for creating new objects
        private static T CreateItem<T>(T prefab) where T : MonoBehaviour, IPoolable
        {
            if (_isInitialized == false)
                Initialize();

            // generates new container if there is no one for prefab
            if (!_containers.TryGetValue(prefab, out var container))
            {
                container = new GameObject().transform;
                container.name = prefab.name;
                container.SetParent(_rootContainer);

                _containers.Add(prefab, container);
            }

            T instantiatedItem = Object.Instantiate(prefab, container);
            _objectsDict[prefab].Add(instantiatedItem);
            _itemToPrefabLink.Add(instantiatedItem, prefab);

            return instantiatedItem;
        }
    }
}