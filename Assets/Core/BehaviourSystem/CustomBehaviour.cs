using System;
using UnityEngine;

namespace Core
{
    public class CustomBehaviour : MonoBehaviour, IManagedObject
    {
        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }
    }
}