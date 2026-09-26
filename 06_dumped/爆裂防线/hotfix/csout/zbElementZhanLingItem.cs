using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbElementZhanLingItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbElementZhanLingItem>
	{
	}

	private zbElementTrialWnd m_wndParent;

	private Label m_lbLv;

	private zbItem m_item0;

	private zbItem m_item1;

	private VisualElement m_grayMask;

	private zbZhanlingElementCsvRecord m_record;

	private VisualElement m_item0Lock;

	private VisualElement m_item1Lock;

	public zbElementZhanLingItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/heroTrial/tb_elementZhanling.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbElementTrialWnd wndParent)
	{
		m_wndParent = wndParent;
		m_lbLv = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_lv", (string)null);
		m_item0 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item0", (string)null);
		m_item0Lock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_item0, "icon_lock", (string)null);
		m_item0.OnEnable(wndParent);
		m_item1 = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item1", (string)null);
		m_item1Lock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_item1, "icon_lock", (string)null);
		m_item1.OnEnable(wndParent);
		m_grayMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "gray_mask", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbZhanlingElementCsvRecord record)
	{
		m_record = record;
		((TextElement)m_lbLv).text = record.trialLevel.ToString();
		InitAward();
	}

	private void InitAward()
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		ZhanLingCommonInfo zhanLingElementInfo = zbServerData.sGameData.RechargeActiveInfo.ZhanLingElementInfo;
		int elementZhanlingTotalLevel = zbSelf.Instance.elementTrialHandler.GetElementZhanlingTotalLevel();
		bool flag = elementZhanlingTotalLevel >= m_record.trialLevel;
		bool flag2 = m_record.trialLevel <= zhanLingElementInfo.LowAward;
		bool flag3 = m_record.trialLevel <= zhanLingElementInfo.HighAward && zhanLingElementInfo.Active == EBoolState.True;
		bool canReceiveLowAward = !flag2 & flag;
		bool canReceiveHighAward = (!flag3 & flag) && zhanLingElementInfo.Active == EBoolState.True;
		bool flag4 = elementZhanlingTotalLevel >= m_record.trialLevel;
		m_item0Lock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag4 ? 1 : 0));
		bool flag5 = elementZhanlingTotalLevel >= m_record.trialLevel && zhanLingElementInfo.Active == EBoolState.True;
		m_item1Lock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag5 ? 1 : 0));
		SetMaskVisible(!flag);
		zbItemData data = new zbItemData((EResourceType)m_record.lowAward[0][0], m_record.lowAward[0][1], m_record.lowAward[0][2]);
		m_item0.Refresh(data);
		m_item0.SetGettedVisible(flag2);
		m_item0.SetSpineVisible(canReceiveLowAward);
		m_item0.SetTouchCallback(delegate
		{
			if (canReceiveLowAward)
			{
				ReviceAward();
			}
			else
			{
				zbUIMgr.Instance.ShowTip((EResourceType)m_record.lowAward[0][0], m_record.lowAward[0][1], 0L);
			}
		});
		zbItemData data2 = new zbItemData((EResourceType)m_record.highAward[0][0], m_record.highAward[0][1], m_record.highAward[0][2]);
		m_item1.Refresh(data2);
		m_item1.SetGettedVisible(flag3);
		m_item1.SetSpineVisible(canReceiveHighAward);
		m_item1.SetTouchCallback(delegate
		{
			if (canReceiveHighAward)
			{
				ReviceAward();
			}
			else
			{
				zbUIMgr.Instance.ShowTip((EResourceType)m_record.highAward[0][0], m_record.highAward[0][1], 0L);
			}
		});
	}

	private void ReviceAward()
	{
		zbZhanlingElementCsvRecord zbZhanlingElementCsvRecord2 = null;
		foreach (zbZhanlingElementCsvRecord value in zbDataConfig.Instance.zhanlingElementCsvTable.GetRecords().records.Values)
		{
			if (zbServerData.sGameData.ElementTrialInfo.TotalLevel >= value.trialLevel && (zbZhanlingElementCsvRecord2 == null || value.trialLevel > zbZhanlingElementCsvRecord2.trialLevel))
			{
				zbZhanlingElementCsvRecord2 = value;
			}
		}
		if (zbZhanlingElementCsvRecord2 != null)
		{
			zbWsMsgHandler.Instance.RechargeActive_ZhanLingElementAward_Req(zbZhanlingElementCsvRecord2.trialLevel);
		}
	}

	public void SetMaskVisible(bool vis)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_grayMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
	}
}
