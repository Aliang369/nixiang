using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldBossRankWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnBack;

	private ListView m_listView;

	private Label m_lbTitle;

	private VisualElement m_noListView;

	private VisualElement m_noBattleInfo;

	private VisualElement m_mine;

	private VisualElement m_awardPreview;

	private zbItem[] m_items = new zbItem[3];

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/worldBoss/tb_worldBossRankItem.uxml");
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_noBattleInfo = UQueryExtensions.Q<VisualElement>(m_RootElement, "nobattleInfo", (string)null);
		m_mine = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_awardPreview = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardPreview", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 110f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_awardPreview, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(RankDetail[] rankDetails)
	{
		InitMine(rankDetails);
		UpdateListView(rankDetails);
	}

	private void UpdateListView(RankDetail[] rankDetails)
	{
		bool flag = rankDetails.Length != 0;
		((VisualElement)m_listView).Clear();
		((VisualElement)m_listView).visible = flag;
		m_noListView.visible = !flag;
		m_listView.makeItem = () => (VisualElement)(object)new zbWorldBossRankItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbWorldBossRankItem).Init(rankDetails[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		rankDetails = ((rankDetails.Length > 20) ? rankDetails.Take(20).ToArray() : rankDetails);
		((BaseVerticalCollectionView)m_listView).itemsSource = rankDetails;
	}

	private void InitMine(RankDetail[] rankDetails)
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		RankDetail selfRankInfo = rankDetails.FirstOrDefault((RankDetail r) => r.RoleBaseInfo.RoleId == zbServerData.sGameData.RoleBaseInfo.RoleId);
		bool flag = selfRankInfo != null;
		bool flag2 = flag && selfRankInfo.RankData.Rank <= 20;
		RoleBaseInfo roleBaseInfo = zbServerData.sGameData.RoleBaseInfo;
		string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(roleBaseInfo.HeadId).icon;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
		string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(roleBaseInfo.HeadFrameId).icon;
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
		UQueryExtensions.Q<VisualElement>(m_mine, "head", (string)null).style.backgroundImage = new StyleBackground(val);
		UQueryExtensions.Q<VisualElement>(m_mine, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		((TextElement)UQueryExtensions.Q<Label>(m_mine, "lb_level", (string)null)).text = roleBaseInfo.Level.ToString();
		((TextElement)UQueryExtensions.Q<Label>(m_mine, "lb_name", (string)null)).text = roleBaseInfo.RoleName;
		((TextElement)UQueryExtensions.Q<Label>(m_mine, "lb_power", (string)null)).text = zbUtil.ConvertToTBMK(flag ? selfRankInfo.RankData.Value : 0);
		((TextElement)UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null)).text = (flag ? selfRankInfo.RankData.Rank.ToString() : "0");
		m_noBattleInfo.visible = !flag;
		m_awardPreview.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(flag & flag2)));
		((TextElement)UQueryExtensions.Q<Label>(m_noBattleInfo, "lb_text", (string)null)).text = ((!flag2 & flag) ? zbUtil.Language("lang.10884") : zbUtil.Language("lang.10883"));
		if (!(flag & flag2))
		{
			return;
		}
		zbWorldBossRankAwardCsvRecord zbWorldBossRankAwardCsvRecord2 = zbDataConfig.Instance.worldBossRankAwardCsvTable.GetRecords().records.Where((zbWorldBossRankAwardCsvRecord r) => r.bossid == zbSelf.Instance.worldBossHandler.worldBossInfo.BossId).ToArray().FirstOrDefault((zbWorldBossRankAwardCsvRecord r) => r.rank >= selfRankInfo.RankData.Rank);
		for (int num = 0; num < 3; num++)
		{
			bool flag3 = num < zbWorldBossRankAwardCsvRecord2.awardList.Length;
			((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
			if (flag3)
			{
				zbItemData data = new zbItemData((EResourceType)zbWorldBossRankAwardCsvRecord2.awardList[num][0], zbWorldBossRankAwardCsvRecord2.awardList[num][1], zbWorldBossRankAwardCsvRecord2.awardList[num][2]);
				m_items[num].Refresh(data);
			}
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
