using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldPanelItemRes : zbViewBaseItem
{
	private zbWorldWnd m_wndParent;

	private VisualElement m_icon;

	private Label m_lbName;

	private zbItem m_item = new zbItem();

	private Label m_lbCount;

	private Label m_lbUnCreate;

	private Button m_btnGoto;

	private Button m_btnTouch;

	private WorldMapGridDetail m_gridDetail;

	public zbWorldPanelItemRes(zbWorldWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/world/tb_worldPanelRes.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_icon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_lbUnCreate = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_unCreate", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_goto", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action<ClickEvent>)OnGoto, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnGoto, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(WorldMapGridDetail gridDetail)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		m_gridDetail = gridDetail;
		zbWorldBuildingCsvRecord recordByKey = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(m_gridDetail.BuildingId);
		if (recordByKey != null)
		{
			m_icon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
			((TextElement)m_lbName).text = recordByKey.name;
			m_item.Refresh(new zbItemData((EResourceType)recordByKey.resOut[0], recordByKey.resOut[1], recordByKey.resOut[2]), isNameVis: false, isCountVis: false);
			((TextElement)m_lbCount).text = recordByKey.resOut[2] + "/H";
		}
		WorldBuildingResDetail worldBuildingResDetail = zbSelf.Instance.worldHandler.GetWorldBuildingResDetail(m_gridDetail.GridId);
		if (worldBuildingResDetail == null)
		{
			((VisualElement)m_item).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbUnCreate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbUnCreate).text = zbUtil.Language("lang.10485");
		}
		else if (worldBuildingResDetail.ResLevel == 0)
		{
			((VisualElement)m_item).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbUnCreate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbUnCreate).text = zbUtil.Language("lang.10486");
		}
		else
		{
			((VisualElement)m_item).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbUnCreate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void OnGoto(ClickEvent evt)
	{
		if (m_gridDetail != null)
		{
			m_wndParent.OnGotoGrid(m_gridDetail);
			m_wndParent.SetPanelVisible(vis: false);
		}
	}
}
