using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldItemDungeon : zbViewBaseItem
{
	protected zbWorldWnd m_wndParent;

	private WorldEventDetail m_eventData;

	private zbWorldBuildingCsvRecord m_record;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbBuilding;

	private float m_cellWidth = 140f;

	private float m_cellHeight = 140f;

	public zbWorldItemDungeon(zbWorldWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/world/tb_worldDungeon.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbBuilding = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_building", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		zbUtil.SetPickingModeIgnoreRecursive((VisualElement)(object)this);
	}

	public void Refresh()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		m_eventData = zbSelf.Instance.worldHandler.GetDungeonEventDetail();
		if (m_eventData == null)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		WorldMapGridDetail worldMapGridDetailByGridId = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(m_eventData.GridId);
		if (worldMapGridDetailByGridId != null && worldMapGridDetailByGridId.State == EWorldMapGridState.Explored)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
			var (num, num2) = zbSelf.Instance.worldHandler.GetColAndRow(worldMapGridDetailByGridId);
			((VisualElement)this).style.left = StyleLength.op_Implicit((float)num * m_cellWidth - m_cellWidth);
			((VisualElement)this).style.top = StyleLength.op_Implicit((float)num2 * m_cellHeight - m_cellHeight);
			m_record = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(m_eventData.EventId);
			if (m_record != null)
			{
				m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.icon));
				((TextElement)m_lbBuilding).text = m_record.name;
			}
			if (m_wndParent.IsRadarNewEvent(m_eventData.GridId))
			{
				m_wndParent.RemoveRadarEvent(m_eventData.GridId);
				zbUIEffect.BlinkTimes((VisualElement)(object)this, 0f, 1f, 100);
			}
		}
		else
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	public void OnTouch()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!(((VisualElement)this).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1)) && m_eventData != null)
		{
			zbUIMgr.Instance.ShowWorldDungeon(m_eventData.GridId, m_eventData.EventId);
		}
	}
}
