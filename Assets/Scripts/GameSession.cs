using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(InputHandler))]
public class GameSession : MonoBehaviour
{
    [SerializeField] private SlicingObjectMovement slicingObject;
    [SerializeField] private KnifeMovement knifeMovement;
    private InputHandler inputHandler;
    
    //UI timer block could subscribe on this event to catch left seconds
    public delegate void OnPreMatchTimerTick(int secondsLeft);
    public static event OnPreMatchTimerTick OnTimerTicks;
    
    //This event should be triggered on pause/resume movement of object to cut
    public delegate void OnObjectToCutMoveModeChanged(bool bMove);
    public static event OnObjectToCutMoveModeChanged OnObjectMoveModeChanged;

    private delegate void OnObjectToCutMovedToEnd();
    private static event OnObjectToCutMovedToEnd OnObjectMovedToEnd;
    
    [Tooltip("PreMatchTimerDurationInSeconds")]
    [SerializeField] private int preMatchTimerDurationInSeconds;


    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        inputHandler.touchStateChanged += (began => { Debug.Log(began); });
    }
    void Start()
    {
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        for (;;)
        {
            yield return StartCoroutine(WaitBeforeMatch());
            OnObjectMoveModeChanged += ObjectMoveModeChanged;
            yield return OnObjectMovedToEnd;
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    IEnumerator WaitBeforeMatch()
    {
        var secondsLeft = preMatchTimerDurationInSeconds;
        while (secondsLeft >= 0)
        {
            OnTimerTicks?.Invoke(secondsLeft--);
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    //Method calls when knife has already been picked up
    private void ObjectMoveModeChanged(bool bMove)
    {
        slicingObject.ManageMovement(bMove);
    }
}
