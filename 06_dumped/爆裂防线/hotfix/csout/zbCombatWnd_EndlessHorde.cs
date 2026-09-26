using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCombatWnd_EndlessHorde
{
	private zbCombatWnd m_wndParent;

	private VisualElement m_RootElement;

	private GameObject m_GameObject;

	[NonSerialized]
	public SkeletonAnimation m_CelebrateAnim;

	private zbPieProgressTex m_celebrateProgress;

	private zbPieProgressTex m_celebrateProgress1;

	private Button m_btnCelebrate;

	public SpineVisualElement spineCelebrateProgress;

	private VisualElement m_rootLandmark;

	private VisualElement m_landmark;

	private Label m_lbLandmark1;

	private Label m_lbLandmark2;

	private GameObject m_rootObjectLandmark;

	private GameObject m_objectLandmark;

	private GameObject[] m_objectRankLine = (GameObject[])(object)new GameObject[3];

	private VisualElement[] m_rankLine = (VisualElement[])(object)new VisualElement[3];

	private Label[] m_lbRankLine1 = (Label[])(object)new Label[3];

	private Label[] m_lbRankLine2 = (Label[])(object)new Label[3];

	private VisualElement[] m_rankLineHead = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_rankLineFrame = (VisualElement[])(object)new VisualElement[3];

	private Label[] m_lbRankLineLevel = (Label[])(object)new Label[3];

	public SpineVisualElement m_pingziAnimElement;

	private StHeadData[] headDatas = new StHeadData[3];

	private float againCaclResult;

	public void OnEnable(zbCombatWnd wndParent, VisualElement root, GameObject _gameObject)
	{
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_GameObject = _gameObject;
		m_CelebrateAnim = ((Component)m_GameObject.transform.Find("CombatDataRoot").Find("RootHero").Find("CelebratAnimation")).gameObject.GetComponent<SkeletonAnimation>();
		m_rootLandmark = UQueryExtensions.Q<VisualElement>(m_RootElement, "barLandmark", (string)null);
		m_landmark = UQueryExtensions.Q<VisualElement>(m_RootElement, "landmarkLine", (string)null);
		m_lbLandmark1 = UQueryExtensions.Q<Label>(m_landmark, "lbg_km1", (string)null);
		m_lbLandmark2 = UQueryExtensions.Q<Label>(m_landmark, "lbg_km2", (string)null);
		m_rootObjectLandmark = ((Component)m_GameObject.transform.Find("CombatDataRoot").Find("endLessScene")).gameObject;
		m_objectLandmark = ((Component)m_rootObjectLandmark.transform.Find("landmarkLine")).gameObject;
		for (int i = 0; i < 3; i++)
		{
			m_rankLine[i] = UQueryExtensions.Q<VisualElement>(m_rootLandmark, "rankLine" + (i + 1), (string)null);
			m_lbRankLine1[i] = UQueryExtensions.Q<Label>(m_rankLine[i], "lbg_km1", (string)null);
			m_lbRankLine2[i] = UQueryExtensions.Q<Label>(m_rankLine[i], "lbg_km2", (string)null);
			m_rankLineHead[i] = UQueryExtensions.Q<VisualElement>(m_rankLine[i], "head", (string)null);
			m_rankLineFrame[i] = UQueryExtensions.Q<VisualElement>(m_rankLine[i], "frame", (string)null);
			m_lbRankLineLevel[i] = UQueryExtensions.Q<Label>(m_rankLine[i], "lb_level", (string)null);
			m_rankLine[i].transform.position = new Vector3(0f, -2000f, 0f);
			m_rankLine[i].style.top = StyleLength.op_Implicit(-37f);
			m_objectRankLine[i] = ((Component)m_GameObject.transform.Find("CombatDataRoot").Find("endLessScene").Find("rankLine" + (i + 1))).gameObject;
			m_objectRankLine[i].transform.position = new Vector3(0f, 20f, 0f);
		}
		m_pingziAnimElement = UQueryExtensions.Q<SpineVisualElement>(m_rootLandmark, "spine_pingzi", (string)null);
		m_celebrateProgress = UQueryExtensions.Q<zbPieProgressTex>(m_RootElement, "celebrateProgress", (string)null);
		m_celebrateProgress1 = UQueryExtensions.Q<zbPieProgressTex>(m_RootElement, "celebrateProgress1", (string)null);
		m_btnCelebrate = UQueryExtensions.Q<Button>(m_RootElement, "btn_N", (string)null);
		spineCelebrateProgress = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_Celebrate", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCelebrate, (Action)OnCelebrate, "", (TrickleDown)0);
	}

	public void StartBattle()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.EndlessHorde)
		{
			zbWsMsgHandler.Instance.Rank_List_Req(ERankType.EndlessHorde);
			((VisualElement)m_btnCelebrate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_rootLandmark.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_rootObjectLandmark.gameObject.SetActive(true);
			RefreshFightForwardEndlessHorde();
		}
		else
		{
			((VisualElement)m_btnCelebrate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_rootLandmark.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_rootObjectLandmark.gameObject.SetActive(false);
		}
		((Component)m_CelebrateAnim).gameObject.SetActive(false);
	}

	public void RefreshFightForwardEndlessHorde(object data = null)
	{
		int distance = zbSelf.Instance.endlessHordeHandler.GetDistance(zbServerData.sGameData.EndlessHordeInfo.Level + 1);
		((TextElement)m_lbLandmark1).text = ((float)distance / 1000f).ToString("0.#");
		(zbEndlessHordeLevelCsvRecord curRecord, zbEndlessHordeLevelCsvRecord maxRecord, int loopNum) record = zbSelf.Instance.endlessHordeHandler.GetRecord(zbServerData.sGameData.EndlessHordeInfo.Level + 1);
		var (zbEndlessHordeLevelCsvRecord2, zbEndlessHordeLevelCsvRecord3, _) = record;
		if (record.loopNum == 0)
		{
			((TextElement)m_lbLandmark2).text = "x" + zbEndlessHordeLevelCsvRecord2.passAward[0][2];
		}
		else
		{
			((TextElement)m_lbLandmark2).text = "x" + zbEndlessHordeLevelCsvRecord3.passAward[0][2];
		}
	}

	public void UpdateLandmark()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.GetBattleType() != EBattleFuncType.EndlessHorde)
		{
			return;
		}
		float num = (float)(zbCombatSystem.Instance.endlessHordeNextDistance - zbCombatSystem.Instance.endlessHordeCurrDistance) - zbCombatSystem.Instance.scrollDistance + m_wndParent.combatData.defense.GetAlertPositionY() * 100f;
		m_landmark.transform.position = new Vector3(0f, 0f - num, 0f);
		m_objectLandmark.transform.position = new Vector3(0f, num / 100f, 99f);
		float num2 = Math.Max(zbCombatSystem.Instance.celebratePreA * (float)(zbCombatSystem.Instance.celebratePreTime + 1) - zbCombatSystem.Instance.scrollTotalDistance + m_wndParent.combatData.defense.GetAlertPositionY() * 100f, m_wndParent.combatData.defense.GetAlertPositionY() * 100f);
		((VisualElement)m_pingziAnimElement).transform.position = new Vector3(0f, 0f - num2, 0f);
		bool flag = false;
		float num3 = (float)zbCombatSystem.Instance.endlessHordeCurrDistance + zbCombatSystem.Instance.scrollDistance;
		float num4 = m_wndParent.combatData.defense.GetAlertPositionY() * 100f;
		Vector3 val = default(Vector3);
		Vector3 val2 = default(Vector3);
		for (int i = 0; i < 3; i++)
		{
			RankDetail rankDetail = null;
			if (i + 1 <= zbSelf.Instance.endlessHordeHandler.rankDetails.Count)
			{
				rankDetail = zbSelf.Instance.endlessHordeHandler.rankDetails[i];
			}
			if (rankDetail == null)
			{
				((Vector3)(ref val))._002Ector(0f, -2200f, 0f);
				if (m_rankLine[i].transform.position != val)
				{
					m_rankLine[i].transform.position = val;
				}
				SetPositionIfChanged(m_objectRankLine[i].transform, new Vector3(0f, -22f, 0f));
				continue;
			}
			float num5 = rankDetail.RankData.Value;
			num = num5 - num3 + num4;
			string text = (num5 / 1000f).ToString("0.#");
			if (((TextElement)m_lbRankLine1[i]).text != text)
			{
				((TextElement)m_lbRankLine1[i]).text = text;
			}
			((Vector3)(ref val2))._002Ector(0f, 0f - num, 0f);
			if (m_rankLine[i].transform.position != val2)
			{
				m_rankLine[i].transform.position = val2;
			}
			SetPositionIfChanged(m_objectRankLine[i].transform, new Vector3(0f, num / 100f, 99f));
			RoleBaseInfo roleBaseInfo = rankDetail.RoleBaseInfo;
			ref StHeadData reference = ref headDatas[i];
			if (reference.HeadId != roleBaseInfo.HeadId || reference.FrameId != roleBaseInfo.HeadFrameId || reference.Level != roleBaseInfo.Level)
			{
				zbUtil.InitPlayerInfo(m_rankLineHead[i], roleBaseInfo.HeadId, m_rankLineFrame[i], roleBaseInfo.HeadFrameId, m_lbRankLineLevel[i], roleBaseInfo.Level);
				reference.HeadId = roleBaseInfo.HeadId;
				reference.FrameId = roleBaseInfo.HeadFrameId;
				reference.Level = roleBaseInfo.Level;
			}
			if (num < -700f)
			{
				flag = true;
			}
		}
		if (flag)
		{
			RefreshRankDataEndlessHorde();
		}
		float progress = Math.Clamp(zbCombatSystem.Instance.celebrateDistance / zbCombatSystem.Instance.celebrateMaxDistance, 0f, 1f);
		m_celebrateProgress.progress = progress;
		m_celebrateProgress1.progress = progress;
		if (zbCombatSystem.Instance.m_isCelebrating)
		{
			if (((VisualElement)m_celebrateProgress1).style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				((VisualElement)m_celebrateProgress1).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
		}
		else if (((VisualElement)m_celebrateProgress1).style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1))
		{
			((VisualElement)m_celebrateProgress1).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (m_celebrateProgress.progress >= 1f)
		{
			TrackEntry current = spineCelebrateProgress.AnimationState.GetCurrent(0);
			if (current != null && current.Animation.Name != "fire")
			{
				spineCelebrateProgress.AnimationState.SetAnimation(0, "fire", true);
			}
		}
		else
		{
			TrackEntry current2 = spineCelebrateProgress.AnimationState.GetCurrent(0);
			if (current2 != null && current2.Animation.Name != "idle")
			{
				spineCelebrateProgress.AnimationState.SetAnimation(0, "idle", true);
			}
		}
		if (((float)zbCombatSystem.Instance.endlessHordeCurrDistance + zbCombatSystem.Instance.scrollDistance >= (float)zbCombatSystem.Instance.endlessHordeNextDistance && !zbCombatSystem.Instance.m_isFightForward) || againCaclResult >= 10f)
		{
			againCaclResult = 0f;
			zbCombatSystem.Instance.Pause();
			zbAudioMgr.Instance.PauseAllAudio();
			zbCombatSystem.Instance.m_isFightForward = true;
			BattleResultInfo battleResult = zbCombatSystem.Instance.combatResult.CalcFightResult();
			zbWsMsgHandler.Instance.EndlessHorde_FightForward_Req(zbServerData.sGameData.EndlessHordeInfo.Level + 1, battleResult, (int)((float)zbCombatSystem.Instance.endlessHordeCurrDistance + zbCombatSystem.Instance.scrollDistance));
		}
		if (zbCombatSystem.Instance.m_isFightForward)
		{
			againCaclResult += Time.unscaledDeltaTime;
		}
		else
		{
			againCaclResult = 0f;
		}
		if (zbCombatSystem.Instance.celebratePreA * (float)(zbCombatSystem.Instance.celebratePreTime + 1) - zbCombatSystem.Instance.scrollTotalDistance <= 0f && !zbCombatSystem.Instance.m_isPingziAward)
		{
			zbCombatSystem.Instance.m_isPingziAward = true;
			PlayPingzi();
		}
	}

	public void RefreshRankDataEndlessHorde(object data = null)
	{
		zbSelf.Instance.endlessHordeHandler.GetRankValueBySelf((float)zbCombatSystem.Instance.endlessHordeCurrDistance + zbCombatSystem.Instance.scrollDistance);
	}

	public void OnCelebrate()
	{
		if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.EndlessHorde && !zbCombatSystem.Instance.m_isCelebrating)
		{
			if ((Object)(object)m_wndParent.combatData.monsters.GetClosestBossMonster(10f, ignoreUnSelect: true) != (Object)null)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10803"));
			}
			else if (!(zbCombatSystem.Instance.celebrateDistance < zbCombatSystem.Instance.celebrateMaxDistance))
			{
				zbCombatSystem.Instance.m_isCelebrating = true;
				((Component)m_CelebrateAnim).gameObject.SetActive(true);
			}
		}
	}

	public void PlayPingzi()
	{
		((MonoBehaviour)m_wndParent).StartCoroutine(PlayPingziAnim());
	}

	public IEnumerator PlayPingziAnim()
	{
		Bone val = m_pingziAnimElement.skeleton.FindBone("shoot");
		var (num, num2) = zbUtil.GetNodePosition((VisualElement)(object)m_btnCelebrate);
		var (num3, num4) = zbUtil.GetNodePosition((VisualElement)(object)m_pingziAnimElement);
		val.X = (num - num3) / 100f;
		val.Y = (0f - (num2 - num4)) / 100f;
		TrackEntry val2 = m_pingziAnimElement.AnimationState.SetAnimation(0, "move", false);
		yield return (object)new WaitForSeconds(val2.AnimationEnd);
		zbCombatSystem.Instance.celebratePreTime++;
		zbCombatSystem.Instance.celebrateDistance = Math.Min(zbCombatSystem.Instance.celebrateDistance + zbCombatSystem.Instance.celebratePreB, zbCombatSystem.Instance.celebrateMaxDistance);
		zbCombatSystem.Instance.m_isPingziAward = false;
		m_pingziAnimElement.AnimationState.SetAnimation(0, "animation", true);
	}

	private void SetPositionIfChanged(Transform target, Vector3 newPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (target.position != newPos)
		{
			target.position = newPos;
		}
	}
}
