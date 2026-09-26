using UnityEngine;

public class zbSeasonMapCinema
{
	private zbSeasonWnd m_wndParent;

	public float curPanSpeed;

	private float minPanSpeed;

	private float maxPanSpeed;

	private Vector2 panLimitMin;

	private Vector2 panLimitMax;

	private const int MapSize = 100;

	private const int AreaSize = 10;

	private Vector2Int m_lastViewCenter;

	private bool m_hasViewCenter;

	public void Init(zbSeasonWnd wndParent)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		zbCameraHelper.Instance.mainCamera = Camera.main;
		m_lastViewCenter = new Vector2Int(-1, -1);
		m_hasViewCenter = false;
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
		UpdateSeasonView();
	}

	public float CalculatePanSpeed(float currentSize)
	{
		float num = Mathf.InverseLerp(zbSeasonMapDefine.minOrthographicSize, zbSeasonMapDefine.maxOrthographicSize, currentSize);
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

	private Vector3 GetZoomCenterDelta()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.zero;
	}

	public void ApplyZoom(float delta)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
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
		Vector3 zoomCenterDelta = GetZoomCenterDelta();
		float orthographicSize = Mathf.Clamp(zbCameraHelper.Instance.mainCamera.orthographicSize + delta, zbSeasonMapDefine.minOrthographicSize, zbSeasonMapDefine.maxOrthographicSize);
		zbCameraHelper.Instance.mainCamera.orthographicSize = orthographicSize;
		Vector3 zoomCenterDelta2 = GetZoomCenterDelta();
		Vector3 val = zoomCenterDelta - zoomCenterDelta2;
		float z = ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z;
		Vector3 targetPos = ((Component)zbCameraHelper.Instance.mainCamera).transform.position + val;
		targetPos = ClampCameraPosition(targetPos);
		targetPos.z = z;
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = targetPos;
		m_wndParent.m_LastCameraPosition = ((Component)Camera.main).transform.position;
		m_wndParent.m_LastCameraOrthographicSize = zbCameraHelper.Instance.mainCamera.orthographicSize;
		UpdateHud();
		UpdateSeasonView();
	}

	public void SetCameraOrthographicSize(float orthographicSize)
	{
		orthographicSize = Mathf.Clamp(orthographicSize, zbSeasonMapDefine.minOrthographicSize, zbSeasonMapDefine.maxOrthographicSize);
		zbCameraHelper.Instance.mainCamera.orthographicSize = orthographicSize;
		UpdateHud();
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
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		float z = ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(worldPos.x, worldPos.y, z);
		val = ClampCameraPosition(val);
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = val;
		m_wndParent.m_LastCameraPosition = ((Component)zbCameraHelper.Instance.mainCamera).transform.position;
		UpdateHud();
		UpdateSeasonView();
	}

	private Vector2Int GetCameraCenterArea()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)zbCameraHelper.Instance.mainCamera).transform.position;
		float x = panLimitMin.x;
		float y = panLimitMax.y;
		float num = panLimitMax.x - panLimitMin.x;
		float num2 = panLimitMax.y - panLimitMin.y;
		float num3 = num / 10f;
		float num4 = num2 / 10f;
		int num5 = Mathf.FloorToInt((position.x - x) / num3);
		int num6 = Mathf.FloorToInt((y - position.y) / num4);
		int num7 = 9;
		int num8 = Mathf.Clamp(num5, 0, num7);
		num6 = Mathf.Clamp(num6, 0, num7);
		return new Vector2Int(num8, num6);
	}

	private void UpdateSeasonView()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int cameraCenterArea = GetCameraCenterArea();
		if (!m_hasViewCenter || !(cameraCenterArea == m_lastViewCenter))
		{
			m_lastViewCenter = cameraCenterArea;
			m_hasViewCenter = true;
			zbWsMsgHandler.Instance.Season_View_Req(((Vector2Int)(ref cameraCenterArea)).x, ((Vector2Int)(ref cameraCenterArea)).y);
		}
	}

	private void UpdateHud()
	{
		m_wndParent.GetSeasonMap()?.GetSeaSonMiniMap()?.UpdateCameraRect();
		zbSeasonHudMgr.Instance.UpdateWorldToPanelCache();
		zbSeasonHudMgr.Instance.UpdateHudPos();
	}

	public void ResetSeasonView()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		m_lastViewCenter = new Vector2Int(-1, -1);
		m_hasViewCenter = false;
	}

	public zbSeasonMapCinema()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		curPanSpeed = 0.015f;
		minPanSpeed = 0.005f;
		maxPanSpeed = 0.015f;
		panLimitMin = new Vector2(-54f, -54f);
		panLimitMax = new Vector2(54f, 54f);
		m_lastViewCenter = new Vector2Int(-1, -1);
		base._002Ector();
	}
}
