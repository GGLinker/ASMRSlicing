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
        sliceExecutor.OnSliceComplete += (slicedPart, remainPart) =>
        {
            slicingObjectMovement = remainPart.GetComponent<SlicingObjectMovement>();
            slicingObjectMovement.OnMotionEnded += (sender, args) => GameOver();
        };
    }
    private IEnumerator Start()
    {
        while(OnPreMatchCountdownRequest == null) yield return null;
        OnPreMatchCountdownRequest?.Invoke(this, preMatchTimerDurationInSeconds);
    }

    
    #region GAME LOOP
    public void GameFlow()
    {
        knifeMovement.bAllowedToSplitObject = true;
        slicingObjectMovement.Move(true);
        _bAllowedInput = true;
        slicingObjectMovement.OnMotionEnded += (sender, args) => GameOver();
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
        OnPreMatchCountdownRequest?.Invoke(this, preMatchTimerDurationInSeconds);
    }
    #endregion
    

    private void HandleInputEvent(object sender, bool bCutBegan)
    {
        if (!_bAllowedInput) return;
        
        knifeMovement.OnTargetAchieved -= KnifeForwardMotionEnded;
        knifeMovement.OnTargetAchieved -= KnifeReverseMotionEnded;
        if (bCutBegan)
        {
            Debug.Log("Forward movement started");
            slicingObjectMovement.Move(false);
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
    private void KnifeForwardMotionEnded(object sender, EventArgs args)
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
    private void KnifeReverseMotionEnded(object sender, EventArgs args)
    {
        Debug.Log("Reverse movement ended");
        knifeMovement.OnTargetAchieved -= KnifeReverseMotionEnded;
        if (knifeMovement.bAllowedToSplitObject)
        {
            slicingObjectMovement.Move(true);
        }
        if (sliceExecutor.bFullySliced)
        {
            GameOver();
            return;
        }
        _bAllowedInput = true;
    }
}
