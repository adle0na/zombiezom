using UnityEngine;

public class CureEffectController : MonoBehaviour
{
    [SerializeField] private Animator leftEffect;
    [SerializeField] private Animator rightEffect;

    public void PlayEffects()
    {
        leftEffect.Play("Cure_L");
        rightEffect.Play("Cure_R");
    }
}
