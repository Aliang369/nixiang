using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbVipTabVipCardItem : zbViewBaseItem
{
	private zbVipWnd m_wndParent;

	private zbVipTabVipCard m_parent;

	private EVipCardTab m_tab;

	private VisualElement m_spriteSelect;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private Label m_lbActived;

	private Button m_btnTouch;

	private VisualElement m_notify;

	public zbVipTabVipCardItem(zbVipWnd wndParent, zbVipTabVipCard parent, EVipCardTab tab)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_wndParent = wndParent;
		m_parent = parent;
		m_tab = tab;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/vip/tb_vipCard.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_select", (string)null);
		m_spriteBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbActived = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_actived", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(0f, -2f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		InitRecord();
	}

	private void InitRecord()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		m_spriteBg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/vip/card/vip_vipCard_tab{(int)m_tab}.jpg"));
		int num = (int)(11199 + m_tab);
		((TextElement)m_lbName).text = zbUtil.Language("lang." + num);
	}

	public void Refresh()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_parent.m_curTab != m_tab) ? 1 : 0));
		((VisualElement)m_lbActived).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!IsActived()));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.vipHandler.HasVipCardNotifyByTab(m_tab)));
	}

	private bool IsActived()
	{
		if (m_tab == EVipCardTab.Week)
		{
			return zbSelf.Instance.vipHandler.ValidateHasWeekCard();
		}
		if (m_tab == EVipCardTab.Truck)
		{
			if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Truck) && zbSelf.Instance.truckHandler.IsTruckOpen())
			{
				GameData sGameData = zbServerData.sGameData;
				if (sGameData == null)
				{
					return false;
				}
				return sGameData.TruckSelfInfo?.Privilege == EBoolState.True;
			}
		}
		else if (m_tab == EVipCardTab.Treasure)
		{
			if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Treasure))
			{
				GameData sGameData2 = zbServerData.sGameData;
				if (sGameData2 == null)
				{
					return false;
				}
				return sGameData2.RechargeActiveInfo?.TreasurePrivilegeInfo.Active == EBoolState.True;
			}
		}
		else if (m_tab == EVipCardTab.CrossLadder && !zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.CrossLadder) && zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen())
		{
			return zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard();
		}
		return false;
	}

	private void OnTouch()
	{
		m_parent.m_needScroll = false;
		m_parent.m_targetTab = m_tab;
		m_parent.RefreshSelect();
	}
}
