using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonAirdropDetailWnd : zbBaseWnd
{
	private Label m_lbName;

	private VisualElement m_spriteForce;

	private Label m_lbTeamCount;

	private VisualElement m_progress;

	private Label m_lbProgress;

	private Label m_lbAwardTitle;

	private zbItem[] m_awards = new zbItem[6];

	private Label m_lbPlayerTitle;

	private VisualElement[] m_spriteHeads = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_spriteFrames = (VisualElement[])(object)new VisualElement[5];

	private Button[] m_btnPlayers = (Button[])(object)new Button[5];

	private Button m_btnRank;

	private Button m_btnGuard;

	private Button m_btnGoto;

	private Button m_btnTouch;

	private SeasonAirdropInfo m_data;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteForce = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_force", (string)null);
		m_lbTeamCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_teamCount", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_RootElement, "lb_progress", (string)null);
		m_lbAwardTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_awardTitle", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "award" + i, (string)null);
			m_awards[i].OnEnable(this);
		}
		m_lbPlayerTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_playerTitle", (string)null);
		for (int j = 0; j < 5; j++)
		{
			m_spriteHeads[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "head" + j, (string)null);
			m_spriteFrames[j] = UQueryExtensions.Q<VisualElement>(m_spriteHeads[j], "frame", (string)null);
			m_btnPlayers[j] = UQueryExtensions.Q<Button>(m_spriteHeads[j], "btn_player", (string)null);
			int tempIndex = j;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPlayers[j], (Action<ClickEvent>)delegate
			{
				OnPlayers(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_btnGuard = UQueryExtensions.Q<Button>(m_RootElement, "btn_guard", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "btn_goto", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnRank, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGuard, (Action)OnGuard, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action)OnGoto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_SeasonAirdrop, OnEventUpdate);
	}

	public void RefreshCustom(SeasonAirdropInfo data)
	{
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		zbSeasonAirdropCsvRecord recordByKey = zbDataConfig.Instance.seasonAirdropCsvTable.GetRecordByKey((int)m_data.Quality);
		if (recordByKey != null)
		{
			((TextElement)m_lbName).text = zbUtil.Language(recordByKey.name);
			for (int i = 0; i < 6; i++)
			{
				bool flag = i < recordByKey.awardList.Length;
				((VisualElement)m_awards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
				if (flag)
				{
					zbItemData data2 = new zbItemData((EResourceType)recordByKey.awardList[i][0], recordByKey.awardList[i][1], recordByKey.awardList[i][2]);
					m_awards[i].Refresh(data2);
				}
			}
		}
		((TextElement)m_lbTeamCount).text = string.Format(zbUtil.Language("lang.11456"), m_data.Participants.Count);
		if (m_data.Participants.Count > 0)
		{
			m_spriteForce.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spriteForce.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/faction_flag" + (m_data.Participants[0].ForceId - 1).ToString() + ".png"));
		}
		else
		{
			m_spriteForce.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		float num = Mathf.Clamp((float)m_data.CurProg / (float)m_data.TotalProg, 0f, 1f);
		m_progress.style.width = StyleLength.op_Implicit(new Length(num * 100f, (LengthUnit)1));
		((TextElement)m_lbProgress).text = $"{num * 100f:0.##}%";
		for (int j = 0; j < 5; j++)
		{
			bool flag2 = j < m_data.Participants.Count;
			m_spriteHeads[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (flag2)
			{
				string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(m_data.Participants[j].RoleBaseInfo.HeadId);
				string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(m_data.Participants[j].RoleBaseInfo.HeadFrameId);
				m_spriteHeads[j].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex));
				m_spriteFrames[j].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex));
			}
		}
	}

	private void OnPlayers(int index)
	{
		if (index < m_data.Participants.Count)
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_data.Participants[index].RoleBaseInfo.RoleId);
		}
	}

	private void OnRank()
	{
	}

	private void OnGuard()
	{
	}

	private void OnGoto()
	{
		zbWsMsgHandler.Instance.Season_March_Start_Req(1, 4, m_data.PosX, m_data.PosY);
	}

	private void OnTouch()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
