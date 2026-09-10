using System;
using Common.UI;
using DG.Tweening;
using FirstGearGames.SmoothCameraShaker;
using UnityEngine;
using UnityEngine.UI;

public class VirtualCamera : MonoBehaviour
{
	public enum State
	{
		None,
		EnterBattle,
		EnterEvent,
		Map,
		Battle,
		Event,
		StartNewGame,
		Fortress,
		GachaRoom
	}

	[NonSerialized]
	public FinalRenderCamera bindedCamera;

	public Transform pawnRoot;

	public Transform gyroscopeRoot;

	public Transform MoveEffectRoot;

	public Transform shakeEffectRoot;

	public Transform cutSceneCameraPivot;

	public Transform finalCameraPivot;

	public CameraShaker cameraShaker;

	public SpriteRenderer cameraFrontFilter;

	public GyroscopeCameraController gyroscopeCameraController;

	public MouseShiftingCameraController mouseShiftingCameraController;

	public AnimationCameraController animationCameraController;

	public LookPivotCameraController lookPivotCameraController;

	public Animation moveEffectAnimation;

	public Image cameraEffectImagePrefab;

	public Transform cameraEffectParent;

	[NonSerialized]
	public ApplicationStateInfo applicationState;

	public State cameraState;

	[NonSerialized]
	public PrefabPool<Image> cameraEffectImagePool;

	[NonSerialized]
	public Sequence currentSequence;

	[NonSerialized]
	public Sequence cameraPosSequence;

	public GameStateInfo runningGameState => applicationState.runningGameState;

	public bool isTweening
	{
		get
		{
			if (cameraPosSequence != null)
			{
				return cameraPosSequence.IsPlaying();
			}
			return false;
		}
	}

	public void BindVirtualCamera(GameStateInfo gs)
	{
		bindedCamera = Bootstrap.instance.finalCamera;
		cameraShaker.SetCam(bindedCamera.environmentCenterCamera);
		applicationState = gs.applicationState;
		applicationState.currentVirtualCamera = this;
		gs.UIManager.ui3DCameraCanvas.worldCamera = bindedCamera.finalCamera;
		gs.UIManager.ui3DWorldCanvas.worldCamera = bindedCamera.finalCamera;
		gs.UIManager.ui3DBaseCanvas.worldCamera = bindedCamera.finalCamera;
	}

	public void BindVirtualCamera(FortressStateInfo fs)
	{
		bindedCamera = Bootstrap.instance.finalCamera;
		cameraShaker.SetCam(bindedCamera.environmentCenterCamera);
		applicationState = fs.applicationState;
		applicationState.currentVirtualCamera = this;
	}

	private void Awake()
	{
		if (cameraEffectImagePrefab != null)
		{
			cameraEffectImagePool = new PrefabPool<Image>(cameraEffectImagePrefab, cameraEffectParent.transform);
		}
	}

	private void Update()
	{
		if (!(bindedCamera == null))
		{
			bindedCamera.transform.position = finalCameraPivot.transform.position;
			bindedCamera.transform.rotation = finalCameraPivot.transform.rotation;
		}
	}

	public void Reset()
	{
		MoveEffectRoot.transform.localPosition = Vector3.zero;
		MoveEffectRoot.transform.localRotation = Quaternion.identity;
	}

	public void SetState(State state)
	{
		cameraState = state;
		animationCameraController.SetAnimState(state);
		lookPivotCameraController.ResetLookRotation();
	}

	public void DoToDeathRattle()
	{
		cameraPosSequence?.Complete(withCallbacks: true);
		cameraPosSequence = DOTween.Sequence();
		cameraPosSequence.AppendCallback(delegate
		{
			cameraFrontFilter.gameObject.SetActive(value: true);
		});
		cameraPosSequence.Append(DOTweenModuleSprite.DOColor(cameraFrontFilter, ScriptableObjectSingleton<VFXDataPreset>.instance.deathRattleFilterColor, ScriptableObjectSingleton<VFXDataPreset>.instance.deathRattleFadeTime));
		cameraPosSequence.Play();
	}

	public void DoOutDeathRattle()
	{
		cameraPosSequence?.Complete(withCallbacks: true);
		cameraPosSequence = DOTween.Sequence();
		cameraPosSequence.Append(DOTweenModuleSprite.DOColor(cameraFrontFilter, Color.clear, ScriptableObjectSingleton<VFXDataPreset>.instance.deathRattleFadeTime));
		cameraPosSequence.AppendCallback(delegate
		{
			cameraFrontFilter.gameObject.SetActive(value: false);
		});
		cameraPosSequence.Play();
	}

	public void DoDeathDrop()
	{
		cameraPosSequence?.Complete(withCallbacks: true);
		cameraPosSequence = DOTween.Sequence();
		cameraPosSequence.Append(MoveEffectRoot.DOLocalMove(ScriptableObjectSingleton<VFXDataPreset>.instance.deathDropPosition1, ScriptableObjectSingleton<VFXDataPreset>.instance.deathDropTime1).SetEase(Ease.OutQuart));
		cameraPosSequence.Join(MoveEffectRoot.DOLocalRotate(ScriptableObjectSingleton<VFXDataPreset>.instance.deathDropRotation1, ScriptableObjectSingleton<VFXDataPreset>.instance.deathDropTime1).SetEase(Ease.OutQuart));
		cameraPosSequence.Append(MoveEffectRoot.DOLocalMove(ScriptableObjectSingleton<VFXDataPreset>.instance.deathDropPosition2, ScriptableObjectSingleton<VFXDataPreset>.instance.deathDropTime2).SetEase(Ease.OutBounce));
		cameraPosSequence.Join(MoveEffectRoot.DOLocalRotate(ScriptableObjectSingleton<VFXDataPreset>.instance.deathDropRotation2, ScriptableObjectSingleton<VFXDataPreset>.instance.deathDropTime2).SetEase(Ease.OutBounce));
		cameraPosSequence.Append(DOTweenModuleSprite.DOColor(cameraFrontFilter, Color.black, ScriptableObjectSingleton<VFXDataPreset>.instance.deathDropTime3));
		cameraPosSequence.Play();
	}
}
