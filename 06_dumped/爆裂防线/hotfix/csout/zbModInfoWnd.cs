using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbModInfoWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private ListView m_listView;

	private Button m_btnBack;

	private VisualElement m_banner;

	private zbModHandbookTypeCsvRecord m_ModHandBookTypeRecord;

	private ModHandbookTypeTarget m_modSkillTypeTarget;

	private int m_skillType;

	private List<zbModHandbookCsvRecord> m_modList;

	private VisualElement m_spriteProgress;

	private zbItem m_item;

	private Label lb_modCount;

	private Button m_btnGetTarget;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/encyclopedia/tb_modInfoItem.uxml");
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_banner = UQueryExtensions.Q<VisualElement>(m_RootElement, "banner", (string)null);
		lb_modCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_modCount", (string)null);
		m_btnGetTarget = UQueryExtensions.Q<Button>(m_RootElement, "btn_getTarget", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).mode = (ScrollViewMode)0;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 140f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGetTarget, (Action<ClickEvent>)OnGetAward, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Handbook, RefreshUI);
		m_gameEevent.RegisterEvent(EGameEvent.Event_ModActive_Ack, RefreshUI);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
	}

	public void RefreshCustom(zbModHandbookTypeCsvRecord record)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		m_ModHandBookTypeRecord = record;
		m_skillType = record.type;
		m_banner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/encyclopedia/banner/{m_skillType}.jpg"));
		RefreshList();
		RefreshPrpgress();
	}

	private void RefreshUI(object obj)
	{
		RefreshList();
		RefreshPrpgress();
	}

	private void RefreshList()
	{
		m_modList = zbDataConfig.Instance.modHandbookCsvTable.GetRecords().records.Values.Where((zbModHandbookCsvRecord record) => record.type == m_skillType).ToList();
		m_listView.makeItem = () => (VisualElement)(object)new zbModInfoItem(this, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (m_modList[index] != null)
			{
				(element as zbModInfoItem).Init(m_modList[index]);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_modList;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	private void RefreshPrpgress()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		m_modSkillTypeTarget = zbSelf.Instance.handbookHandler.GetModHandbookTypeTargetDetail(m_skillType);
		zbItemData data = new zbItemData((EResourceType)m_ModHandBookTypeRecord.award[0], m_ModHandBookTypeRecord.award[1], m_ModHandBookTypeRecord.award[2]);
		m_item.Refresh(data);
		if (m_modSkillTypeTarget != null)
		{
			float num = Mathf.Min((float)m_modSkillTypeTarget.Count / (float)m_ModHandBookTypeRecord.unlock, 1f);
			m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num * 120f, (LengthUnit)0));
			((TextElement)lb_modCount).text = $"{m_modSkillTypeTarget.Count}/{m_ModHandBookTypeRecord.unlock}";
			m_item.SetGettedVisible(m_modSkillTypeTarget.State == EActiveState.Done);
			m_item.SetSpineVisible(zbSelf.Instance.handbookHandler.CanGetHandbookTypeTargetAward(m_skillType));
			((VisualElement)m_btnGetTarget).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.handbookHandler.CanGetHandbookTypeTargetAward(m_skillType)));
		}
		else
		{
			((TextElement)lb_modCount).text = $"0/{m_ModHandBookTypeRecord.unlock}";
		}
	}

	private void OnGetAward(ClickEvent evt)
	{
		if (m_modSkillTypeTarget != null && m_modSkillTypeTarget.State == EActiveState.Active && m_modSkillTypeTarget.Count >= m_ModHandBookTypeRecord.unlock && m_modSkillTypeTarget.SkillType == (ESkillType)m_ModHandBookTypeRecord.type)
		{
			zbWsMsgHandler.Instance.Handbook_ModTypeTargetAward_Req((ESkillType)m_ModHandBookTypeRecord.type);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
