using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossLadderMatchSuccessWnd : zbBaseWnd
{
	private Transform[] m_nodePlayers = (Transform[])(object)new Transform[3];

	private SpriteRenderer[] m_spriteFootholds = (SpriteRenderer[])(object)new SpriteRenderer[3];

	private SkeletonAnimation[] m_spineTaizis = (SkeletonAnimation[])(object)new SkeletonAnimation[3];

	private zbPlayerAvatar[] m_spineAvatars = new zbPlayerAvatar[3];

	private VisualElement[] m_nodes = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_spriteLadders = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[,] m_spriteStars = new VisualElement[3, 5];

	private Label[] m_lbNames = (Label[])(object)new Label[3];

	private Label[] m_lbPowers = (Label[])(object)new Label[3];

	private Button[] m_btnTouchs = (Button[])(object)new Button[3];

	private CrossLadderPlayerInfo[] m_list;

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < 3; i++)
		{
			m_nodePlayers[i] = ((Component)m_GameObjectSceneRoot.transform.Find("node" + i)).GetComponent<Transform>();
			m_spriteFootholds[i] = ((Component)((Component)m_nodePlayers[i]).transform.Find("foothold")).GetComponent<SpriteRenderer>();
			m_spineTaizis[i] = ((Component)((Component)m_nodePlayers[i]).transform.Find("taizi")).GetComponent<SkeletonAnimation>();
			m_spineAvatars[i] = ((Component)((Component)m_spriteFootholds[i]).transform.Find("avatar")).GetComponent<zbPlayerAvatar>();
			m_nodes[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "node" + i, (string)null);
			m_spriteLadders[i] = UQueryExtensions.Q<VisualElement>(m_nodes[i], "bg_ladder", (string)null);
			for (int j = 0; j < 5; j++)
			{
				m_spriteStars[i, j] = UQueryExtensions.Q<VisualElement>(m_spriteLadders[i], "star" + j, (string)null);
			}
			m_lbNames[i] = UQueryExtensions.Q<Label>(m_nodes[i], "lb_name", (string)null);
			m_lbPowers[i] = UQueryExtensions.Q<Label>(m_nodes[i], "lb_power", (string)null);
			m_btnTouchs[i] = UQueryExtensions.Q<Button>(m_nodes[i], "btn_touch", (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchs[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnTouchs(evt, tempIndex);
			}, "", (TrickleDown)0);
		}
		m_gameEevent.RegisterEvent(EGameEvent.Event_CommFight_Ack, OnCommFightAck);
	}

	public void RefreshCustom(CrossLadderPlayerInfo[] list)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_CrossLadder_2);
		zbGuide.Instance.StopCurGuideStep();
		m_list = list;
		for (int i = 0; i < 3; i++)
		{
			((Component)m_nodePlayers[i]).gameObject.SetActive(false);
			((Component)m_spriteFootholds[i]).gameObject.SetActive(false);
			m_nodes[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		((MonoBehaviour)this).StartCoroutine(ChuchangSequence());
	}

	private IEnumerator ChuchangSequence()
	{
		zbUIMgr.Instance.ShowModel();
		for (int i = 0; i < 3; i++)
		{
			if (i < m_list.Length)
			{
				RefreshItem(i);
				((MonoBehaviour)this).StartCoroutine(FadeIn(i));
				yield return (object)new WaitForSecondsRealtime(0.1f);
			}
		}
		yield return (object)new WaitForSecondsRealtime(0.5f);
		zbUIMgr.Instance.HideModel();
	}

	private IEnumerator FadeIn(int index)
	{
		((Component)m_nodePlayers[index]).gameObject.SetActive(true);
		m_spineTaizis[index].AnimationState.ClearTrack(0);
		m_spineTaizis[index].AnimationState.SetAnimation(0, "enter", false);
		m_spineTaizis[index].AnimationState.AddAnimation(0, "loop", true, 0f);
		yield return (object)new WaitForSecondsRealtime(1f / 3f);
		((Component)m_spriteFootholds[index]).gameObject.SetActive(true);
		m_nodes[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private void RefreshItem(int index)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		CrossLadderPlayerInfo crossLadderPlayerInfo = m_list[index];
		if (crossLadderPlayerInfo == null)
		{
			return;
		}
		zbLadderCsvRecord recordByKey = zbDataConfig.Instance.ladderCsvTable.GetRecordByKey(crossLadderPlayerInfo.AchieveInfo.LadderId);
		if (recordByKey != null)
		{
			m_spriteLadders[index].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
			for (int i = 0; i < 5; i++)
			{
				m_spriteStars[index, i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((recordByKey.id == zbSelf.Instance.crossLadderHandler.maxLadderId) ? 1 : 0));
				if (recordByKey.id != zbSelf.Instance.crossLadderHandler.maxLadderId)
				{
					if (i < recordByKey.star)
					{
						m_spriteStars[index, i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_star1.png"));
					}
					else
					{
						m_spriteStars[index, i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossLadder/crossLadder_star0.png"));
					}
				}
			}
		}
		zbFootholdCsvRecord recordByKey2 = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(crossLadderPlayerInfo.RoleBaseInfo.FootholdId);
		if (recordByKey2 != null)
		{
			m_spriteFootholds[index].sprite = zbAssetLoader.Instance.LoadSpriteAsset(recordByKey2.icon);
		}
		m_spineAvatars[index].RefreshCustom(crossLadderPlayerInfo.RoleBaseInfo.AvatarId, crossLadderPlayerInfo.RoleBaseInfo.GunId);
		((TextElement)m_lbNames[index]).text = crossLadderPlayerInfo.RoleBaseInfo.RoleName;
		((TextElement)m_lbPowers[index]).text = zbUtil.ConvertToTBMK(crossLadderPlayerInfo.RoleBaseInfo.TeamAttack);
	}

	private void OnTouchs(ClickEvent evt, int index)
	{
		CrossLadderPlayerInfo crossLadderPlayerInfo = m_list[index];
		if (crossLadderPlayerInfo != null)
		{
			zbUIMgr.Instance.ShowArenaDetail(EBattleFuncType.CrossLadderMatch, crossLadderPlayerInfo.RoleBaseInfo, null, crossLadderPlayerInfo.AchieveInfo);
		}
	}

	private void OnCommFightAck(object obj)
	{
		Hide();
	}
}
