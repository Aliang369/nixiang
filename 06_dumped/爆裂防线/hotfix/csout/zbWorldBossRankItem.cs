using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldBossRankItem : zbViewBaseItem
{
	private VisualElement m_playerInfo;

	private Label m_lbName;

	private Label m_lbPower;

	private Label m_lbDmg;

	private Label m_lbTime;

	private Label m_lbPosition;

	private Label m_lbRank;

	private Button m_btnHead;

	private zbItem[] m_items = new zbItem[3];

	private RankDetail m_rankDetail;

	public zbWorldBossRankItem(VisualTreeAsset tAsset, zbWorldBossRankWnd parent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "playerInfo", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbPower = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_power", (string)null);
		m_lbDmg = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_dmg", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		m_lbPosition = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_position", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_head", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(parent);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnClickHead, "", (TrickleDown)0);
	}

	public void Init(RankDetail rankDetail)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		m_rankDetail = rankDetail;
		InitPlayerInfo();
		zbWorldBossRankAwardCsvRecord zbWorldBossRankAwardCsvRecord2 = zbDataConfig.Instance.worldBossRankAwardCsvTable.GetRecords().records.Where((zbWorldBossRankAwardCsvRecord r) => r.bossid == zbSelf.Instance.worldBossHandler.worldBossInfo.BossId).ToArray().FirstOrDefault((zbWorldBossRankAwardCsvRecord r) => r.rank >= m_rankDetail.RankData.Rank);
		((TextElement)m_lbRank).text = m_rankDetail.RankData.Rank.ToString();
		for (int num = 0; num < 3; num++)
		{
			bool flag = num < zbWorldBossRankAwardCsvRecord2.awardList.Length;
			((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData((EResourceType)zbWorldBossRankAwardCsvRecord2.awardList[num][0], zbWorldBossRankAwardCsvRecord2.awardList[num][1], zbWorldBossRankAwardCsvRecord2.awardList[num][2]);
				m_items[num].Refresh(data);
			}
		}
	}

	private void InitPlayerInfo()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = m_rankDetail.RoleBaseInfo.RoleName;
		((TextElement)m_lbPower).text = zbUtil.ConvertToTBMK(m_rankDetail.RankData.Value);
		string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(m_rankDetail.RoleBaseInfo.HeadId).icon;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "head", (string)null).style.backgroundImage = new StyleBackground(val);
		string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(m_rankDetail.RoleBaseInfo.HeadFrameId).icon;
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		((TextElement)UQueryExtensions.Q<Label>(m_playerInfo, "lb_level", (string)null)).text = m_rankDetail.RoleBaseInfo.Level.ToString();
	}

	private void OnClickHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_rankDetail.RoleBaseInfo.RoleId);
	}
}
