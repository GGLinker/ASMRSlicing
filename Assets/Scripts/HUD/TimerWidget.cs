using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TimerWidget : MonoBehaviour
{
    private TextMeshProUGUI textWidget;

    private void Awake()
    {
        textWidget = gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void UpdateContent(int timerValue)
    {
        textWidget.text = timerValue.ToString();
    }
}
