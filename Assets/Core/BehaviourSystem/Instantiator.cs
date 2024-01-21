using System.Linq;
using UnityEngine;

namespace Core
{
    public class Instantiator
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoadRuntimeMethod()
        {
            var prefabs = Resources.LoadAll<GameObject>("OnLoad");
            foreach (var prefab in prefabs)
            {
                Object.Instantiate(prefab);
            }
        }
    }
}