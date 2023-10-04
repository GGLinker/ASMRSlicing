using System.Collections;
using TMPro;
using UnityEngine;

public class TimerWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textWidget;

    private void Start()
    {
        GameSession.Instance.OnPreMatchCountdownRequest += (_, amount) =>
        {
            StartCoroutine(Countdown(amount));
        };
        textWidget.gameObject.SetActive(false);
    }

    private IEnumerator Countdown(int amount)
    {
        var secondsLeft = amount;
        textWidget.gameObject.SetActive(true);
        while (secondsLeft >= 0)
        {
            textWidget.text = (secondsLeft--).ToString();
            yield return new WaitForSecondsRealtime(1f);
        }
        textWidget.gameObject.SetActive(false);

        GameSession.Instance.GameFlow();
    }
}
