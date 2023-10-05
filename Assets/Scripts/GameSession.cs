using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
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

    private bool _bAllowedInput;
    private bool _bGameOver;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Singleton violation: " + gameObject.name);
            return;
        }
        Instance = this;

        knifeMovement.OnFullDescent += KnifeSliceMotionEnded;
        knifeMovement.OnRelease += KnifeReleaseMotionEnded;

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
        InputHandler.Instance.touchStateChanged += HandleInputEvent;
        
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
        _bGameOver = false;
        OnPreMatchCountdownRequest?.Invoke(this, preMatchTimerDurationInSeconds);
    }
    private void GameOver()
    {
        if (_bGameOver) return;
        
        _bGameOver = true;
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
    

    #region KnifeMovement

    private void MoveKnife(bool bCut)
    {
        if (bCut)
        {
            slicingObjectMovement.Move(false);
            knifeMovement.Cut();
        }
        else
        {
            _bAllowedInput = false;
            knifeMovement.ReleaseKnife();
        }
    }
    
    private void KnifeSliceMotionEnded(object sender, EventArgs args)
    {
        sliceExecutor.ThrowOffSlicedPart();
        MoveKnife(false);
    }
    private void KnifeReleaseMotionEnded(object sender, KnifeMovement.KnifeMovementState state)
    {
        if (sliceExecutor.bFullySliced)
        {
            GameOver();
            return;
        }
        if (state == KnifeMovement.KnifeMovementState.Idle)
        {
            slicingObjectMovement.Move(true);
        }

        if (!_bGameOver)
        {
            _bAllowedInput = true;
        }
    }

    #endregion
}
