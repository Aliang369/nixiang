using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEliteQuestThankLetterItem : zbViewBaseItem
{
	private HelpRecordDetail m_detail;

	private VisualElement m_playerInfo;

	private Label m_lbName;

	private Label m_lbProgress;

	private Label m_lbTime;

	private Button m_btnTouch;

	private Label m_lbTili;

	public zbEliteQuestThankLetterItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/tb_eliteQuestThankLetter.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "player", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_progress", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_lbTili = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_tili", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_get", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(HelpRecordDetail detail)
	{
		m_detail = detail;
		InitBtn();
		InitPlayerInfo(m_playerInfo, detail.RoleBaseInfo);
	}

	private void InitBtn()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		JToken obj = zbDataConfig.Instance.questBaseJsonTable.GetRecord()["elite"][(object)"helpAward"];
		JArray val = (JArray)(object)((obj is JArray) ? obj : null);
		string strIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)val[0]), Extensions.Value<int>((IEnumerable<JToken>)val[1])).strIconPath;
		((VisualElement)m_lbTili).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(strIconPath));
		((TextElement)m_lbTili).text = "X" + Extensions.Value<int>((IEnumerable<JToken>)val[2]);
	}

	private void InitPlayerInfo(VisualElement element, RoleBaseInfo roleBaseInfo)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = roleBaseInfo.RoleName;
		((TextElement)m_lbProgress).text = "Day" + m_detail.EliteLevel;
		((TextElement)m_lbTime).text = zbSelf.Instance.friendHandler.GetThankLetterShort(zbSelf.Instance.timeHandler.GetServerTime() - m_detail.RecordTm);
		zbPlayerHeadCsvRecord recordByKey = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(roleBaseInfo.HeadId);
		if (recordByKey != null)
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon);
			UQueryExtensions.Q<VisualElement>(element, "head", (string)null).style.backgroundImage = new StyleBackground(val);
		}
		zbPlayerHeadFrameCsvRecord recordByKey2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(roleBaseInfo.HeadFrameId);
		if (recordByKey2 != null)
		{
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon);
			UQueryExtensions.Q<VisualElement>(element, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		}
		((TextElement)UQueryExtensions.Q<Label>(element, "lb_level", (string)null)).text = roleBaseInfo.Level.ToString();
	}

	private void OnTouch()
	{
		zbWsMsgHandler.Instance.EliteQuest_HelpAward_Req(m_detail.RecordId);
	}
}
