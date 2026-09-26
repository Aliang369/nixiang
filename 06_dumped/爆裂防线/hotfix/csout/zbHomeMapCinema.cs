using System;
using UnityEngine;

public class zbHomeMapCinema : MonoBehaviour
{
	private zbHomeWnd m_wndParent;

	public float curPanSpeed;

	private float minPanSpeed;

	private float maxPanSpeed;

	private float minOrthographicSize;

	private float maxOrthographicSize;

	private Vector2 panLimitMin;

	private Vector2 panLimitMax;

	public float MinOrthographicSize => minOrthographicSize;

	public float MaxOrthographicSize => maxOrthographicSize;

	public Vector2 PanLimitMin
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return panLimitMin;
		}
	}

	public Vector2 PanLimitMax
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return panLimitMax;
		}
	}

	public void Init(zbHomeWnd wndParent)
	{
		m_wndParent = wndParent;
		zbCameraHelper.Instance.mainCamera = Camera.main;
		maxOrthographicSize = Math.Max(maxOrthographicSize, 22f * ((float)Screen.height / (float)Screen.width) / 2.0555556f);
	}

	public void ApplyPan(Vector3 delta)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		float z = ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z;
		Vector3 val = ClampCameraPosition(((Component)zbCameraHelper.Instance.mainCamera).transform.position + delta);
		Vector3 position = default(Vector3);
		((Vector3)(ref position))._002Ector(val.x, val.y, z);
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = position;
		m_wndParent.m_LastCameraPosition = ((Component)Camera.main).transform.position;
		UpdateHud();
	}

	public float CalculatePanSpeed(float currentSize)
	{
		float num = Mathf.InverseLerp(minOrthographicSize, maxOrthographicSize, currentSize);
		return Mathf.Lerp(maxPanSpeed, minPanSpeed, num);
	}

	public Vector3 ClampCameraPosition(Vector3 targetPos)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		float orthographicSize = zbCameraHelper.Instance.mainCamera.orthographicSize;
		float num = orthographicSize * zbCameraHelper.Instance.mainCamera.aspect;
		float num2 = panLimitMin.x + num;
		float num3 = panLimitMax.x - num;
		float num4 = panLimitMin.y + orthographicSize;
		float num5 = panLimitMax.y - orthographicSize;
		float num6 = Mathf.Clamp(targetPos.x, num2, num3);
		float num7 = Mathf.Clamp(targetPos.y, num4, num5);
		return new Vector3(num6, num7, targetPos.z);
	}

	public void ApplyZoom(float delta)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zoomCenterDelta = zbUtil.GetZoomCenterDelta();
		float orthographicSize = Mathf.Clamp(zbCameraHelper.Instance.mainCamera.orthographicSize + delta, minOrthographicSize, maxOrthographicSize);
		zbCameraHelper.Instance.mainCamera.orthographicSize = orthographicSize;
		Vector3 zoomCenterDelta2 = zbUtil.GetZoomCenterDelta();
		Vector3 val = zoomCenterDelta - zoomCenterDelta2;
		float z = ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z;
		Vector3 targetPos = ((Component)zbCameraHelper.Instance.mainCamera).transform.position + val;
		targetPos = ClampCameraPosition(targetPos);
		targetPos.z = z;
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = targetPos;
		m_wndParent.m_LastCameraPosition = ((Component)Camera.main).transform.position;
		m_wndParent.m_LastCameraOrthographicSize = zbCameraHelper.Instance.mainCamera.orthographicSize;
		UpdateHud();
	}

	public void SetCameraOrthographicSize(float orthographicSize)
	{
		zbCameraHelper.Instance.mainCamera.orthographicSize = orthographicSize;
	}

	public void FocusToPosition(Vector3 worldPos)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		float z = ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(worldPos.x, worldPos.y, z);
		val = ClampCameraPosition(val);
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = val;
	}

	private void UpdateHud()
	{
		zbHomeMgr.Instance.roomMgr.UpdateHudWhenCameraMove();
		zbHomeMgr.Instance.roomMgr.Update();
		zbHomeMgr.Instance.floorMgr.UpdateHudWhenCameraMove();
	}

	public zbHomeMapCinema()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		curPanSpeed = 0.015f;
		minPanSpeed = 0.005f;
		maxPanSpeed = 0.015f;
		minOrthographicSize = 10f;
		maxOrthographicSize = 28f;
		panLimitMin = new Vector2(-20f, -50f);
		panLimitMax = new Vector2(24f, 10f);
		((MonoBehaviour)this)._002Ector();
	}
}
