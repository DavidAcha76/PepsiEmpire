using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 Move;
    public Vector2 LookDelta; 
    public float CameraYaw;
    public float CameraPitch;
    public NetworkButtons Buttons;

    public enum ButtonsMask
    {
        Jump = 0,
        Run = 1,
        Crouch = 2,
        Interact = 3
    }

    public bool IsJumpPressed => Buttons.IsSet(ButtonsMask.Jump);
    public bool IsRunPressed => Buttons.IsSet(ButtonsMask.Run);
    public bool IsCrouchPressed => Buttons.IsSet(ButtonsMask.Crouch);
    public bool IsInteractPressed => Buttons.IsSet(ButtonsMask.Interact);
}
