using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionWarRankLegionItem : zbViewBaseItem
{
	private zbLegionWarRankWnd m_wndParent;

	private LegionWarLegionRankDetail m_detail;

	private VisualElement m_legionInfo;

	private Button m_btnHead;

	private Label m_lbPlayerPoint;

	private Label m_lbRank;

	public zbLegionWarRankLegionItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/tb_legionWarRankLegion.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbLegionWarRankWnd wndParent)
	{
		m_wndParent = wndParent;
		m_legionInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "legion", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_head", (string)null);
		m_lbPlayerPoint = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_playerPoint", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnLegion, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(LegionWarLegionRankDetail detail)
	{
		m_detail = detail;
		((TextElement)m_lbPlayerPoint).text = m_detail.RankData.Value.ToString();
		zbSelf.Instance.rankHandler.InitRank(m_lbRank, m_detail.RankData.Rank);
		zbSelf.Instance.legionWarHandler.InitLegionInfo(m_legionInfo, m_detail.LegionFlagInfo);
	}

	private void OnLegion()
	{
		zbWsMsgHandler.Instance.Legion_TipsInfo_Req(m_detail.LegionFlagInfo.LegionId);
	}
}
