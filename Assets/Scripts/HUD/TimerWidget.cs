using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TimerWidget : MonoBehaviour
{
    private TextMeshProUGUI _textWidget;

    private void Awake()
    {
        _textWidget = gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void UpdateContent(int timerValue)
    {
        _textWidget.text = timerValue.ToString();
    }
}
