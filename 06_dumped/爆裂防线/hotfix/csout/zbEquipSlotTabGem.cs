using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEquipSlotTabGem
{
	private zbEquipSlotWnd m_wndParent;

	private VisualElement m_RootElement;

	private ScrollView m_scrollView;

	private VisualElement[] m_nodeGems = (VisualElement[])(object)new VisualElement[10];

	private VisualElement[] m_spriteFrames = (VisualElement[])(object)new VisualElement[10];

	private VisualElement[] m_spriteIcons = (VisualElement[])(object)new VisualElement[10];

	private VisualElement[] m_spriteLocks = (VisualElement[])(object)new VisualElement[10];

	private Label[] m_lbDescs = (Label[])(object)new Label[10];

	private Label[] m_lbNones = (Label[])(object)new Label[10];

	private Button[] m_btnDowns = (Button[])(object)new Button[10];

	private EquipSlotInfo slotInfo;

	private EquipSlotDetail m_data;

	private EEquipType m_equipType;

	private List<GemSlotDetail> m_sortedGemList;

	public void OnEnable(zbEquipSlotWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		for (int i = 0; i < 10; i++)
		{
			m_nodeGems[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "gem" + i, (string)null);
			m_spriteFrames[i] = UQueryExtensions.Q<VisualElement>(m_nodeGems[i], "frame", (string)null);
			m_spriteIcons[i] = UQueryExtensions.Q<VisualElement>(m_nodeGems[i], "icon", (string)null);
			m_spriteLocks[i] = UQueryExtensions.Q<VisualElement>(m_nodeGems[i], "lock", (string)null);
			m_lbDescs[i] = UQueryExtensions.Q<Label>(m_nodeGems[i], "lb_desc", (string)null);
			m_lbNones[i] = UQueryExtensions.Q<Label>(m_nodeGems[i], "lb_none", (string)null);
			m_btnDowns[i] = UQueryExtensions.Q<Button>(m_nodeGems[i], "btn_down", (string)null);
			int curIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDowns[i], (Action<ClickEvent>)delegate
			{
				OnDowns(curIndex);
			}, "", (TrickleDown)0);
		}
	}

	public void Refresh(EquipSlotInfo _slotInfo, EEquipType equipType)
	{
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		slotInfo = _slotInfo;
		m_equipType = equipType;
		m_data = zbSelf.Instance.equipHandler.GetEquipSlotInfo(equipType, _slotInfo);
		if (m_data == null)
		{
			return;
		}
		m_sortedGemList = new List<GemSlotDetail>((IEnumerable<GemSlotDetail>)zbSelf.Instance.equipHandler.GetGemSlotList(m_equipType, slotInfo));
		m_sortedGemList.Sort(delegate(GemSlotDetail a, GemSlotDetail b)
		{
			bool flag = a.GemTid > 0;
			bool flag2 = b.GemTid > 0;
			return (flag != flag2) ? flag2.CompareTo(flag) : a.Index.CompareTo(b.Index);
		});
		int num = 0;
		foreach (GemSlotDetail sortedGem in m_sortedGemList)
		{
			if (sortedGem.GemTid > 0)
			{
				zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(sortedGem.GemTid);
				if (recordByKey != null)
				{
					string resourceFrameTex = zbResourceHelper.GetResourceFrameTex(zbResourceHelper.GetResourceQuality(EResourceType.Gem, sortedGem.GemTid));
					Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
					m_spriteFrames[num].style.backgroundImage = StyleBackground.op_Implicit(val);
					Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
					m_spriteIcons[num].style.backgroundImage = StyleBackground.op_Implicit(val2);
				}
				if (zbSelf.Instance.equipHandler.IsGemLock(sortedGem.GemTid) && m_wndParent.m_visUpgrade)
				{
					m_spriteLocks[num].visible = true;
				}
				else
				{
					m_spriteLocks[num].visible = false;
				}
				zbLabelLinkUtil.BuildModLink(zbResourceHelper.GetResorceIconPath(EResourceType.Gem, sortedGem.GemTid).strDesc, m_lbDescs[num]);
				m_spriteFrames[num].visible = true;
				((VisualElement)m_lbDescs[num]).visible = true;
				((VisualElement)m_lbNones[num]).visible = false;
				((VisualElement)m_btnDowns[num]).visible = m_wndParent.m_visUpgrade;
			}
			else
			{
				m_spriteFrames[num].visible = false;
				((VisualElement)m_lbDescs[num]).visible = false;
				((VisualElement)m_lbNones[num]).visible = true;
				((VisualElement)m_btnDowns[num]).visible = false;
				m_spriteLocks[num].visible = false;
			}
			num++;
		}
		m_scrollView.scrollOffset = Vector2.zero;
	}

	private void OnDowns(int index)
	{
		if (m_sortedGemList[index] != null)
		{
			zbWsMsgHandler.Instance.EquipSlot_GemRemove_Req(zbServerData.sGameData.EquipSlotInfo.GemPlanId, m_data.EquipType, m_sortedGemList[index].Index);
		}
	}
}
