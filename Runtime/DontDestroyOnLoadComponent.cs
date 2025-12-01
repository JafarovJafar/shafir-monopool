using UnityEngine;

namespace Shafir.MonoPool
{
    internal class DontDestroyOnLoadComponent : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}