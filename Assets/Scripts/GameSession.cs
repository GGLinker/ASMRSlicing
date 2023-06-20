using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[RequireComponent(typeof(InputHandler))]
public class GameSession : MonoBehaviour
{
    [SerializeField] private SlicingObjectMovement slicingObjectMovement;
    [SerializeField] private KnifeMovement knifeMovement;
    [SerializeField] private SliceExecutor sliceExecutor;
    private InputHandler inputHandler;
    
    //UI timer block could subscribe on this event to catch left seconds
    public delegate void OnPreMatchTimerTick(int secondsLeft);
    public static event OnPreMatchTimerTick OnTimerTicks;
    
    //This event should be triggered on pause/resume movement of object to cut
    public delegate void OnObjectToCutMoveModeChanged(bool bMove);
    public static event OnObjectToCutMoveModeChanged OnInputModeChanged;
    
    
    [Tooltip("PreMatchTimerDurationInSeconds")]
    [SerializeField] private int preMatchTimerDurationInSeconds;

    private bool bAllowedInput;

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        inputHandler.touchStateChanged += (bBegan) => {OnInputModeChanged?.Invoke(bBegan);};

        sliceExecutor.OnSliceComplete += (slicedPart, remainPart) =>
        {
            slicingObjectMovement = remainPart.GetComponent<SlicingObjectMovement>();
        };
    }
    void Start()
    {
        StartCoroutine(GameLoop());
    }

    
    #region GAME LOOP
    IEnumerator GameLoop()
    {
        yield return StartCoroutine(WaitBeforeMatch());
        slicingObjectMovement.ManageMovement(true);
        bAllowedInput = true;
        OnInputModeChanged += InputModeChanged;
        slicingObjectMovement.OnMotionEnded += () =>
        {
            StartCoroutine(GameLoopSecondPart());
        };
    }
    IEnumerator GameLoopSecondPart()
    {
        OnInputModeChanged -= InputModeChanged;
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

    private void InputModeChanged(bool bCutBegan)
    {
        if (!bAllowedInput) return;
        if (bCutBegan)
        {
            slicingObjectMovement.ManageMovement(false);
            knifeMovement.OnMotionEnded += KnifeForwardMotionEnded;
            knifeMovement.SetupMovement(false);
            knifeMovement.ManageMovement(true);
        }
        else
        {
            knifeMovement.ManageMovement(false);
            bAllowedInput = false;
            knifeMovement.OnMotionEnded -= KnifeForwardMotionEnded;
            knifeMovement.OnMotionEnded += KnifeReverseMotionEnded;
            knifeMovement.SetupMovement(true);
            knifeMovement.ManageMovement(true);
        }
    }
    private void KnifeForwardMotionEnded()
    {
        knifeMovement.OnMotionEnded -= KnifeForwardMotionEnded;
        knifeMovement.bAllowedToSplitObject = true;
        //simulate "release touch" event
        OnInputModeChanged?.Invoke(false);
        
        var splitPartRigidbody = sliceExecutor.GetLastSlicedPart().GetComponent<Rigidbody>();
        if (splitPartRigidbody != null)
        {
            splitPartRigidbody.useGravity = true;
            splitPartRigidbody.isKinematic = false;
            splitPartRigidbody.AddForce(new Vector3(0, 0, -1) * 30);
            splitPartRigidbody.AddTorque(new Vector3(-1, 0, 0) * 30);
        }
    }
    private void KnifeReverseMotionEnded()
    {
        knifeMovement.OnMotionEnded -= KnifeReverseMotionEnded;
        if (knifeMovement.bAllowedToSplitObject)
        {
            slicingObjectMovement.ManageMovement(true);
        }
        bAllowedInput = true;
    }
}
