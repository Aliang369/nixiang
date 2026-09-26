using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbOpenServerRankItem : VisualElement
{
	private Label m_lbRank;

	private Label m_lbVal;

	private Label m_lbName;

	private Label m_lbTime;

	private Label m_lbMainlevlForRank;

	private VisualElement m_bg;

	private VisualElement m_root;

	private zbItem[] m_awards;

	private VisualElement m_nodeDedaily;

	private VisualElement m_btnDaily;

	private VisualElement m_notify;

	private RankDetail m_rankDetail;

	private VisualElement m_mask;

	private VisualElement m_panel;

	private Button m_btnDetail;

	private Button m_btnHead;

	private VisualElement m_icon;

	private Label m_lbCount;

	private zbOpenServerRankWnd m_parent;

	public VisualElement m_spinePlaceholder { get; private set; }

	public VisualElement m_spinePlaceholderTop { get; private set; }

	public zbOpenServerRankItem(VisualTreeAsset tAsset, zbOpenServerRankWnd parent)
	{
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		m_awards = new zbItem[4];
		((VisualElement)this)._002Ector();
		m_parent = parent;
		TemplateContainer val = tAsset.Instantiate();
		((VisualElement)this).Add((VisualElement)(object)val);
		m_root = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "root", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_rank", (string)null);
		m_lbVal = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_value", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_name", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_time", (string)null);
		((TextElement)m_lbTime).text = zbUtil.Language("lang.10519");
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "bg", (string)null);
		m_spinePlaceholder = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "spinePlaceholder", (string)null);
		m_spinePlaceholderTop = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "spinePlaceholderTop", (string)null);
		m_btnDetail = UQueryExtensions.Q<Button>((VisualElement)(object)val, "btn_detail", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)val, "btn_head", (string)null);
		m_mask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "detailPanelMask", (string)null);
		m_panel = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "panel", (string)null);
		m_nodeDedaily = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "nodedaily", (string)null);
		m_btnDaily = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "btndaily", (string)null);
		m_notify = zbUtil.CreateNotifyTag(m_btnDaily, (Vector2?)new Vector2(273f, 35f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_icon = UQueryExtensions.Q<VisualElement>(m_nodeDedaily, "diamond", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_icon, "lbldaily", (string)null);
		m_lbMainlevlForRank = UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_mainLevelForRank", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)val, "award" + i, (string)null);
			m_awards[i].OnEnable(parent);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)val);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDetail, (Action)OnDetail, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>(m_mask, (Action)OnCloseDetail, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnClickHead, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>(m_btnDaily, (Action)OnDaily, "", (TrickleDown)0);
		m_parent.m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_FreeGiftAward, OnUpdateEventFreeGiftAward);
	}

	private void OnClickHead()
	{
		if (m_rankDetail != null && m_rankDetail.RoleBaseInfo != null)
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_rankDetail.RoleBaseInfo.RoleId);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10499"));
		}
	}

	private void OnCloseDetail()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_mask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_panel.visible = false;
	}

	private void OnDetail()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (m_rankDetail == null || m_rankDetail.RoleBaseInfo == null)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10499"));
			return;
		}
		m_mask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_panel.visible = true;
		((TextElement)m_lbTime).text = zbUtil.Language(zbUtil.Language("lang.10519"));
	}

	public void Init(RankDetail rankDetail, int index, int rankLimit)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		m_rankDetail = rankDetail;
		int num = index + 1;
		bool flag = num == 1;
		m_nodeDedaily.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag || !zbSelf.Instance.rankHandler.HasOpenServerRankRedDot()) ? 1 : 0));
		if (flag && zbSelf.Instance.rankHandler.HasOpenServerRankRedDot())
		{
			zbFreeGiftCsvRecord recordByKey = zbDataConfig.Instance.freeGiftCsvTable.GetRecordByKey(3);
			string strIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)recordByKey.award[0], recordByKey.award[1]).strIconPath;
			m_icon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(strIconPath));
			((TextElement)m_lbCount).text = "X" + recordByKey.award[2];
		}
		((VisualElement)this).style.marginTop = StyleLength.op_Implicit(flag ? 210f : 0f);
		m_bg.style.alignSelf = StyleEnum<Align>.op_Implicit((Align)(flag ? 2 : ((num % 2 != 0) ? 1 : 3)));
		bool flag2 = rankDetail != null && rankDetail.RoleBaseInfo != null;
		((VisualElement)m_lbMainlevlForRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag2 ? 1 : 0));
		((VisualElement)m_lbName).visible = flag2;
		if (flag2)
		{
			((TextElement)m_lbName).text = rankDetail.RoleBaseInfo.RoleName;
			((TextElement)m_lbVal).text = zbResourceHelper.GetMainLevelName((int)rankDetail.RankData.Value);
			((TextElement)UQueryExtensions.Q<Label>(m_panel, "lb_timeVal", (string)null)).text = zbUtil.FormatUnixTimeToUtcString(rankDetail.RankData.RankTm);
		}
		else
		{
			((TextElement)m_lbMainlevlForRank).text = "上榜条件: 主线Day" + rankLimit;
			((TextElement)m_lbVal).text = zbUtil.Language(zbUtil.Language("lang.10499"));
		}
		zbSelf.Instance.rankHandler.InitRankInOpenServerRank(m_lbRank, num);
		int[][] awardList = zbDataConfig.Instance.opsMainQuestCsvTable.GetRecordByKey(num).awardList;
		for (int i = 0; i < 4; i++)
		{
			bool flag3 = i < awardList.Length;
			((VisualElement)m_awards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
			if (flag3)
			{
				m_awards[i].Refresh(new zbItemData((EResourceType)awardList[i][0], awardList[i][1], awardList[i][2]));
			}
		}
	}

	private void OnDaily()
	{
		if (zbSelf.Instance.rankHandler.HasOpenServerRankRedDot())
		{
			zbWsMsgHandler.Instance.Aux_FreeGiftAward_Req(3);
		}
	}

	private void OnUpdateEventFreeGiftAward(object data)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_nodeDedaily.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}
}
