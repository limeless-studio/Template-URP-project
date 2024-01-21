namespace Core
{
    // Actor:
    public enum DamageCause
    {
        DamagedByActor,
        DamagedByEnvironment,
        DamagedByOther
    }
    
    // Input:
    public enum InputType
    {
        Keyboard,
        Mouse,
        Gamepad
    }
    
    public enum ButtonType
    {
        Button,
        Axis
    }
    
    public enum ButtonState : byte
    {
        None,
        Pressed,
        Released,
        Held
    }
}