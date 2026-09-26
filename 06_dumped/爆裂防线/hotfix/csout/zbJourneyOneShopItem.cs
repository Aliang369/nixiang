using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyOneShopItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbJourneyOneShopItem>
	{
	}

	private zbJourneyOneShopWnd m_wndParent;

	private Label m_lbLimit;

	private Label m_lbName;

	private zbItem m_item;

	private Button m_btnTouch;

	private VisualElement m_selectBg;

	private JourneyCountRecordDetail m_detail;

	private VisualElement m_notifiy;

	private int m_index;

	private zbChapterOneExchangeCsvRecord m_record;

	public zbJourneyOneShopItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/journey/journeyOne/tb_journeyOneShop.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbJourneyOneShopWnd wndParent)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_lbLimit = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_limit", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_selectBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "select_bg", (string)null);
		m_notifiy = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-3f, 0f), 1f, true);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(JourneyCountRecordDetail detail, int index)
	{
		m_detail = detail;
		m_index = index;
		m_record = zbDataConfig.Instance.chapterOneExchangeCsvTable.GetRecordByKey(detail.Id);
		InitItem();
		InitNotify();
	}

	private void InitNotify()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		bool shopRemindState = zbSelf.Instance.journeyOneHandler.GetShopRemindState(m_record.id);
		bool flag = m_record.limitNum == 0 || m_detail.Count < m_record.limitNum;
		bool flag2 = zbSelf.Instance.moneyHandler.HasEnoughMoney(m_record.moneyType, m_record.price);
		bool flag3 = shopRemindState & flag & flag2;
		m_notifiy.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
	}

	private void InitItem()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		zbItemData data = new zbItemData((EResourceType)m_record.award[0], m_record.award[1], m_record.award[2]);
		m_item.Refresh(data);
		bool flag = m_record.limitNum > 0;
		((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((TextElement)m_lbLimit).text = zbUtil.Language("lang.10805") + m_detail.Count + "/" + m_record.limitNum;
		((TextElement)m_lbName).text = zbUtil.Language(zbResourceHelper.GetResorceIconPath((EResourceType)m_record.award[0], m_record.award[1]).strName);
	}

	private void OnTouch()
	{
		m_wndParent.SetSelect(m_index);
	}

	public void SetSelectBg(bool vis)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_selectBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
	}
}
