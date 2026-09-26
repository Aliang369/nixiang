using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMineFacility
{
	private zbFacilityHubWnd m_wndParent;

	private VisualElement m_root;

	private zbFacilityMineItem[] m_items = new zbFacilityMineItem[4];

	private Label m_lbCost;

	private zbItem m_costItem;

	private Button m_btnCreate;

	private VisualElement m_dialogBox;

	private Label m_lbLeftTime;

	private VisualElement m_facilitys;

	private VisualElement m_notify;

	private zbFacilityCsvRecord m_record;

	private bool m_LastHasFacility;

	private bool m_hasFacility;

	private VisualElement m_nodeTarget;

	private VisualElement m_spriteProgress;

	private Label m_lbProgress;

	private VisualElement[] m_nodeTargets = (VisualElement[])(object)new VisualElement[3];

	private zbItem[] m_itemTargets = new zbItem[3];

	private Label[] m_lbTargets = (Label[])(object)new Label[3];

	private Label m_lbDesc;

	private Button m_btnProgress;

	private VisualElement m_tip;

	private VisualElement m_doubleAward;

	private IVisualElementScheduledItem m_hideSchedule;

	public void OnEnable(zbFacilityHubWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_root = root;
		for (int i = 0; i < 4; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbFacilityMineItem>(m_root, "node" + i, (string)null);
			m_items[i].OnEnable(m_wndParent, this, i);
		}
		m_lbCost = UQueryExtensions.Q<Label>(m_root, "lb_cost0", (string)null);
		m_costItem = UQueryExtensions.Q<zbItem>((VisualElement)(object)m_lbCost, "itemCost", (string)null);
		m_costItem.OnEnable(m_wndParent);
		m_btnCreate = UQueryExtensions.Q<Button>(m_root, "btn_create", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCreate, (Action)OnCreate, "", (TrickleDown)0);
		m_dialogBox = UQueryExtensions.Q<VisualElement>(m_root, "dialogBox", (string)null);
		m_lbLeftTime = UQueryExtensions.Q<Label>(m_root, "lb_leftTime", (string)null);
		m_facilitys = UQueryExtensions.Q<VisualElement>(m_root, "facilitys", (string)null);
		m_nodeTarget = UQueryExtensions.Q<VisualElement>(m_root, "node_target", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_progress", (string)null);
		for (int j = 0; j < 3; j++)
		{
			m_nodeTargets[j] = UQueryExtensions.Q<VisualElement>(m_nodeTarget, $"target{j}", (string)null);
			m_lbTargets[j] = UQueryExtensions.Q<Label>(m_nodeTargets[j], "lb_target", (string)null);
			m_itemTargets[j] = UQueryExtensions.Q<zbItem>(m_nodeTargets[j], "item", (string)null);
			m_itemTargets[j].OnEnable(m_wndParent);
		}
		m_lbDesc = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_desc", (string)null);
		m_btnProgress = UQueryExtensions.Q<Button>(m_nodeTarget, "btn_progress", (string)null);
		m_tip = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "tip", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnProgress, (Action<ClickEvent>)OnShowTip, "", (TrickleDown)0);
		m_doubleAward = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "double_award", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnCreate);
	}

	public void Refresh()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_tip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		RefreshFacility();
	}

	public void RefreshFacility()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		FacilitySelfInfo facilitySelfInfo = zbServerData.sGameData.FacilitySelfInfo;
		m_LastHasFacility = facilitySelfInfo != null && facilitySelfInfo.Create != null && facilitySelfInfo.Create.ExpireTm > zbSelf.Instance.timeHandler.GetServerTime();
		m_hasFacility = zbSelf.Instance.facilityHandler.HasFacility();
		m_record = zbDataConfig.Instance.facilityCsvTable.GetRecordByKey(m_hasFacility ? facilitySelfInfo.Create.FacilityType : m_wndParent.m_facilityType);
		((Component)m_wndParent.m_bgAnim).gameObject.SetActive(m_hasFacility);
		m_facilitys.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_hasFacility ? 1 : 0));
		m_nodeTarget.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!m_hasFacility));
		m_wndParent.m_bg.sprite = zbAssetLoader.Instance.LoadSpriteAsset("ui/image/facility/" + (m_hasFacility ? "mine_facilityBg.jpg" : "facility_hubBg.jpg"));
		((TextElement)m_btnCreate).text = zbUtil.Language(m_hasFacility ? "lang.11263" : "lang.11264");
		if (m_hasFacility)
		{
			RefreshMineFacility();
			return;
		}
		RefreshCreateFacilityView();
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void RefreshMineFacility()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		((SkeletonRenderer)m_wndParent.m_bgAnim).skeleton.SetToSetupPose();
		m_wndParent.m_bgAnim.AnimationState.ClearTracks();
		m_wndParent.m_bgAnim.AnimationState.SetAnimation(0, m_record.quality.ToString(), true);
		RefreshTarget();
		m_dialogBox.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!m_hasFacility));
	}

	private void RefreshTarget()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbServerData.sGameData.FacilitySelfInfo.Create.ExploreTotal < m_record.tierTarget3) ? 1 : 0));
		((TextElement)m_lbProgress).text = zbServerData.sGameData.FacilitySelfInfo.Create.ExploreTotal.ToString();
		((TextElement)m_lbTargets[0]).text = m_record.tierTarget1.ToString();
		((TextElement)m_lbTargets[1]).text = m_record.tierTarget2.ToString();
		((TextElement)m_lbTargets[2]).text = m_record.tierTarget3.ToString();
		m_doubleAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.FacilitySelfInfo.Create.DoubleAward != EBoolState.True));
		int num = ((zbServerData.sGameData.FacilitySelfInfo.Create.DoubleAward != EBoolState.True) ? 1 : 2);
		int[][] tierAwardList = m_record.tierAwardList1;
		int num2 = 0;
		if (num2 < tierAwardList.Length)
		{
			int[] array = tierAwardList[num2];
			m_itemTargets[0].Refresh(new zbItemData((EResourceType)array[0], array[1], array[2] * num));
		}
		tierAwardList = m_record.tierAwardList2;
		num2 = 0;
		if (num2 < tierAwardList.Length)
		{
			int[] array2 = tierAwardList[num2];
			m_itemTargets[1].Refresh(new zbItemData((EResourceType)array2[0], array2[1], array2[2] * num));
		}
		tierAwardList = m_record.tierAwardList3;
		num2 = 0;
		if (num2 < tierAwardList.Length)
		{
			int[] array3 = tierAwardList[num2];
			m_itemTargets[2].Refresh(new zbItemData((EResourceType)array3[0], array3[1], array3[2] * num));
		}
		float progress = GetProgress();
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(progress * 510f, (LengthUnit)0));
	}

	private float GetProgress()
	{
		FacilitySelfInfo facilitySelfInfo = zbServerData.sGameData.FacilitySelfInfo;
		int[] array = new int[3] { m_record.tierTarget1, m_record.tierTarget2, m_record.tierTarget3 };
		float num = 1f / 3f;
		for (int i = 0; i < 3; i++)
		{
			int num2 = array[i];
			if (facilitySelfInfo.Create.ExploreTotal < num2)
			{
				int num3 = ((i != 0) ? array[i - 1] : 0);
				float num4 = (float)(facilitySelfInfo.Create.ExploreTotal - num3) / (float)(num2 - num3);
				return Mathf.Clamp01((float)i * num + num4 * num);
			}
		}
		return 1f;
	}

	private void RefreshCreateFacilityView()
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		if (m_hasFacility)
		{
			m_wndParent.m_facilityType = m_record.type;
			((TextElement)m_lbLeftTime).text = zbUtil.FormatTimeHHMMSS(zbServerData.sGameData.FacilitySelfInfo.Create.ExpireTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else
		{
			m_costItem.Refresh(new zbItemData(EResourceType.Item, m_record.createCostItem, 1L), isNameVis: false, isCountVis: false);
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Item, m_record.createCostItem);
			((TextElement)m_lbCost).text = zbUtil.GetNeedHaveRichString(resourceCount, 1L);
		}
		((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_hasFacility ? 1 : 0));
		m_dialogBox.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!m_hasFacility));
		for (int i = 0; i < 4; i++)
		{
			m_items[i].Refresh(m_record);
		}
	}

	private void OnCreate()
	{
		FacilitySelfInfo facilitySelfInfo = zbServerData.sGameData.FacilitySelfInfo;
		if (zbSelf.Instance.facilityHandler.HasFacility())
		{
			zbWsMsgHandler.Instance.Facility_View_Req(facilitySelfInfo.Create.FacilityId, openFacilityWnd: true);
		}
		else if (!zbSelf.Instance.itemHandler.HasEnoughItem(m_record.createCostItem, 1L))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Item, m_record.createCostItem, 1L);
		}
		else
		{
			zbWsMsgHandler.Instance.Facility_Create_Req(m_record.type);
		}
	}

	private void OnShowTip(ClickEvent evt)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_tip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (m_hideSchedule != null)
		{
			m_hideSchedule.Pause();
			m_hideSchedule = null;
		}
		m_hideSchedule = ((VisualElement)m_btnProgress).schedule.Execute((Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_tip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}).StartingIn(5000L);
	}

	public void UpdateAtShow()
	{
		FacilitySelfInfo facilitySelfInfo = zbServerData.sGameData.FacilitySelfInfo;
		if (facilitySelfInfo != null)
		{
			if (m_hasFacility)
			{
				((TextElement)m_lbLeftTime).text = zbUtil.FormatTimeHHMMSS(facilitySelfInfo.Create.ExpireTm - zbSelf.Instance.timeHandler.GetServerTime());
			}
			if (m_LastHasFacility && !m_hasFacility)
			{
				RefreshFacility();
			}
			m_LastHasFacility = m_hasFacility;
		}
	}
}
