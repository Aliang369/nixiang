using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldItemEvent : zbViewBaseItem
{
	private class BlinkTask
	{
		public float originalAlpha;

		public float elapsed;

		public float duration;

		public float freq;
	}

	protected zbWorldWnd m_wndParent;

	public WorldMapGridDetail gridDetail;

	private WorldEventDetail m_eventData;

	private VisualElement m_spriteEvent;

	public zbWorldItemEvent(zbWorldWnd wndParent, WorldMapGridDetail data)
	{
		m_wndParent = wndParent;
		gridDetail = data;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/world/tb_worldEvent.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteEvent = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_event", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		zbUtil.SetPickingModeIgnoreRecursive((VisualElement)(object)this);
	}

	public void Refresh()
	{
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		gridDetail = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(gridDetail.GridId);
		m_eventData = zbSelf.Instance.worldHandler.GetWorldEventDetail(gridDetail.GridId);
		WorldReconDetail worldReconDetailByGridId = zbSelf.Instance.worldHandler.GetWorldReconDetailByGridId(gridDetail.GridId);
		if (gridDetail.State == EWorldMapGridState.Explored && m_eventData != null && worldReconDetailByGridId == null)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (m_eventData.EventType == EWorldEventType.AwardBox)
			{
				zbWorldEventBoxCsvRecord recordByKey = zbDataConfig.Instance.worldEventBoxCsvTable.GetRecordByKey(m_eventData.EventId);
				if (recordByKey != null)
				{
					m_spriteEvent.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/world/world_event{recordByKey.quality}.png"));
				}
				else
				{
					((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
			}
			else if (m_eventData.EventType == EWorldEventType.Fight)
			{
				zbWorldEventFightCsvRecord recordByKey2 = zbDataConfig.Instance.worldEventFightCsvTable.GetRecordByKey(m_eventData.EventId);
				if (recordByKey2 != null)
				{
					m_spriteEvent.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/world/world_event{recordByKey2.quality}.png"));
				}
				else
				{
					((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
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
			if (m_eventData.EventType == EWorldEventType.AwardBox)
			{
				zbUIMgr.Instance.ShowWorldEventBox(m_eventData);
			}
			else if (m_eventData.EventType == EWorldEventType.Fight)
			{
				zbUIMgr.Instance.ShowWorldEventFight(m_eventData);
			}
		}
	}
}
