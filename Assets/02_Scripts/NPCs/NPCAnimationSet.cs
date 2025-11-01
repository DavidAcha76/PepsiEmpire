using UnityEngine;

[CreateAssetMenu(fileName = "NPCAnimationSet", menuName = "NPCs/Animation Set")]
public class NPCAnimationSet : ScriptableObject
{
    [Header("Controller base (genérico)")]
    public RuntimeAnimatorController baseController;

    [Header("Clips de este modelo")]
    public AnimationClip sadIdle;
    public AnimationClip sadWalk;
    public AnimationClip happyIdle;
    public AnimationClip happyWalk;
    public AnimationClip drinking;
}
