using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFacilityItem : zbViewBaseItem
{
	private zbFacilityHubWnd m_wndParent;

	private VisualElement m_headFrame;

	private VisualElement m_head;

	private Label m_lbName;

	private Label m_lbCanExploreRoom;

	private Label m_lbLeftTime;

	private Button m_btnTouch;

	private VisualElement m_bg;

	private Label m_lbfacilityName;

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[6];

	private Button m_btnHead;

	private VisualElement m_notify;

	private FacilityBaseInfo m_facilityBaseInfo;

	private zbFacilityCsvRecord m_record;

	public zbFacilityItem(zbFacilityHubWnd wndParent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/facility/tb_facility.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_wndParent = wndParent;
		m_headFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "headFrame", (string)null);
		m_head = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "head", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_head", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnHead, "", (TrickleDown)0);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbCanExploreRoom = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_canExploreRoom", (string)null);
		m_lbLeftTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_leftTime", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "root", (string)null);
		m_lbfacilityName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_facilityName", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "star" + i, (string)null);
		}
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)this);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(FacilityBaseInfo facilityBaseInfo)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		m_facilityBaseInfo = facilityBaseInfo;
		zbSelf.Instance.worldLairHandler.RefreshHead(m_headFrame, m_facilityBaseInfo.FacilityCreator);
		m_record = zbDataConfig.Instance.facilityCsvTable.GetRecordByKey(m_facilityBaseInfo.FacilityType);
		m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/facility/facility_bg{m_record.type}.png"));
		((TextElement)m_lbName).text = m_facilityBaseInfo.FacilityCreator.RoleName;
		int leftShareRoomCount = zbSelf.Instance.facilityHandler.GetLeftShareRoomCount(m_record, m_facilityBaseInfo);
		((TextElement)m_lbCanExploreRoom).text = leftShareRoomCount + "/" + m_record.shareCount;
		((TextElement)m_lbfacilityName).text = zbUtil.Language(m_record.name);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.moneyHandler.HasEnoughMoney(4, zbSelf.Instance.facilityHandler.tiliCost * 2) || leftShareRoomCount <= 0) ? 1 : 0));
		RefreshLeftTime();
		RefreshStars();
	}

	private void RefreshStars()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		int quality = m_record.quality;
		for (int i = 0; i < 6; i++)
		{
			bool flag = i < quality;
			m_stars[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		}
	}

	private void OnTouch()
	{
		zbWsMsgHandler.Instance.Facility_View_Req(m_facilityBaseInfo.FacilityId, openFacilityWnd: true);
	}

	private void OnHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_facilityBaseInfo.FacilityCreator.RoleId);
	}

	public void RefreshLeftTime()
	{
		((TextElement)m_lbLeftTime).text = zbUtil.FormatTimeHHMMSS(m_facilityBaseInfo.ExpireTm - zbSelf.Instance.timeHandler.GetServerTime());
	}
}
