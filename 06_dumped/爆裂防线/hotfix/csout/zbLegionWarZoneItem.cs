using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarZoneItem : zbViewBaseItem
{
	private zbBaseWnd m_wndParent;

	private Label m_lbZoneNumber;

	private VisualElement m_selectBg;

	private Button btn_clickItem;

	private int currIndex;

	private zbLegionWarZonePanel parent;

	public zbLegionWarZoneItem(zbBaseWnd wndParent, zbLegionWarZonePanel _parent)
	{
		m_wndParent = wndParent;
		parent = _parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/tb_legionZonePullDown.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_selectBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "selectBg", (string)null);
		m_lbZoneNumber = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		btn_clickItem = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_clickItem", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)btn_clickItem, (Action)OnFollow, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int index)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		currIndex = index;
		((TextElement)m_lbZoneNumber).text = string.Format(zbUtil.Language("lang.10899"), index + 1);
		if (index % 2 == 1)
		{
			m_selectBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_selectBg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/pullDown_grid.jpg"));
		}
		else
		{
			m_selectBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (parent.currSettleZone == currIndex + 1)
		{
			m_selectBg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/pullDown_select.jpg"));
			m_selectBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}

	private void OnFollow()
	{
		if (m_wndParent.GetWndId() == EWndId.LegionWarSettle)
		{
			if (parent.currSettleZone != currIndex + 1)
			{
				if (((zbLegionWarSettleWnd)m_wndParent).m_currTab == 0)
				{
					zbWsMsgHandler.Instance.LegionWar_SettlePlayerInfo_Req(currIndex + 1);
				}
				else
				{
					zbWsMsgHandler.Instance.LegionWar_SettleLegionInfo_Req(currIndex + 1);
				}
			}
		}
		else
		{
			zbWsMsgHandler.Instance.LegionWar_SettlePlayerInfo_Req(currIndex + 1);
		}
	}
}
