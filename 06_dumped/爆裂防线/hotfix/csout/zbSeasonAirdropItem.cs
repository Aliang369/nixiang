using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonAirdropItem : zbViewBaseItem
{
	private zbSeasonAirdropListWnd m_wndParent;

	private VisualElement m_iconAirdrop;

	private VisualElement m_progress;

	private Label m_lbName;

	private VisualElement m_spriteForce;

	private Label m_lbTeamCount;

	private zbItem[] m_awards = new zbItem[6];

	private Button m_btnGoto;

	private SeasonAirdropInfo m_data;

	public zbSeasonAirdropItem(zbSeasonAirdropListWnd wndParent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/tb_seasonAirdrop.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_wndParent = wndParent;
		m_iconAirdrop = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_airdrop", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_spriteForce = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_force", (string)null);
		m_lbTeamCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_teamCount", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "award" + i, (string)null);
			m_awards[i].OnEnable(wndParent);
		}
		m_btnGoto = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_goto", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action)OnGoto, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(SeasonAirdropInfo seasonAirdropInfo)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (seasonAirdropInfo == null)
		{
			return;
		}
		m_data = seasonAirdropInfo;
		string resName = "ui/image/season/airdrop_award2.jpg";
		switch (m_data.Quality)
		{
		case ESeasonAirdropQuality.Blue:
			resName = "ui/image/season/airdrop_award2.jpg";
			break;
		case ESeasonAirdropQuality.Purple:
			resName = "ui/image/season/airdrop_award1.jpg";
			break;
		case ESeasonAirdropQuality.Gold:
			resName = "ui/image/season/airdrop_award0.jpg";
			break;
		}
		m_iconAirdrop.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
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
					zbItemData data = new zbItemData((EResourceType)recordByKey.awardList[i][0], recordByKey.awardList[i][1], recordByKey.awardList[i][2]);
					m_awards[i].Refresh(data);
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
	}

	private void OnGoto()
	{
		zbUIMgr.Instance.HideWnd(EWndId.SeasonAirdropClash);
		m_wndParent.Hide();
		zbWsMsgHandler.Instance.Season_Airdrop_Decrypt_Req(m_data.DropId);
	}
}
