using System;
using DG.Tweening;
using UnityEngine;

public class GyroscopeCameraController : MonoBehaviour
{
	public float gyroFactor = 10f;

	public Vector2 maxOffset = new Vector2(2.5f, 0.2f);

	public float resetPosNeedTime = 1.5f;

	[SerializeField]
	private Transform GyroscopePivot;

	[SerializeField]
	private AnimationCurve xCurve;

	[SerializeField]
	private AnimationCurve yCurve;

	[NonSerialized]
	public Vector2 offset;

	[NonSerialized]
	private float resetPosTimer;

	[SerializeField]
	public bool EnableGyroscope
	{
		get
		{
			return Input.gyro.enabled;
		}
		set
		{
			Input.gyro.enabled = value;
		}
	}

	private bool toResetPosFlag => resetPosTimer > resetPosNeedTime;

	private void Awake()
	{
		EnableGyroscope = true;
	}

	private void Update()
	{
		UpdateGyroscopeCameraEffect();
	}

	private void UpdateGyroscopeCameraEffect()
	{
		if (!EnableGyroscope)
		{
			return;
		}
		ApplicationStateInfo currentApplicationState = Bootstrap.instance.CurrentApplicationState;
		if (currentApplicationState == null || !currentApplicationState.inited || currentApplicationState == null || currentApplicationState.renderingManager?.EnableCameraShake != true)
		{
			return;
		}
		Vector3 rotationRateUnbiased = Input.gyro.rotationRateUnbiased;
		if (Input.GetKey(KeyCode.RightArrow))
		{
			rotationRateUnbiased.y++;
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			rotationRateUnbiased.y--;
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			rotationRateUnbiased.x++;
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			rotationRateUnbiased.x--;
		}
		if (rotationRateUnbiased.x == 0f && rotationRateUnbiased.y == 0f)
		{
			resetPosTimer += Time.deltaTime;
		}
		else
		{
			resetPosTimer = 0f;
		}
		float num = offset.x + rotationRateUnbiased.y * gyroFactor * xCurve.Evaluate(Mathf.Abs(offset.x / maxOffset.x)) * Time.deltaTime;
		float num2 = offset.y - rotationRateUnbiased.x * gyroFactor * yCurve.Evaluate(Mathf.Abs(offset.y / maxOffset.y)) * Time.deltaTime;
		if (Mathf.Abs(num) > maxOffset.x)
		{
			num = num / Mathf.Abs(num) * maxOffset.x;
		}
		if (Mathf.Abs(num2) > maxOffset.y)
		{
			num2 = num2 / Mathf.Abs(num2) * maxOffset.y;
		}
		offset.x = num;
		offset.y = num2;
		Vector3 zero = Vector3.zero;
		zero = ((!(currentApplicationState.runningGameState?.currentEncounter?.room != null)) ? (1.5f * Vector3.up) : currentApplicationState.runningGameState.currentEncounter.room.cameraRotatePivotPos);
		Vector3 position = GyroscopePivot.transform.parent.position;
		zero.y = position.y;
		Quaternion quaternion = Quaternion.AngleAxis(offset.y, Vector3.right) * Quaternion.AngleAxis(offset.x, Vector3.up);
		Vector3 vector = position - zero;
		GyroscopePivot.transform.position = zero + quaternion * vector;
		GyroscopePivot.transform.localRotation = Quaternion.LookRotation(-(quaternion * vector), Vector3.up);
		if (toResetPosFlag)
		{
			resetPosTimer = 0f;
			DOTween.To(() => offset, delegate(Vector2 v)
			{
				offset = v;
			}, Vector2.zero, 0.6f).SetEase(Ease.InQuad).Play();
		}
	}
}
