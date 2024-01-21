namespace Core
{
    public interface IManagedObject
    {
    }
    
    public static class ManagedExtension
    {
        public static void Register(this IManagedObject managedObject)
        {
            BehaviourManager.Instance.Register(managedObject);
        }
        
        public static void Unregister(this IManagedObject managedObject)
        {
            BehaviourManager.Instance.Unregister(managedObject);
        }
    }
}