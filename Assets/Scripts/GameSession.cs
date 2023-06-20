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

    private bool bAllowedInput;

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        inputHandler.touchStateChanged += (bBegan) => {OnObjectMoveModeChanged?.Invoke(bBegan);};
    }
    void Start()
    {
        StartCoroutine(GameLoop());
    }

    
    #region GAME LOOP
    IEnumerator GameLoop()
    {
        yield return StartCoroutine(WaitBeforeMatch());
        bAllowedInput = true;
        OnObjectMoveModeChanged += ObjectMoveModeChanged;
        OnObjectMovedToEnd += () =>
        {
            StartCoroutine(GameLoopSecondPart());
        };
    }
    IEnumerator GameLoopSecondPart()
    {
        OnObjectMoveModeChanged -= ObjectMoveModeChanged;
        bAllowedInput = false;
        Debug.Log("Obj moved to end");
        yield return new WaitForSecondsRealtime(1f);
        StartCoroutine(GameLoop());
    }
    #endregion


    IEnumerator WaitBeforeMatch()
    {
        var secondsLeft = preMatchTimerDurationInSeconds;
        while (secondsLeft >= 0)
        {
            OnTimerTicks?.Invoke(secondsLeft);
            secondsLeft--;
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    //Method calls when knife has already been picked up
    private void ObjectMoveModeChanged(bool bCutBegan)
    {
        if (!bAllowedInput) return;
        if (bCutBegan)
        {
            slicingObject.ManageMovement(false);
            knifeMovement.OnMotionEnded += KnifeForwardMotionEnded;
            knifeMovement.SetupMovement(false);
            knifeMovement.ManageMovement(true);
        }
        else
        {
            knifeMovement.ManageMovement(false);
            bAllowedInput = false;
            knifeMovement.OnMotionEnded += KnifeReverseMotionEnded;
            knifeMovement.SetupMovement(true);
            knifeMovement.ManageMovement(true);
        }
    }
    private void KnifeForwardMotionEnded()
    {
        knifeMovement.bAllowedToSplitObject = true;
        //simulate "release touch" event
        OnObjectMoveModeChanged?.Invoke(false);
    }
    private void KnifeReverseMotionEnded()
    {
        knifeMovement.OnMotionEnded -= KnifeReverseMotionEnded;
        bAllowedInput = true;
    }
}
