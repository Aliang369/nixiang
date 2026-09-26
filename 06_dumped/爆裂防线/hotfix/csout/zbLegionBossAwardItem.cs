using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionBossAwardItem : zbViewBaseItem
{
	private Label m_lbName;

	private Button m_btnReceive;

	private zbItem[] m_items = new zbItem[3];

	private VisualElement m_iconIsReceive;

	private Label m_lbNoPass;

	private zbLegionBossAwardWnd.BossChallengeInfo m_detail;

	public zbLegionBossAwardItem(VisualTreeAsset tAsset, zbLegionBossAwardWnd parent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(parent);
		}
		m_btnReceive = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_receive", (string)null);
		m_iconIsReceive = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_isReceive", (string)null);
		m_lbNoPass = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_noPass", (string)null);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReceive, (Action)OnReceive, "", (TrickleDown)0);
	}

	public void Init(zbLegionBossAwardWnd.BossChallengeInfo detail)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		m_detail = detail;
		((TextElement)m_lbName).text = m_detail.name;
		for (int i = 0; i < 3; i++)
		{
			bool flag = i < m_detail.awardList.Length;
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				m_items[i].Refresh(new zbItemData((EResourceType)m_detail.awardList[i][0], m_detail.awardList[i][1], m_detail.awardList[i][2]));
			}
		}
		LegionBossInfo legionBossInfo = zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo;
		bool flag2 = legionBossInfo.BossId > m_detail.id;
		bool flag3 = legionBossInfo.BossId == m_detail.id;
		bool flag4 = legionBossInfo.PassState == EBoolState.True;
		bool flag5 = zbServerData.sGameData.LegionSelfInfo.BossInfo.PassAward.Contains(m_detail.id);
		((VisualElement)m_btnReceive).visible = false;
		m_iconIsReceive.visible = false;
		((VisualElement)m_lbNoPass).visible = false;
		if ((flag3 & flag4 & flag5) || (flag2 & flag5))
		{
			m_iconIsReceive.visible = true;
		}
		else if (((flag3 & flag4) && !flag5) || (flag2 && !flag5))
		{
			((VisualElement)m_btnReceive).visible = true;
		}
		else
		{
			((VisualElement)m_lbNoPass).visible = true;
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void OnReceive()
	{
		zbWsMsgHandler.Instance.LegionBoss_PassAward_Req(m_detail.id);
	}
}
