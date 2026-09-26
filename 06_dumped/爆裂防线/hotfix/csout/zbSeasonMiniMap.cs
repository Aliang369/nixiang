using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonMiniMap
{
	private zbSeasonWnd m_wndParent;

	private VisualElement m_root;

	private Dictionary<int, VisualElement> m_areas = new Dictionary<int, VisualElement>();

	private VisualElement m_cameraRect;

	private const int MINI_MAP_SIZE = 230;

	private const int CELL_BORDER_WIDTH = 1;

	private const int RECT_BORDER_WIDTH = 5;

	private const float MAP_PADDING = 1f;

	private float m_areaSize;

	public zbSeasonMiniMap(zbSeasonWnd wndParent)
	{
		m_wndParent = wndParent;
		m_areaSize = 228f / (float)zbSeasonMapDefine.gridCount;
	}

	public void OnEnable()
	{
		m_root = UQueryExtensions.Q<VisualElement>(m_wndParent.GetRootElement(), "minimap", (string)null);
	}

	public void InitAreas(List<ServerAreaData> datas)
	{
		m_root.Clear();
		m_areas.Clear();
		foreach (ServerAreaData data in datas)
		{
			CreateAreas(data);
		}
		CreateCameraRect();
		UpdateCameraRect();
	}

	private void CreateAreas(ServerAreaData data)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = new VisualElement();
		val.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		val.style.left = StyleLength.op_Implicit(1f + (float)data.logicX * m_areaSize);
		val.style.top = StyleLength.op_Implicit(1f + (float)data.logicY * m_areaSize);
		float num = (data.isCenter ? (m_areaSize * 2f) : m_areaSize);
		val.style.width = StyleLength.op_Implicit(num);
		val.style.height = StyleLength.op_Implicit(num);
		val.style.backgroundImage = (StyleBackground)(data.isCenter ? new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/icon_miniMapMiddle.png")) : StyleBackground.op_Implicit((Texture2D)null));
		val.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(new Length(m_areaSize * 1.5f), new Length(m_areaSize * 1.5f)));
		val.style.borderRightWidth = StyleFloat.op_Implicit(1f);
		val.style.borderTopWidth = StyleFloat.op_Implicit(1f);
		Color val2 = default(Color);
		ColorUtility.TryParseHtmlString("#565377", ref val2);
		val.style.borderRightColor = StyleColor.op_Implicit(val2);
		val.style.borderTopColor = StyleColor.op_Implicit(val2);
		val.style.backgroundColor = StyleColor.op_Implicit(zbSeasonMapDefine.GetForceColor(data.force));
		int areaId = data.areaId;
		m_areas.Add(areaId, val);
		m_root.Add(val);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>(val, (Action)delegate
		{
			OnCellClick(data.areaId);
		}, "", (TrickleDown)0);
	}

	private void OnCellClick(int cellId)
	{
		m_wndParent.GetSeasonMap()?.TrackArea(cellId);
	}

	private void CreateCameraRect()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		m_cameraRect = new VisualElement();
		m_cameraRect.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		m_cameraRect.style.borderLeftWidth = StyleFloat.op_Implicit(5f);
		m_cameraRect.style.borderRightWidth = StyleFloat.op_Implicit(5f);
		m_cameraRect.style.borderTopWidth = StyleFloat.op_Implicit(5f);
		m_cameraRect.style.borderBottomWidth = StyleFloat.op_Implicit(5f);
		m_cameraRect.style.borderLeftColor = StyleColor.op_Implicit(Color.green);
		m_cameraRect.style.borderRightColor = StyleColor.op_Implicit(Color.green);
		m_cameraRect.style.borderTopColor = StyleColor.op_Implicit(Color.green);
		m_cameraRect.style.borderBottomColor = StyleColor.op_Implicit(Color.green);
		m_cameraRect.pickingMode = (PickingMode)1;
		m_root.Add(m_cameraRect);
	}

	public void UpdateCameraRect()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (m_cameraRect != null)
		{
			Camera mainCamera = zbCameraHelper.Instance.mainCamera;
			float num = zbSeasonMapDefine.cellSize * (float)zbSeasonMapDefine.gridCount;
			float num2 = 228f / num;
			Vector3 position = ((Component)mainCamera).transform.position;
			float num3 = mainCamera.orthographicSize * 2f;
			float num4 = num3 * mainCamera.aspect * num2;
			float num5 = num3 * num2;
			if (num5 > num4)
			{
				num5 = num4;
			}
			float num6 = 115f + position.x * num2;
			float num7 = 115f - position.y * num2;
			m_cameraRect.style.width = StyleLength.op_Implicit(num4);
			m_cameraRect.style.height = StyleLength.op_Implicit(num5);
			m_cameraRect.style.left = StyleLength.op_Implicit(num6 - num4 * 0.5f);
			m_cameraRect.style.top = StyleLength.op_Implicit(num7 - num5 * 0.5f);
		}
	}
}
