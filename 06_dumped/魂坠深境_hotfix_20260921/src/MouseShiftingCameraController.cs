using System;
using UnityEngine;

public class MouseShiftingCameraController : MonoBehaviour
{
	[Serializable]
	public class MouseShiftingConfig
	{
		public Vector2 maxRotationOffset = new Vector2(2.5f, 0.2f);

		public AnimationCurve xRotationCurve;

		public AnimationCurve yRotationCurve;

		public Vector2 maxPositionOffset = new Vector2(1f, 1f);

		public AnimationCurve xPositionCurve;

		public AnimationCurve yPositionCurve;
	}

	public MouseShiftingConfig roomShiftConfig;

	public MouseShiftingConfig mapShiftConfig;

	public MouseShiftingConfig fortressShiftConfig;

	[SerializeField]
	private Transform shiftingPivot;

	[SerializeField]
	public bool enableMouseShifting;

	[SerializeField]
	public VirtualCamera virtualCamera;

	public MouseShiftingConfig CurrentShiftConfig
	{
		get
		{
			if (virtualCamera.cameraState == VirtualCamera.State.Event || virtualCamera.cameraState == VirtualCamera.State.Battle)
			{
				return roomShiftConfig;
			}
			if (virtualCamera.cameraState == VirtualCamera.State.Map)
			{
				return mapShiftConfig;
			}
			if (virtualCamera.cameraState == VirtualCamera.State.Fortress)
			{
				return fortressShiftConfig;
			}
			return roomShiftConfig;
		}
	}

	private void Update()
	{
		if (PlatformHelper.IsMobile())
		{
			return;
		}
		ApplicationStateInfo currentApplicationState = Bootstrap.instance.CurrentApplicationState;
		if (currentApplicationState != null && currentApplicationState.inited && currentApplicationState != null && currentApplicationState.renderingManager?.EnableCameraShake == true)
		{
			if (!enableMouseShifting)
			{
				shiftingPivot.transform.localRotation = Quaternion.identity;
				return;
			}
			MouseShiftingConfig currentShiftConfig = CurrentShiftConfig;
			Vector2 vector = (Input.mousePosition - new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f)) / new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
			float y = currentShiftConfig.maxRotationOffset.x * currentShiftConfig.xRotationCurve.Evaluate(vector.x);
			float num = currentShiftConfig.maxRotationOffset.y * currentShiftConfig.yRotationCurve.Evaluate(vector.y);
			shiftingPivot.transform.localRotation = Quaternion.Euler(0f - num, y, 0f);
			float x = currentShiftConfig.maxPositionOffset.x * currentShiftConfig.xPositionCurve.Evaluate(vector.x);
			float y2 = currentShiftConfig.maxPositionOffset.y * currentShiftConfig.yPositionCurve.Evaluate(vector.y);
			shiftingPivot.transform.localPosition = new Vector3(x, y2, 0f);
		}
	}
}
