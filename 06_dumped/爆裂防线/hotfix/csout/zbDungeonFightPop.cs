using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbDungeonFightPop : zbBaseWnd
{
	private Label m_lbltitle;

	private Label m_lbldesc;

	private Button m_btnFight;

	private DungeonTriggerEventInfo m_triggerEventInfo;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbltitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbldesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_message", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(DungeonTriggerEventInfo info)
	{
		m_triggerEventInfo = info;
		string text = zbUtil.Language("lang.10311");
		string text2;
		string text3;
		if (info.EventType == EDungeonEventType.RiotFight)
		{
			text2 = zbUtil.Language("lang.10309");
			text3 = zbUtil.Language("lang.10312");
		}
		else if (info.EventType == EDungeonEventType.BossFight)
		{
			text2 = zbUtil.Language("lang.10310");
			text3 = zbUtil.Language("lang.10313");
		}
		else
		{
			text2 = zbUtil.Language("lang.10311");
			text3 = zbUtil.Language("lang.10314");
			if (info.EventId != 0)
			{
				zbDungeonEventCsvRecord recordByKey = zbDataConfig.Instance.dungeonEventCsvTable.GetRecordByKey(info.EventId);
				if (recordByKey != null)
				{
					text2 = recordByKey.name;
					text3 = recordByKey.desc;
				}
			}
		}
		((TextElement)m_lbltitle).text = text2;
		((TextElement)m_lbldesc).text = text3;
		((TextElement)m_btnFight).text = text;
	}

	private void OnFight()
	{
		Hide();
		zbSelf.Instance.combatHandler.StartShowCombat(zbDungeonMgr.Instance.dungeonHandler.triggerBattleInfo);
	}
}
