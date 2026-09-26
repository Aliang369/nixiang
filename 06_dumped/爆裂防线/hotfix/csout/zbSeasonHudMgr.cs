using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonHudMgr
{
	private static zbSeasonHudMgr m_instance;

	protected Vector2 m_worldToPanelScale;

	protected Vector2 m_worldToPanelOffset;

	private zbSeasonWnd m_wndParent;

	private VisualElement m_hudRoot;

	private List<zbSeasonHudBase> m_huds = new List<zbSeasonHudBase>();

	public static zbSeasonHudMgr Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new zbSeasonHudMgr();
			}
			return m_instance;
		}
	}

	public Vector2 WorldToPanelScale
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_worldToPanelScale;
		}
	}

	public Vector2 WorldToPanelOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_worldToPanelOffset;
		}
	}

	public void OnEnable(zbSeasonWnd wndParent)
	{
		m_wndParent = wndParent;
		m_hudRoot = UQueryExtensions.Q<VisualElement>(m_wndParent.GetRootElement(), "hud", (string)null);
		UpdateWorldToPanelCache();
	}

	public void AddHud(zbSeasonHudBase hud)
	{
		if (hud != null)
		{
			m_huds.Add(hud);
			if (m_hudRoot != null && hud.GetRoot() != null)
			{
				m_hudRoot.Add(hud.GetRoot());
			}
			hud.Show();
		}
	}

	public void RemoveHud(zbSeasonHudBase hud)
	{
		if (hud != null)
		{
			m_huds.Remove(hud);
			hud.Destroy();
		}
	}

	public void Clear()
	{
		for (int num = m_huds.Count - 1; num >= 0; num--)
		{
			if (m_huds[num] != null)
			{
				m_huds[num].Destroy();
			}
		}
		m_huds.Clear();
		m_hudRoot = null;
		m_wndParent = null;
	}

	public void Update()
	{
		UpdateWorldToPanelCache();
		for (int num = m_huds.Count - 1; num >= 0; num--)
		{
			if (m_huds[num] != null && m_huds[num].active)
			{
				m_huds[num].Update();
				m_huds[num].UpdateHudPos();
			}
		}
	}

	public void UpdateHudPos()
	{
		for (int i = 0; i < m_huds.Count; i++)
		{
			if (m_huds[i].active)
			{
				m_huds[i].UpdateHudPos();
			}
		}
	}

	public void UpdateWorldToPanelCache()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (m_hudRoot != null && m_hudRoot.panel != null)
		{
			Camera mainCamera = zbCameraHelper.Instance.mainCamera;
			Vector2 val = RuntimePanelUtils.CameraTransformWorldToPanel(m_hudRoot.panel, Vector3.zero, mainCamera);
			Vector2 val2 = RuntimePanelUtils.CameraTransformWorldToPanel(m_hudRoot.panel, Vector3.right, mainCamera);
			Vector2 val3 = RuntimePanelUtils.CameraTransformWorldToPanel(m_hudRoot.panel, Vector3.up, mainCamera);
			m_worldToPanelScale.x = val2.x - val.x;
			m_worldToPanelScale.y = val3.y - val.y;
			m_worldToPanelOffset = val;
		}
	}
}
