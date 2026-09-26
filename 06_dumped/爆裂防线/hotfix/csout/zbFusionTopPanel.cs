using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFusionTopPanel
{
	private zbBaseWnd m_wndParent;

	private Label m_lbTime;

	private VisualElement m_nodeDailyAward;

	private Label m_lbDailyBestScore;

	private VisualElement m_progress;

	private VisualElement[] m_targets = (VisualElement[])(object)new VisualElement[4];

	private zbItem[] m_itemTargets = new zbItem[4];

	private Label[] m_lbTargets = (Label[])(object)new Label[4];

	private Label m_lbScore;

	private Label m_lbHistoryScore;

	private FusionInfo m_fusionInfo;

	public void OnEnable(zbBaseWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_lbTime = UQueryExtensions.Q<Label>(root, "lb_time", (string)null);
		m_nodeDailyAward = UQueryExtensions.Q<VisualElement>(root, "node_dailyAward", (string)null);
		m_lbDailyBestScore = UQueryExtensions.Q<Label>(m_nodeDailyAward, "lb_dailyBestScore", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_nodeDailyAward, "progress", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_targets[i] = UQueryExtensions.Q<VisualElement>(m_nodeDailyAward, $"target{i}", (string)null);
			m_itemTargets[i] = UQueryExtensions.Q<zbItem>(m_targets[i], "itemTarget", (string)null);
			m_itemTargets[i].OnEnable(m_wndParent);
			m_lbTargets[i] = UQueryExtensions.Q<Label>(m_targets[i], "lb_target", (string)null);
		}
		m_lbScore = UQueryExtensions.Q<Label>(root, "lb_score", (string)null);
		m_lbHistoryScore = UQueryExtensions.Q<Label>(root, "lb_historyScore", (string)null);
	}

	public void Refresh(FusionInfo fusionInfo = null)
	{
		if (fusionInfo == null)
		{
			m_fusionInfo = zbServerData.sGameData?.OperateInfo?.FusionInfo;
		}
		else
		{
			m_fusionInfo = fusionInfo;
		}
		if (m_fusionInfo != null)
		{
			((TextElement)m_lbScore).text = m_fusionInfo.FusionBase.Score.ToString();
			((TextElement)m_lbHistoryScore).text = m_fusionInfo.FusionBase.HistoryScore.ToString();
			RefreshDailyAward();
			RefresLeftTime();
		}
	}

	private void RefreshDailyAward()
	{
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbDailyBestScore).text = m_fusionInfo.FusionBase.DailyBestScore.ToString();
		float num = m_fusionInfo.FusionBase.DailyAwardList.Count;
		RepeatedField<OperateTargetDetail> dailyAwardList = m_fusionInfo.FusionBase.DailyAwardList;
		for (int i = 0; i < 4; i++)
		{
			bool flag = (float)i < num;
			m_targets[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (!flag)
			{
				continue;
			}
			float num2 = (float)(i + 1) / num * 508f - 66.666664f;
			m_targets[i].style.left = StyleLength.op_Implicit(num2);
			zbItemData data = new zbItemData(dailyAwardList[i].AwardList[0].Type, dailyAwardList[i].AwardList[0].Tid, dailyAwardList[i].AwardList[0].Count);
			m_itemTargets[i].Refresh(data);
			bool flag2 = dailyAwardList[i].State == ECommonState.Yes || (dailyAwardList[i].State == ECommonState.No && m_fusionInfo.FusionBase.DailyBestScore >= dailyAwardList[i].Target);
			bool gettedVisible = dailyAwardList[i].State == ECommonState.End;
			m_itemTargets[i].SetSpineVisible(flag2);
			int tempTarget = dailyAwardList[i].Target;
			if (flag2)
			{
				m_itemTargets[i].SetTouchCallback(delegate
				{
					zbWsMsgHandler.Instance.Operate_FusionDailyAward_Req(tempTarget);
				});
			}
			else
			{
				m_itemTargets[i].SetTouchCallback(null);
			}
			m_itemTargets[i].SetGettedVisible(gettedVisible);
			((TextElement)m_lbTargets[i]).text = m_fusionInfo.FusionBase.DailyAwardList[i].Target.ToString();
		}
		m_progress.style.width = StyleLength.op_Implicit(new Length(GetProgress(((IEnumerable<OperateTargetDetail>)dailyAwardList).ToArray()) * 508f, (LengthUnit)0));
	}

	private float GetProgress(OperateTargetDetail[] targetList)
	{
		if (targetList == null || targetList.Length == 0)
		{
			return 0f;
		}
		int num = targetList.Length;
		float num2 = 1f / (float)num;
		for (int i = 0; i < num; i++)
		{
			int target = targetList[i].Target;
			if (m_fusionInfo.FusionBase.DailyBestScore < target)
			{
				int num3 = ((i != 0) ? targetList[i - 1].Target : 0);
				float num4 = (float)(m_fusionInfo.FusionBase.DailyBestScore - num3) / (float)(target - num3);
				return Mathf.Clamp01((float)i * num2 + num4 * num2);
			}
		}
		return 1f;
	}

	public void RefresLeftTime()
	{
		if (m_fusionInfo != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(m_fusionInfo.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}
}
