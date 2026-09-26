using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHomeCreateDetailWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private VisualElement[] m_nodeCosts = (VisualElement[])(object)new VisualElement[2];

	private VisualElement[] m_spriteCosts = (VisualElement[])(object)new VisualElement[2];

	private Label[] m_lbCosts = (Label[])(object)new Label[2];

	private Label m_lbCount;

	private Button m_btnBag;

	private Button m_btnClose;

	private int[][] m_costList;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_nodeCosts[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "cost" + i, (string)null);
			m_spriteCosts[i] = UQueryExtensions.Q<VisualElement>(m_nodeCosts[i], "cost", (string)null);
			m_lbCosts[i] = UQueryExtensions.Q<Label>(m_nodeCosts[i], "lb_cost", (string)null);
		}
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_btnBag = UQueryExtensions.Q<Button>(m_RootElement, "btn_bag", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBag, (Action<ClickEvent>)OnBag, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_HomelandRoomCreate_Ack, OnRefreshAck);
	}

	public override void Refresh()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.homeEditHandler.createRoomRecord == null)
		{
			return;
		}
		((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.10111"), zbSelf.Instance.homeEditHandler.createRoomRecord.name);
		int roomCountByType = zbSelf.Instance.homelandHandler.GetRoomCountByType(zbSelf.Instance.homeEditHandler.createRoomRecord.id);
		int maxNum = zbSelf.Instance.homeEditHandler.createRoomRecord.maxNum;
		if (roomCountByType >= maxNum)
		{
			((VisualElement)m_lbCount).style.color = StyleColor.op_Implicit(Color.red);
		}
		else
		{
			((VisualElement)m_lbCount).style.color = StyleColor.op_Implicit(Color.white);
		}
		((TextElement)m_lbCount).text = roomCountByType + "/" + maxNum;
		for (int i = 0; i < 2; i++)
		{
			m_nodeCosts[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (zbSelf.Instance.homeEditHandler.createRoomRecord.type == 3)
		{
			zbRoomUpgradeEffectCsvRecord[] records = zbDataConfig.Instance.roomUpgradeEffectCsvTable.GetRecords().records;
			foreach (zbRoomUpgradeEffectCsvRecord zbRoomUpgradeEffectCsvRecord2 in records)
			{
				if (zbRoomUpgradeEffectCsvRecord2.id == zbSelf.Instance.homeEditHandler.createRoomRecord.length && zbRoomUpgradeEffectCsvRecord2.lv == 1)
				{
					m_costList = zbSelf.Instance.homelandHandler.AppendExtraCostItem(zbRoomUpgradeEffectCsvRecord2.costList, zbSelf.Instance.homeEditHandler.createRoomRecord.itemId, zbRoomUpgradeEffectCsvRecord2.costItemNum);
				}
			}
		}
		if (m_costList == null)
		{
			return;
		}
		int num = 0;
		int[][] costList = m_costList;
		foreach (int[] array in costList)
		{
			if (num < m_nodeCosts.Length)
			{
				m_nodeCosts[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)array[0], array[1]);
				m_spriteCosts[num].style.backgroundImage = StyleBackground.op_Implicit(zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath.strIconPath));
				((TextElement)m_lbCosts[num]).text = array[2].ToString();
				long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)array[0], array[1]);
				((TextElement)m_lbCosts[num]).text = zbUtil.GetNeedHaveRichString(resourceCount, array[2]);
			}
			num++;
		}
	}

	private void OnBag(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowBag();
	}

	public void OnClose()
	{
		zbSelf.Instance.homeEditHandler.createRoomRecord = null;
		zbSelf.Instance.homeEditHandler.createRoomCostList = null;
		Hide();
		zbHomeMgr.Instance.floorMgr.Refresh();
		zbHomeMgr.Instance.roomMgr.Refresh();
		zbHomeWnd zbHomeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Home) as zbHomeWnd;
		if ((Object)(object)zbHomeWnd2 != (Object)null)
		{
			zbHomeWnd2.SetUIVisible(vis: true);
		}
	}

	private void OnRefreshAck(object data)
	{
		Refresh();
	}
}
