using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbWorldResourceDetailWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Label m_lbName;

	private Label m_lbDesc;

	private zbItem m_item;

	private Label m_lbCount;

	private Button m_btnTouch;

	protected override void Awake()
	{
		base.Awake();
		m_item = new zbItem();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(WorldMapGridDetail gridDetail)
	{
		zbWorldBuildingCsvRecord recordByKey = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(gridDetail.BuildingId);
		if (recordByKey != null)
		{
			((TextElement)m_lbName).text = recordByKey.name;
			((TextElement)m_lbDesc).text = recordByKey.desc;
			m_item.Refresh(new zbItemData((EResourceType)recordByKey.resOut[0], recordByKey.resOut[1], recordByKey.resOut[2]), isNameVis: false, isCountVis: false);
			((TextElement)m_lbCount).text = recordByKey.resOut[2] + "/H";
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
