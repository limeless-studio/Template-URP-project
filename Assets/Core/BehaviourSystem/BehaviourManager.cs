using UnityEngine;

namespace Core
{
    public class BehaviourManager : MonoBehaviour
    {
        public static BehaviourManager Instance { get; private set; }
        private readonly FastRemoveList<IUpdate> _updateList = new FastRemoveList<IUpdate>();
        private readonly FastRemoveList<ILateUpdate> _lateUpdateList = new FastRemoveList<ILateUpdate>();
        private readonly FastRemoveList<IFixedUpdate> _fixedUpdateList = new FastRemoveList<IFixedUpdate>();
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }


        public void Register(IManagedObject managedObj)
        {
            if (managedObj is IUpdate update)
            {
                _updateList.Add(update);
            }
            
            if (managedObj is ILateUpdate lateUpdate)
            {
                _lateUpdateList.Add(lateUpdate);
            }
            
            if (managedObj is IFixedUpdate fixedUpdate)
            {
                _fixedUpdateList.Add(fixedUpdate);
            }
            
        }
        
        public void Unregister(IManagedObject managedObj)
        {
            if (managedObj is IUpdate update)
            {
                _updateList.Remove(update);
            }
            
            if (managedObj is ILateUpdate lateUpdate)
            {
                _lateUpdateList.Remove(lateUpdate);
            }
            
            if (managedObj is IFixedUpdate fixedUpdate)
            {
                _fixedUpdateList.Remove(fixedUpdate);
            }
        }

        public void Clear()
        {
            _updateList.Clear();
            _lateUpdateList.Clear();
            _fixedUpdateList.Clear();
        }
    }
}