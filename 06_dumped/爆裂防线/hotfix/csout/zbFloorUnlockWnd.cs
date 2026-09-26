using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbFloorUnlockWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Button m_btnUnlock;

	private VisualElement m_costList;

	private Label[] m_costLabels = (Label[])(object)new Label[3];

	private zbItem[] m_items = new zbItem[3];

	private int m_floor;

	private int[][] m_unlockCostList;

	protected override void OnEnable()
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnUnlock = UQueryExtensions.Q<Button>(m_RootElement, "btn_unlock", (string)null);
		m_costList = UQueryExtensions.Q<VisualElement>(m_RootElement, "costList", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_costLabels[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + i, (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
			((VisualElement)m_costLabels[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUnlock, (Action<ClickEvent>)OnUnlock, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(int floor)
	{
		m_floor = floor;
		if (zbDataConfig.Instance.homeFloorCsvTable.GetRecordByKey(floor) != null)
		{
			zbSelf.Instance.homelandHandler.UpdateCostUI(m_unlockCostList, m_costLabels, m_items);
		}
	}

	private void OnUnlock(ClickEvent evt)
	{
		int[][] unlockCostList = m_unlockCostList;
		foreach (int[] array in unlockCostList)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array[0], array[1], array[2]))
			{
				zbUIMgr.Instance.ShowTip((EResourceType)array[0], array[1], array[2]);
				return;
			}
		}
		zbWsMsgHandler.Instance.HomelandV2_FloorUnlock_Req(m_floor);
		Hide();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
