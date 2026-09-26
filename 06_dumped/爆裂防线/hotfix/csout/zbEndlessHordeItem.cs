using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbEndlessHordeItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbEndlessHordeItem>
	{
	}

	private zbEndlessHordeWnd m_wndParent;

	private Label m_lbLv;

	private zbItem m_item0;

	private zbItem m_item1;

	private VisualElement m_grayMask;

	private VisualElement m_item0Lock;

	private VisualElement m_item1Lock;

	private VisualElement m_notify0;

	private VisualElement m_notify1;

	private zbZhanLingEndlessCsvRecord m_record;

	public zbEndlessHordeItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/endlessHorde/tb_endlessHorde.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbEndlessHordeWnd wndParent)
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
		m_notify0 = zbUtil.CreateNotifyTag((VisualElement)(object)m_item0);
		m_notify1 = zbUtil.CreateNotifyTag((VisualElement)(object)m_item1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbZhanLingEndlessCsvRecord record)
	{
		m_record = record;
		((TextElement)m_lbLv).text = ((float)record.distance / 1000f).ToString("0.#") + "km";
		InitAward();
	}

	private void InitAward()
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		ZhanLingCommonInfo zhanLingEndlessInfo = zbServerData.sGameData.RechargeActiveInfo.ZhanLingEndlessInfo;
		int passDistance = zbServerData.sGameData.EndlessHordeInfo.PassDistance;
		bool flag = passDistance >= m_record.distance;
		bool flag2 = m_record.distance <= zhanLingEndlessInfo.LowAward;
		bool flag3 = m_record.distance <= zhanLingEndlessInfo.HighAward && zhanLingEndlessInfo.Active == EBoolState.True;
		bool canReceiveLowAward = !flag2 & flag;
		bool canReceiveHighAward = (!flag3 & flag) && zhanLingEndlessInfo.Active == EBoolState.True;
		bool flag4 = passDistance >= m_record.distance;
		m_item0Lock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag4 ? 1 : 0));
		bool flag5 = passDistance >= m_record.distance && zhanLingEndlessInfo.Active == EBoolState.True;
		m_item1Lock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag5 ? 1 : 0));
		SetMaskVisible(!flag);
		zbItemData data = new zbItemData((EResourceType)m_record.lowAward[0][0], m_record.lowAward[0][1], m_record.lowAward[0][2]);
		m_item0.Refresh(data);
		m_item0.SetGettedVisible(flag2);
		m_item0.SetSpineVisible(canReceiveLowAward);
		m_notify0.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!canReceiveLowAward));
		m_item0.SetTouchCallback(delegate
		{
			if (canReceiveLowAward)
			{
				zbWsMsgHandler.Instance.RechargeActive_ZhanLingEndlessAward_Req(m_record.distance);
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
		m_notify1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!canReceiveHighAward));
		m_item1.SetTouchCallback(delegate
		{
			if (canReceiveHighAward)
			{
				zbWsMsgHandler.Instance.RechargeActive_ZhanLingEndlessAward_Req(m_record.distance);
			}
			else
			{
				zbUIMgr.Instance.ShowTip((EResourceType)m_record.highAward[0][0], m_record.highAward[0][1], 0L);
			}
		});
	}

	public void SetMaskVisible(bool vis)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_grayMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
	}
}
