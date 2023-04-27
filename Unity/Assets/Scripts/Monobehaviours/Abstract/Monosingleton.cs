
using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Core
{
    public class Monosingleton<T> : MonoBehaviour where T : Monosingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (!IsInitialized)
                    Debug.LogError("[Monosingleton<" + typeof(T) + ">] _instance not initialized");
                return _instance;
            }
        }

        public static bool IsInitialized
        {
            get
            {
                return _instance != null;
            }
        }

        private void Awake()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("[Monosingleton<" + typeof(T) + ">] Attempting to create a second instance. Removing this instance.");
                Destroy(this.gameObject);
            }
            else
            {
                _instance = (T)this;
            }

            Init();
        }

        protected virtual void Init(){}
    }
}
