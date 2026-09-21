using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class AnimationCameraController : SerializedMonoBehaviour
{
	private enum CameraAnimEventType
	{
		EnterLocationEnd = 5,
		ExitLocationStart = 10,
		StartNewGame = 11
	}

	private static readonly int animKeyword_Reset = Animator.StringToHash("Reset");

	private static readonly int animKeyword_State = Animator.StringToHash("State");

	[SerializeField]
	public Animator animator;

	public VirtualCamera virtualCamera;

	public void SetAnimState(VirtualCamera.State state)
	{
		animator.SetInteger(animKeyword_State, (int)state);
		animator.SetTrigger(animKeyword_Reset);
	}

	private void _OnAnimEvent(CameraAnimEventType type)
	{
		switch (type)
		{
		case CameraAnimEventType.EnterLocationEnd:
			if (virtualCamera.runningGameState != null && virtualCamera.runningGameState.currentEncounter != null)
			{
				virtualCamera.runningGameState.actionManager.EnqueueAction(new NotifyWorldEventAction(virtualCamera.runningGameState.currentEncounter.triggerEvent));
			}
			break;
		case CameraAnimEventType.StartNewGame:
			OnStartNewGame();
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		case CameraAnimEventType.ExitLocationStart:
			break;
		}
	}

	private void OnStartNewGame()
	{
		NormalEventPanel.AnimConfig normalEventPanelAnimConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.normalEventPanelAnimConfig;
		Image image = virtualCamera.cameraEffectImagePool.Spawn();
		image.gameObject.SetActive(value: true);
		image.color = Color.black;
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(normalEventPanelAnimConfig.imageFadeInterval);
		sequence.Append(DOTweenModuleUI.DOFade(image, 0f, 2f));
		sequence.AppendCallback(delegate
		{
			virtualCamera.cameraEffectImagePool.Despawn(image);
		});
		sequence.Play();
	}
}
