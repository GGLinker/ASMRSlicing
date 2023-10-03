using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(InputHandler))]
public class GameSession : MonoBehaviour
{
    #region Singleton

    public static GameSession Instance { get; private set; }

    #endregion
    
    [SerializeField] private SlicingObjectMovement slicingObjectMovement;
    [SerializeField] private KnifeMovement knifeMovement;
    [SerializeField] private SliceExecutor sliceExecutor;
    [SerializeField] private TimerWidget timerWidget;
    [Header("Game Over widget operating zone")]
    [SerializeField] private Animator gameOverTitleWidgetAnimator;
    [SerializeField] private AnimationClip appearAnimation;
    [SerializeField] private AnimationClip fadeAnimation;

    public event EventHandler OnGameOver;

    private InputHandler _inputHandler;
    
    [Tooltip("PreMatchTimerDurationInSeconds")]
    [SerializeField] private int preMatchTimerDurationInSeconds;

    private bool _bAllowedInput;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Singleton violation: " + gameObject.name);
            return;
        }
        Instance = this;
        
        _inputHandler = GetComponent<InputHandler>();
        _inputHandler.touchStateChanged += HandleInputEvent;

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
        knifeMovement.bAllowedToSplitObject = true;
        slicingObjectMovement.ManageMovement(true);
        _bAllowedInput = true;
        SubscribeOnSlicingObjectMotionEnd();
    }
    private void GameOver()
    {
        _bAllowedInput = false;
        Debug.Log("GAME OVER");
        OnGameOver?.Invoke(this, EventArgs.Empty);
    }

    public void Restart()
    {
        sliceExecutor.RespawnSlicedObject();
        StartCoroutine(GameLoop());
    }
    
    IEnumerator WaitBeforeMatch()
    {
        var secondsLeft = preMatchTimerDurationInSeconds;
        timerWidget.gameObject.SetActive(true);
        while (secondsLeft >= 0)
        {
            timerWidget.UpdateContent(secondsLeft--);
            yield return new WaitForSecondsRealtime(1f);
        }
        timerWidget.gameObject.SetActive(false);
    }
    #endregion
    

    private void SubscribeOnSlicingObjectMotionEnd()
    {
        slicingObjectMovement.OnMotionEnded += (sender, args) => GameOver();
    }

    private void HandleInputEvent(object sender, bool bCutBegan)
    {
        if (!_bAllowedInput) return;
        
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
            _bAllowedInput = false;
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
        HandleInputEvent(this, false);
    }
    private void KnifeReverseMotionEnded()
    {
        Debug.Log("Reverse movement ended");
        knifeMovement.OnTargetAchieved -= KnifeReverseMotionEnded;
        if (knifeMovement.bAllowedToSplitObject)
        {
            slicingObjectMovement.ManageMovement(true);
        }
        if (sliceExecutor.bFullySliced)
        {
            GameOver();
            return;
        }
        _bAllowedInput = true;
    }
}
