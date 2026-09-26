using System;
using System.Collections;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarEffectWnd : zbBaseWnd
{
	private Button m_btnBack;

	private VisualElement m_veTitle;

	private Label m_lbTitle;

	private Label[] m_lbPlayerName = (Label[])(object)new Label[3];

	private SkeletonAnimation m_skeletonAnimation;

	private zbLegionWarZonePanel m_legionWarZone = new zbLegionWarZonePanel();

	private zbPlayerAvatar[] m_playerAvatar { get; set; } = new zbPlayerAvatar[3];

	protected override void OnEnable()
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_skeletonAnimation = ((Component)GetMyGameObjectSceneRoot().transform.Find("SkeletonAnimation")).GetComponent<SkeletonAnimation>();
		m_veTitle = UQueryExtensions.Q<VisualElement>(m_RootElement, "title", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_zone", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_lbPlayerName[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_playerName" + (i + 1), (string)null);
			m_playerAvatar[i] = ((Component)GetMyGameObjectSceneRoot().transform.Find("playerAvatar" + (i + 1))).GetComponent<zbPlayerAvatar>();
		}
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "legionZonePullDown", (string)null);
		m_legionWarZone.OnEnable(this, root);
		((VisualElement)m_legionWarZone.btn_clickItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(LegionWarPlayerRankDetail[] list)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		m_veTitle.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnBack).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		int num = 0;
		foreach (LegionWarPlayerRankDetail legionWarPlayerRankDetail in list)
		{
			if (num > 2)
			{
				break;
			}
			m_playerAvatar[num].RefreshCustom(legionWarPlayerRankDetail.RoleBaseInfo.AvatarId, legionWarPlayerRankDetail.RoleBaseInfo.GunId);
			m_playerAvatar[num].SetAnimation(0, "idle", loop: true);
			((TextElement)m_lbPlayerName[num]).text = legionWarPlayerRankDetail.RoleBaseInfo.RoleName;
			num++;
		}
		((MonoBehaviour)this).StartCoroutine(SpawnInit());
		m_legionWarZone.Refresh();
	}

	private IEnumerator SpawnInit()
	{
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		m_skeletonAnimation.AnimationState.SetAnimation(0, "loop", true);
		m_veTitle.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnBack).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private IEnumerator SpawnClose()
	{
		m_veTitle.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnBack).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "out", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		Hide();
	}

	private void OnClose()
	{
		((MonoBehaviour)this).StartCoroutine(SpawnClose());
	}
}
