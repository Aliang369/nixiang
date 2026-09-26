using System;
using System.IO;
using Newtonsoft.Json;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCombatTestWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnFight;

	private BattleInfo dataInfo;

	private BattleResultInfo dataResult;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "button_fight", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action<ClickEvent>)OnFight, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom()
	{
		string? directoryName = Path.GetDirectoryName(Application.dataPath);
		string text = Path.Combine(directoryName, "Logs/combat_BattleInfo.json");
		if (File.Exists(text))
		{
			string text2 = File.ReadAllText(text);
			dataInfo = JsonConvert.DeserializeObject<BattleInfo>(text2);
		}
		else
		{
			Debug.LogError((object)("找不到 combat_BattleInfo.json 文件: " + text));
		}
		text = Path.Combine(directoryName, "Logs/combat_BattleSettle.json");
		if (File.Exists(text))
		{
			string text3 = File.ReadAllText(text);
			dataResult = JsonConvert.DeserializeObject<BattleResultInfo>(text3);
		}
		else
		{
			Debug.LogError((object)("找不到 combat_BattleSettle.json 文件: " + text));
		}
	}

	public void OnFight(ClickEvent evt)
	{
		zbSelf.Instance.combatHandler.StartShowCombat(dataInfo, 0, null, dataResult);
		Hide();
		DestroySelfWnd();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
		DestroySelfWnd();
	}
}
