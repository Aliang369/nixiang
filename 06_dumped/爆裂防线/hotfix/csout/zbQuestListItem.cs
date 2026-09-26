using System;
using System.Collections;
using System.Linq;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbQuestListItem : zbViewBaseItem
{
	private zbMainQuestListWnd m_wndParent;

	private Label m_lbDay;

	private Label m_lbDesc;

	private Button m_btn_why;

	private Button m_btnTouch;

	private Button[] m_boxButtons = (Button[])(object)new Button[3];

	private SpineVisualElement[] m_spines = (SpineVisualElement[])(object)new SpineVisualElement[3];

	private zbMainLevelCsvRecord mainLevelRecord;

	private zbEliteLevelCsvRecord eliteLevelRecord;

	private int m_curLevel;

	private VisualElement m_spriteProgress;

	private Label m_lbLevelHp;

	private Label m_lbNotEnabled;

	private VisualElement m_perfect;

	private VisualElement m_root;

	private Button m_btnChallengeAgain;

	private Sprite m_dayBg;

	private Sprite m_nightBg;

	private bool m_isReceivingReward;

	private bool m_IsMain;

	public zbQuestListItem(zbMainQuestListWnd parent, VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_wndParent = parent;
		m_lbDay = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_day", (string)null);
		m_btn_why = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_why", (string)null);
		m_root = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "root", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_lbLevelHp = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_levelHp", (string)null);
		m_perfect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "perfect", (string)null);
		m_btnChallengeAgain = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_challengeAgain", (string)null);
		m_lbNotEnabled = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_notEnabled", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_boxButtons[i] = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_box" + i, (string)null);
			m_spines[i] = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_boxButtons[i], "spine", (string)null);
			int currentIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_boxButtons[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnBoxButtons(evt, currentIndex + 1);
			}, "", (TrickleDown)0);
		}
		m_dayBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/mainquest/mainQuest_listBg.png");
		m_nightBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/mainquest/eliteQuest_listBg.png");
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btn_why, (Action<ClickEvent>)OnCurrentLevelDetails, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnCurrentLevelDetails, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnChallengeAgain, (Action<ClickEvent>)OnFight, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(int curLevel, bool isMain)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		int num = 1;
		m_isReceivingReward = false;
		m_IsMain = isMain;
		((TextElement)m_lbDesc).text = (isMain ? "Day" : "Night");
		m_root.style.backgroundImage = new StyleBackground(m_IsMain ? m_dayBg : m_nightBg);
		m_curLevel = curLevel + num;
		if (isMain)
		{
			mainLevelRecord = zbDataConfig.Instance.mainLevelCsvTable.GetRecordByKey(m_curLevel);
		}
		else
		{
			eliteLevelRecord = zbDataConfig.Instance.eliteLevelCsvTable.GetRecordByKey(m_curLevel);
		}
		RefreshQuestDetail();
		((TextElement)m_lbDay).text = m_curLevel.ToString();
	}

	private void OnCurrentLevelDetails(ClickEvent evt)
	{
		if (m_IsMain)
		{
			int[][] record = mainLevelRecord.baseAward.Concat(mainLevelRecord.growthAward).Concat(mainLevelRecord.spAward1).Concat(mainLevelRecord.spAward2)
				.Concat(mainLevelRecord.spAward3)
				.Concat(mainLevelRecord.spAward4)
				.Concat(mainLevelRecord.winAward)
				.ToArray();
			int[] source = new int[3] { mainLevelRecord.mst1, mainLevelRecord.mst2, mainLevelRecord.mst3 };
			zbUIMgr.Instance.ShowQuestView(record, source.Where((int id) => id != 0).Distinct().ToArray(), m_curLevel);
		}
		else
		{
			int[][] record2 = eliteLevelRecord.baseAward.Concat(eliteLevelRecord.growthAward).Concat(eliteLevelRecord.spAward1).Concat(eliteLevelRecord.spAward2)
				.Concat(eliteLevelRecord.spAward3)
				.Concat(eliteLevelRecord.spAward4)
				.Concat(eliteLevelRecord.winAward)
				.ToArray();
			int[] source2 = new int[3] { eliteLevelRecord.mst1, eliteLevelRecord.mst2, eliteLevelRecord.mst3 };
			zbUIMgr.Instance.ShowQuestView(record2, source2.Where((int id) => id != 0).Distinct().ToArray(), m_curLevel, isMain: false);
		}
	}

	private void OnBoxButtons(ClickEvent evt, int index)
	{
		if (m_isReceivingReward)
		{
			return;
		}
		if (m_IsMain)
		{
			if (zbSelf.Instance.mainQuestHandler.CanClaimReward(m_curLevel, index))
			{
				m_isReceivingReward = true;
				PlayReceiveRewardAnim();
			}
			else
			{
				zbUIMgr.Instance.ShowQuestPassAward(index, mainLevelRecord);
			}
		}
		else if (zbSelf.Instance.eliteQuestHandler.CanClaimReward(m_curLevel, index))
		{
			m_isReceivingReward = true;
			PlayReceiveRewardAnim();
		}
		else
		{
			zbUIMgr.Instance.ShowEliteQuestPassAward(index, eliteLevelRecord);
		}
	}

	private void PlayReceiveRewardAnim()
	{
		if (!(m_IsMain ? (m_curLevel < zbServerData.sGameData.MainQuestInfo.NextLevel) : (m_curLevel < zbServerData.sGameData.EliteQuestInfo.NextLevel)))
		{
			return;
		}
		MainQuestDetail obj = (m_IsMain ? zbSelf.Instance.mainQuestHandler.GetMainQuestDetail(m_curLevel) : zbSelf.Instance.eliteQuestHandler.GetEliteQuestDetail(m_curLevel));
		int score = (int)obj.Score;
		int scoreAward = (int)obj.ScoreAward;
		for (int i = 0; i < 3; i++)
		{
			int num = i + 1;
			if (num <= score && num > scoreAward)
			{
				m_spines[i].AnimationState.SetAnimation(0, "3", false);
			}
		}
		((MonoBehaviour)m_wndParent).StartCoroutine(DelayReciveReward());
	}

	private IEnumerator DelayReciveReward()
	{
		yield return (object)new WaitForSeconds(0.6f);
		if (m_IsMain)
		{
			zbWsMsgHandler.Instance.MainQuest_ScoreAward_Req(m_curLevel);
		}
		else
		{
			zbWsMsgHandler.Instance.EliteQuest_ScoreAward_Req(m_curLevel);
		}
	}

	private void RefreshQuestDetail()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		MainQuestDetail mainQuestDetail = (m_IsMain ? zbSelf.Instance.mainQuestHandler.GetMainQuestDetail(m_curLevel) : zbSelf.Instance.eliteQuestHandler.GetEliteQuestDetail(m_curLevel));
		bool flag = mainQuestDetail != null;
		((VisualElement)m_lbLevelHp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_perfect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_spriteProgress.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_lbNotEnabled).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		if (flag)
		{
			((VisualElement)m_btnChallengeAgain).visible = mainQuestDetail.DefensePct != 100f;
			float num = Mathf.Floor(mainQuestDetail.DefensePct);
			((TextElement)m_lbLevelHp).text = string.Format("{0}: {1}%", zbUtil.Language("lang.10342"), (num <= 0f && mainQuestDetail.DefensePct > 0f) ? 1 : ((int)num));
			((VisualElement)m_lbLevelHp).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(mainQuestDetail.DefensePct >= 100f));
			m_perfect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(mainQuestDetail.DefensePct >= 100f)));
			if (!m_IsMain)
			{
				zbSelf.Instance.eliteQuestHandler.GetQuestProgress(m_curLevel);
			}
			else
			{
				zbSelf.Instance.mainQuestHandler.GetQuestProgress(m_curLevel);
			}
			m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(mainQuestDetail.DefensePct, (LengthUnit)1));
			int score = (int)mainQuestDetail.Score;
			int scoreAward = (int)mainQuestDetail.ScoreAward;
			for (int i = 0; i < 3; i++)
			{
				int num2 = i + 1;
				string text = ((num2 <= scoreAward) ? "4" : ((num2 > score) ? "1" : "2"));
				TrackEntry current = m_spines[i].AnimationState.GetCurrent(0);
				object obj;
				if (current == null)
				{
					obj = null;
				}
				else
				{
					Animation animation = current.Animation;
					obj = ((animation != null) ? animation.Name : null);
				}
				if ((string?)obj != text)
				{
					m_spines[i].AnimationState.SetAnimation(0, text, true);
				}
			}
		}
		else
		{
			for (int j = 0; j < 3; j++)
			{
				m_spines[j].AnimationState.SetAnimation(0, "1", true);
			}
			((VisualElement)m_btnChallengeAgain).visible = false;
			m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(0f, (LengthUnit)1));
		}
	}

	private void OnFight(ClickEvent evt)
	{
		if (m_IsMain)
		{
			zbUIMgr.Instance.ShowMainLevel(0, m_curLevel);
		}
		else
		{
			zbUIMgr.Instance.ShowEliteQuest(m_curLevel);
		}
	}
}
