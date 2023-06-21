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


    [Tooltip("PreMatchTimerDurationInSeconds")]
    [SerializeField] private int preMatchTimerDurationInSeconds;

    private bool bAllowedInput;

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        inputHandler.touchStateChanged += InputModeChanged;

        sliceExecutor.OnSliceComplete += (slicedPart, remainPart) =>
        {
            slicingObjectMovement = remainPart.GetComponent<SlicingObjectMovement>();
            SubscribeOnSlicingObjectMotionEnd();
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
        SubscribeOnSlicingObjectMotionEnd();
    }
    IEnumerator GameLoopSecondPart()
    {
        bAllowedInput = false;
        sliceExecutor.RespawnSlicedObject();
        Debug.Log("GAME OVER");
        yield return null;
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

    private void SubscribeOnSlicingObjectMotionEnd()
    {
        slicingObjectMovement.OnMotionEnded += () =>
        {
            StartCoroutine(GameLoopSecondPart());
        };
    }

    private void InputModeChanged(bool bCutBegan)
    {
        if (!bAllowedInput) return;
        
        knifeMovement.OnTargetAchieved -= KnifeForwardMotionEnded;
        knifeMovement.OnTargetAchieved -= KnifeReverseMotionEnded;
        if (bCutBegan)
        {
            Debug.Log("Forward movement started");
            slicingObjectMovement.ManageMovement(false);
            knifeMovement.OnTargetAchieved += KnifeForwardMotionEnded;
            knifeMovement.SetupMovement(false);
            knifeMovement.ManageMovement(true);
        }
        else
        {
            knifeMovement.ManageMovement(false);
            Debug.Log("Reverse movement started");
            bAllowedInput = false;
            if (sliceExecutor.bFullySliced)
            {
                Debug.LogError("Yep");
                StartCoroutine(GameLoopSecondPart());
            }
            knifeMovement.OnTargetAchieved += KnifeReverseMotionEnded;
            knifeMovement.SetupMovement(true);
            knifeMovement.ManageMovement(true);
        }
    }
    private void KnifeForwardMotionEnded()
    {
        Debug.Log("Forward movement ended");
        
        knifeMovement.OnTargetAchieved -= KnifeForwardMotionEnded;
        knifeMovement.bAllowedToSplitObject = true;
        
        var splitPartRigidbody = sliceExecutor.GetLastSlicedPart()?.GetComponent<Rigidbody>();
        if (splitPartRigidbody != null)
        {
            splitPartRigidbody.useGravity = true;
            splitPartRigidbody.isKinematic = false;
            splitPartRigidbody.AddForce(new Vector3(0, 0, -1) * 30);
            splitPartRigidbody.AddTorque(new Vector3(-1, 0, 0) * 30);
        }
        
        //simulate "release touch" event
        InputModeChanged(false);
    }
    private void KnifeReverseMotionEnded()
    {
        Debug.Log("Reverse movement ended");
        knifeMovement.OnTargetAchieved -= KnifeReverseMotionEnded;
        if (knifeMovement.bAllowedToSplitObject)
        {
            slicingObjectMovement.ManageMovement(true);
        }
        bAllowedInput = true;
    }
}
