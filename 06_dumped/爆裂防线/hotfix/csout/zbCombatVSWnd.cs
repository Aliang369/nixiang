using System;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCombatVSWnd : zbBaseWnd
{
	[NonSerialized]
	public SkeletonAnimation m_skeletonAnimation;

	private VisualElement m_nodes_1;

	private Label m_lbNames_1;

	private Label m_lbPowers_1;

	private VisualElement m_nodes_2;

	private Label m_lbNames_2;

	private Label m_lbPowers_2;

	private float timeMe;

	private float timeAll = 3f;

	private BattleInfo battleInfo;

	private int paramId;

	private zbCombatRecorder restoreData;

	private BattleResultInfo dataResult;

	private zbPlayerAvatar m_spineAvatar1;

	private zbPlayerAvatar m_spineAvatar2;

	private bool showName;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_skeletonAnimation = ((Component)GetMyGameObjectSceneRoot().transform.Find("SkeletonAnimation")).GetComponent<SkeletonAnimation>();
		m_spineAvatar1 = ((Component)GetMyGameObjectSceneRoot().transform.Find("playerAvatar1")).GetComponent<zbPlayerAvatar>();
		m_spineAvatar2 = ((Component)GetMyGameObjectSceneRoot().transform.Find("playerAvatar2")).GetComponent<zbPlayerAvatar>();
		m_nodes_1 = UQueryExtensions.Q<VisualElement>(m_RootElement, "node1", (string)null);
		m_lbNames_1 = UQueryExtensions.Q<Label>(m_nodes_1, "lb_name", (string)null);
		m_lbPowers_1 = UQueryExtensions.Q<Label>(m_nodes_1, "lb_power", (string)null);
		m_nodes_2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "node2", (string)null);
		m_lbNames_2 = UQueryExtensions.Q<Label>(m_nodes_2, "lb_name", (string)null);
		m_lbPowers_2 = UQueryExtensions.Q<Label>(m_nodes_2, "lb_power", (string)null);
	}

	public void StartBattle(BattleInfo _battleInfo, int _paramId, zbCombatRecorder _restoreData, BattleResultInfo _dataResult, RoleBaseInfo roleBaseInfo)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "enter", false);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_vs.ogg");
		battleInfo = _battleInfo;
		paramId = _paramId;
		restoreData = _restoreData;
		dataResult = _dataResult;
		m_nodes_1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodes_2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		showName = true;
		timeMe = 0f;
		timeAll = val.AnimationEnd;
		((TextElement)m_lbNames_1).text = roleBaseInfo.RoleName;
		((TextElement)m_lbPowers_1).text = zbUtil.ConvertToTBMK(roleBaseInfo.TeamAttack);
		((TextElement)m_lbNames_2).text = zbSelf.Instance.GetRoleName();
		((TextElement)m_lbPowers_2).text = zbUtil.ConvertToTBMK(zbServerData.sGameData.RoleBaseInfo.TeamAttack);
		m_spineAvatar1.RefreshCustom(roleBaseInfo.AvatarId, roleBaseInfo.GunId);
		m_spineAvatar2.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
	}

	protected override void UpdateAtShow()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		timeMe += Time.unscaledDeltaTime;
		if (timeMe > timeAll)
		{
			Hide();
			zbUIMgr.Instance.ShowCombat(battleInfo, paramId, restoreData, dataResult);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_CommFight_Ack);
			timeMe = 0f;
		}
		if (timeMe >= 2f / 3f && showName)
		{
			m_nodes_1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodes_2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbUIEffect.FadeTree(m_nodes_1, 0f, 1f, 1f / 6f);
			zbUIEffect.FadeTree(m_nodes_2, 0f, 1f, 1f / 6f);
			showName = false;
		}
	}
}
