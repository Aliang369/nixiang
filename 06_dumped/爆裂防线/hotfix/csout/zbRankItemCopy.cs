using System;
using System.Collections;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRankItemCopy : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbRankItemCopy>
	{
	}

	private Label m_lbRank;

	private VisualElement m_playerInfo;

	private Label m_lbProgress;

	private Label m_lbTime;

	private VisualElement m_bg;

	public SpineVisualElement m_spine;

	private Bone m_bone;

	private zbRankWnd m_wndParent;

	private RankDetail m_rankDetail;

	public zbRankItemCopy()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/rank/tb_rankCopy.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbRankWnd wndParent)
	{
		m_wndParent = wndParent;
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_progress", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "player", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine", (string)null);
		((VisualElement)m_spine).schedule.Execute((Action)delegate
		{
			if (m_spine.skeleton != null)
			{
				m_bone = m_spine.skeleton.FindBone("follow");
			}
		}).StartingIn(100L);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(RankDetail rankDetail)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		bool flag = rankDetail.RoleBaseInfo.RoleId == zbServerData.sGameData.RoleBaseInfo.RoleId;
		m_bg.style.backgroundImage = new StyleBackground(flag ? m_wndParent.m_rankBgMine : m_wndParent.m_rankBg);
		m_rankDetail = rankDetail;
		zbSelf.Instance.rankHandler.InitPlayerInfo(m_playerInfo, m_rankDetail.RoleBaseInfo);
		zbSelf.Instance.rankHandler.InitRank(m_lbRank, m_rankDetail.RankData.Rank);
		((TextElement)m_lbProgress).text = zbResourceHelper.GetMainLevelName((int)m_rankDetail.RankData.Value);
		((TextElement)m_lbTime).text = zbUtil.FormatUnixTimeToUtcString(m_rankDetail.RankData.RankTm);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public IEnumerator PlayAnimation(string animName)
	{
		TrackEntry val = m_spine.AnimationState.SetAnimation(0, animName, false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.Animation.Duration);
		}
	}

	public void Update()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (m_spine != null && m_spine.skeleton != null && m_bone != null)
		{
			Vector2 val = new Vector2(m_bone.WorldX, m_bone.WorldY);
			m_bg.transform.scale = new Vector3(m_bone.ScaleX, m_bone.ScaleY, 1f);
			m_bg.transform.rotation = Quaternion.Euler(0f, 0f, m_bone.Rotation);
			m_bg.transform.position = new Vector3(val.x, val.y, 0f);
		}
	}
}
