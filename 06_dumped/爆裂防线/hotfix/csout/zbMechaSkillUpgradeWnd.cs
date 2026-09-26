using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaSkillUpgradeWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private VisualElement m_spriteSkill;

	private Label m_lbLevel;

	private Label m_lbName;

	private Label m_lbDesc;

	private ListView m_listView;

	private List<zbMechaSkillUpgradeCsvRecord> listViewDatas = new List<zbMechaSkillUpgradeCsvRecord>();

	private Label[] m_costLabels = (Label[])(object)new Label[2];

	private zbItem[] m_items = new zbItem[2];

	private Button m_btnUpgrade;

	private Label m_lbLimit;

	private Label m_lbMax;

	private Button m_btnClose;

	private VisualElement m_notify;

	private zbMechaCsvRecord m_mechaRecord;

	private MechaDetail m_mechaDetail;

	private MechaSkillDetail m_skillDetail;

	private zbMechaSkillCsvRecord m_skillRecord;

	private zbMechaSkillUpgradeCsvRecord m_curSkillUpRecord;

	private zbMechaSkillUpgradeCsvRecord m_nextSkillUpRecord;

	private int[][] m_upgradeCostList;

	private bool m_isFirstIn = true;

	protected override void OnEnable()
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/mecha/tb_mechaSkillUpgrade.uxml");
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		for (int i = 0; i < 2; i++)
		{
			m_costLabels[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + i, (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
			((VisualElement)m_costLabels[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_lbLimit = UQueryExtensions.Q<Label>(m_RootElement, "lb_limit", (string)null);
		m_lbMax = UQueryExtensions.Q<Label>(m_RootElement, "lb_max", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(-3f, -5f), 1f, true);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MechaInfo, OnEventUpdate);
	}

	public void RefreshCustom(zbMechaCsvRecord mechaRecord, MechaSkillDetail skillDetail, bool isFirstIn = true)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		m_mechaRecord = mechaRecord;
		m_mechaDetail = zbSelf.Instance.mechaHandler.GetMechaDetail(m_mechaRecord.id);
		m_skillDetail = zbSelf.Instance.mechaHandler.GetMechaSkillDetail(m_mechaRecord.id, skillDetail.Index);
		m_isFirstIn = isFirstIn;
		m_skillRecord = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(m_skillDetail.SkillId);
		if (m_skillRecord != null)
		{
			m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_skillRecord.icon));
			((TextElement)m_lbName).text = m_skillRecord.name;
			((TextElement)m_lbDesc).text = zbSelf.Instance.mechaHandler.GetMechaSkillDesc(m_skillRecord.id, m_skillDetail.Level);
		}
		m_curSkillUpRecord = zbDataConfig.Instance.mechaSkillUpgradeCsvTable.GetRecords()?.records.FirstOrDefault((zbMechaSkillUpgradeCsvRecord r) => r.quality == m_mechaRecord.quality && r.lv == m_skillDetail.Level);
		if (m_curSkillUpRecord != null)
		{
			((TextElement)m_lbLevel).text = "Lv." + m_curSkillUpRecord.lv;
		}
		UpdateListView();
		RefreshCostList();
	}

	private void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		listViewDatas.Clear();
		zbMechaSkillUpgradeCsvRecord[] records = zbDataConfig.Instance.mechaSkillUpgradeCsvTable.GetRecords().records;
		foreach (zbMechaSkillUpgradeCsvRecord zbMechaSkillUpgradeCsvRecord2 in records)
		{
			if (m_mechaRecord.quality == zbMechaSkillUpgradeCsvRecord2.quality)
			{
				listViewDatas.Add(zbMechaSkillUpgradeCsvRecord2);
			}
		}
		m_listView.makeItem = () => (VisualElement)(object)new zbMechaSkillUpgradeItem(m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbMechaSkillUpgradeItem).Refresh(listViewDatas[index].lv, m_skillDetail, m_skillRecord);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = listViewDatas;
		if (m_isFirstIn)
		{
			m_isFirstIn = false;
			((MonoBehaviour)this).StartCoroutine(ScrollToAnimation());
		}
	}

	private void RefreshCostList()
	{
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		m_nextSkillUpRecord = zbDataConfig.Instance.mechaSkillUpgradeCsvTable.GetRecords()?.records.FirstOrDefault((zbMechaSkillUpgradeCsvRecord r) => r.quality == m_mechaRecord.quality && r.lv == m_skillDetail.Level + 1);
		if (m_nextSkillUpRecord != null)
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(m_mechaDetail?.Level >= m_nextSkillUpRecord.mechaLv)));
			((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(m_mechaDetail?.Level < m_nextSkillUpRecord.mechaLv)));
			((TextElement)m_lbLimit).text = string.Format(zbUtil.Language("lang.10854"), m_mechaRecord.name, m_nextSkillUpRecord.mechaLv);
			m_upgradeCostList = zbSelf.Instance.homelandHandler.AppendExtraCostItem(new int[0][], m_mechaRecord.skillCost[1], m_nextSkillUpRecord.costNum, m_mechaRecord.skillCost[0]);
		}
		else
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_upgradeCostList = new int[0][];
		}
		zbSelf.Instance.homelandHandler.UpdateCostUI(m_upgradeCostList, m_costLabels, m_items);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.HasSkillUpgradeNotify(m_mechaRecord.id, m_skillDetail.Index)));
	}

	private IEnumerator ScrollToAnimation()
	{
		yield return null;
		int num = listViewDatas.FindIndex((zbMechaSkillUpgradeCsvRecord d) => d.lv == m_skillDetail?.Level);
		num = Math.Min(listViewDatas.Count - 1, num + 1);
		zbUtil.ScrollVerticalToIndex(UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null), 6, listViewDatas.Count, 106f, num);
	}

	private void OnUpgrade()
	{
		if (m_mechaDetail != null && m_nextSkillUpRecord != null && m_mechaDetail.Level < m_nextSkillUpRecord.mechaLv)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10854"), m_mechaRecord.name, m_nextSkillUpRecord.mechaLv));
			return;
		}
		int[][] upgradeCostList = m_upgradeCostList;
		foreach (int[] array in upgradeCostList)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array[0], array[1], array[2]))
			{
				zbUIMgr.Instance.ShowTip((EResourceType)array[0], array[1], array[2]);
				return;
			}
		}
		zbWsMsgHandler.Instance.Mecha_UpgradeSkill_Req(m_mechaRecord.id, m_skillDetail.Index);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object obj)
	{
		RefreshCustom(m_mechaRecord, m_skillDetail, isFirstIn: false);
	}
}
