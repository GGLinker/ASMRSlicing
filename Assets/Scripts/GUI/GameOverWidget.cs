using System.Collections;
using UnityEngine;

public class GameOverWidget : MonoBehaviour
{
    [SerializeField] private Animator titleWidgetAnimator;
    
    private const string APPEAR_ANIM = "Appear";
    private const string DISAPPEAR_ANIM = "Disappear";

    private void Start()
    {
        titleWidgetAnimator.gameObject.SetActive(false);
        GameSession.Instance.OnGameOver += (_,_) =>
        {
            StartCoroutine(AnimationCycle());
        };
    }

    private IEnumerator AnimationCycle()
    {
        titleWidgetAnimator.gameObject.SetActive(true);
        titleWidgetAnimator.SetTrigger(APPEAR_ANIM);
        yield return new WaitForSecondsRealtime(titleWidgetAnimator.GetCurrentAnimatorClipInfo(0).Length);
        yield return new WaitForSeconds(1f);
        titleWidgetAnimator.SetTrigger(DISAPPEAR_ANIM);
        yield return new WaitForSecondsRealtime(titleWidgetAnimator.GetCurrentAnimatorClipInfo(0).Length);
        titleWidgetAnimator.gameObject.SetActive(false);
        
        GameSession.Instance.Restart();
    }
}
