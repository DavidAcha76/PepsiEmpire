using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class NPCAnimatorDriver : MonoBehaviour
{
    public NPCAnimationSet animationSet;
    public string speedParam = "Speed";
    public string happyParam = "IsHappy";
    public string drinkTrigger = "Drink";

    private Animator _anim;
    private NavMeshAgent _agent;
    private AnimatorOverrideController _override;

    public bool IsHappy { get; private set; }

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (!animationSet || !animationSet.baseController)
        {
            Debug.LogError($"{name}: Falta AnimationSet o BaseController.");
            return;
        }
        _override = new AnimatorOverrideController(animationSet.baseController);
        _anim.runtimeAnimatorController = _override;
        ApplyClips(animationSet);

        // Opcional: log para confirmar que sí hay estados
         Debug.Log($"{name}: override clips = {_override.overridesCount}");

        SetHappy(false);
    }


    void Update()
    {
        _anim.SetFloat(speedParam, _agent ? _agent.velocity.magnitude : 0f);
        _anim.SetBool(happyParam, IsHappy);
    }

    void ApplyClips(NPCAnimationSet set)
    {
        _override["SadIdle_PLACEHOLDER"] = set.sadIdle;
        _override["SadWalk_PLACEHOLDER"] = set.sadWalk;
        _override["HappyIdle_PLACEHOLDER"] = set.happyIdle;
        _override["HappyWalk_PLACEHOLDER"] = set.happyWalk;
        _override["Drink_PLACEHOLDER"] = set.drinking;
    }


    public void SetHappy(bool v)
    {
        IsHappy = v;
        _anim.SetBool(happyParam, v);
    }

    public void PlayDrink()
    {
        _anim.ResetTrigger(drinkTrigger);
        _anim.SetTrigger(drinkTrigger);
    }
}
