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
    [SerializeField] private int preMatchTimerDurationInSeconds;
    public event EventHandler<int> OnPreMatchCountdownRequest;
    public event EventHandler OnGameOver;

    private InputHandler _inputHandler;

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

        //Update slicing object ref & binding to passing knife's position
        sliceExecutor.OnSliceComplete += (sender, sliceParts) =>
        {
            slicingObjectMovement = sliceParts.remainPart.GetComponent<SlicingObjectMovement>();
            slicingObjectMovement.OnMotionEnded += (_,_) => GameOver();
        };
        slicingObjectMovement.OnMotionEnded += (_,_) => GameOver();
    }
    private IEnumerator Start()
    {
        while(OnPreMatchCountdownRequest == null) yield return null;
        OnPreMatchCountdownRequest?.Invoke(this, preMatchTimerDurationInSeconds);
    }

    
    #region GAME LOOP
    public void GameFlow()
    {
        slicingObjectMovement.Move(false);
        knifeMovement.Reset();
        slicingObjectMovement.Move(true);
        _bAllowedInput = true;
    }
    
    public void Restart()
    {
        sliceExecutor.RespawnSlicedObject();
        knifeMovement.Reset();
        OnPreMatchCountdownRequest?.Invoke(this, preMatchTimerDurationInSeconds);
    }
    private void GameOver()
    {
        _bAllowedInput = false;
        knifeMovement.ReleaseKnife();
        Debug.Log("GAME OVER");
        OnGameOver?.Invoke(this, EventArgs.Empty);
    }

    #endregion
    

    private void HandleInputEvent(object sender, bool bCutBegan)
    {
        if (!_bAllowedInput) return;
        
        MoveKnife(bCutBegan);
    }

    private void MoveKnife(bool bCut)
    {
        knifeMovement.OnTargetAchieved -= KnifeForwardMotionEnded;
        knifeMovement.OnTargetAchieved -= KnifeReverseMotionEnded;
        if (bCut)
        {
            slicingObjectMovement.Move(false);
            knifeMovement.OnTargetAchieved += KnifeForwardMotionEnded;
            knifeMovement.Cut();
        }
        else
        {
            _bAllowedInput = false;
            knifeMovement.OnTargetAchieved += KnifeReverseMotionEnded;
            knifeMovement.ReleaseKnife();
        }
    }
    
    private void KnifeForwardMotionEnded(object sender, KnifeMovement.KnifeMovementState state)
    {
        Debug.Log("Cut movement ended");
        knifeMovement.OnTargetAchieved -= KnifeForwardMotionEnded;

        sliceExecutor.ThrowOffSlicedPart();
        MoveKnife(false);
    }
    private void KnifeReverseMotionEnded(object sender, KnifeMovement.KnifeMovementState state)
    {
        Debug.Log("Reverse movement ended");
        knifeMovement.OnTargetAchieved -= KnifeReverseMotionEnded;
        
        if (sliceExecutor.bFullySliced)
        {
            GameOver();
            return;
        }
        if (state == KnifeMovement.KnifeMovementState.Idle)
        {
            slicingObjectMovement.Move(true);
        }
        
        _bAllowedInput = true;
    }
}
