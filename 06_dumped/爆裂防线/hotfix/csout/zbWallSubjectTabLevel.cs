using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallSubjectTabLevel
{
	private zbWallSubjectWnd m_wndParent;

	private VisualElement m_RootElement;

	private zbHolidayEventView m_holidayEventView;

	private VisualElement m_nodeTarget;

	private VisualElement m_spriteProgress;

	private Label m_lbTarget;

	private Label m_lbTargetDesc;

	private zbItem[] m_targets;

	private zbItem[] m_awardItems;

	private Button[] m_btnChapters;

	private Button m_btnSkill;

	private VisualElement m_spriteSkill;

	private Button m_btnFight;

	private Label m_lbFight;

	private Label m_lbLevel;

	private Label m_lbPassed;

	private Button m_btnRelic;

	private Button m_btnBar;

	private VisualElement m_notifyBar;

	private VisualElement m_newRelic;

	private int m_maxLevel;

	private int m_curLevel;

	private zbWallSubjectLevelCsvRecord m_levelRecord;

	private Vector3[] currentPos;

	private int m_wallId;

	public void OnEnable(zbWallSubjectWnd wndParent, VisualElement root)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_holidayEventView = new zbHolidayEventView();
		m_holidayEventView.OnEnable(wndParent, UQueryExtensions.Q<VisualElement>(m_RootElement, "holidayEvent", (string)null), 120f);
		m_nodeTarget = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_target", (string)null);
		if ((int)Application.platform == 8)
		{
			m_nodeTarget.style.top = StyleLength.op_Implicit(188f);
		}
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbTarget = UQueryExtensions.Q<Label>(m_RootElement, "lb_target", (string)null);
		m_lbTargetDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_targetDesc", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_targets[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "target" + i, (string)null);
			m_targets[i].OnEnable(wndParent);
		}
		for (int j = 0; j < 5; j++)
		{
			m_awardItems[j] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + j, (string)null);
			m_awardItems[j].OnEnable(wndParent);
			m_btnChapters[j] = UQueryExtensions.Q<Button>(m_RootElement, "btn_chapter" + j, (string)null);
			int tempIndex = j;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnChapters[j], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnChapters(evt, tempIndex);
			}, "", (TrickleDown)0);
		}
		m_btnSkill = UQueryExtensions.Q<Button>(m_RootElement, "btn_skill", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_lbFight = UQueryExtensions.Q<Label>(m_RootElement, "lb_fight", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_lbPassed = UQueryExtensions.Q<Label>(m_RootElement, "lb_passed", (string)null);
		m_btnRelic = UQueryExtensions.Q<Button>(m_RootElement, "btn_relic", (string)null);
		m_btnBar = UQueryExtensions.Q<Button>(m_RootElement, "btn_bar", (string)null);
		m_notifyBar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnBar, (Vector2?)new Vector2(10f, 10f), 1f, true);
		m_newRelic = zbUtil.CreateNewTag((VisualElement)(object)m_btnRelic, (Vector2?)new Vector2(-3f, -3f), 1f);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkill, (Action)OnSkill, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRelic, (Action)OnRelic, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBar, (Action)OnBar, "", (TrickleDown)0);
		InitRecord();
	}

	public void InitRecord()
	{
		for (int i = 0; i < 4; i++)
		{
			int[] array = zbSelf.Instance.wallSubjectHandler.m_targetAwards[i][0];
			if (array != null)
			{
				m_targets[i].Refresh(new zbItemData((EResourceType)array[0], array[1], array[2]));
			}
		}
	}

	public void Refresh()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_data != null)
		{
			m_wallId = m_wndParent.wallSubjectRecord.wallId;
			zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(m_wallId);
			if (recordByKey != null)
			{
				m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon));
			}
			int[] array = m_wndParent.wallSubjectRecord.chapterGrandAward[0];
			if (array != null)
			{
				m_targets[4].Refresh(new zbItemData((EResourceType)array[0], array[1], array[2]));
			}
			m_holidayEventView.Refresh(m_wndParent.m_data.BaseInfo.PlanId);
			m_maxLevel = zbSelf.Instance.wallSubjectHandler.GetMaxLevel(m_wndParent.m_data.SubjectId);
			m_curLevel = Math.Clamp(m_wndParent.m_data.ChapterInfo.PassLevel + 1, 1, m_maxLevel);
			m_levelRecord = zbSelf.Instance.wallSubjectHandler.m_levelRecords.FirstOrDefault((zbWallSubjectLevelCsvRecord r) => r.subjectId == m_wndParent.m_data.SubjectId && r.level == m_curLevel);
			((Component)m_wndParent.spineCurrent).transform.position = currentPos[m_levelRecord.chapter - 1];
			((VisualElement)m_lbPassed).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_wndParent.m_data.ChapterInfo.PassLevel < m_maxLevel) ? 1 : 0));
			((VisualElement)m_btnFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_wndParent.m_data.ChapterInfo.PassLevel >= m_maxLevel) ? 1 : 0));
			m_notifyBar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallSubjectHandler.HasBarNotify(m_wndParent.m_data)));
			m_newRelic.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallSubjectHandler.HasRelicNew()));
			RefreshTarget();
			RefreshAwardList();
		}
	}

	private void RefreshTarget()
	{
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 20; i++)
		{
			if (m_wndParent.m_data.ChapterInfo.PassLevel >= m_maxLevel || m_curLevel > i + 1)
			{
				zbSelf.Instance.wallSubjectHandler.SetGameObjectSprite(m_wndParent.m_spriteLevels[i], "wallSubject_point1.png");
			}
			else
			{
				zbSelf.Instance.wallSubjectHandler.SetGameObjectSprite(m_wndParent.m_spriteLevels[i], "wallSubject_point0.png");
			}
		}
		for (int j = 0; j < 5; j++)
		{
			int chapterId = j + 1;
			ECommonState levelTargeState = zbSelf.Instance.wallSubjectHandler.GetLevelTargeState(m_wndParent.m_data, chapterId);
			if (levelTargeState == ECommonState.Yes)
			{
				m_targets[j].SetTouchCallback(delegate
				{
					zbWsMsgHandler.Instance.Operate_WallSubjectChapterAward_Req(m_wndParent.m_data.BaseInfo.UUID, chapterId);
				});
			}
			else
			{
				m_targets[j].SetTouchCallback(null);
			}
			m_targets[j].SetSpineVisible(levelTargeState == ECommonState.Yes);
			m_targets[j].SetGettedVisible(levelTargeState == ECommonState.End);
		}
		float num = Mathf.Clamp((float)m_wndParent.m_data.ChapterInfo.PassLevel / (float)m_maxLevel, 0f, 1f);
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num * 556f + 45f, (LengthUnit)0));
		((TextElement)m_lbTarget).text = GetPassChapter() + "/5";
		if (zbUtil.Language("lang.11396") == "")
		{
			((TextElement)m_lbLevel).text = m_levelRecord.chapter + "-" + GetCurChapterLevel();
			return;
		}
		((TextElement)m_lbLevel).text = zbUtil.Language("lang.11396") + ": " + m_levelRecord.chapter + "-" + GetCurChapterLevel();
	}

	private int GetPassChapter()
	{
		int result = 0;
		foreach (OperateSubjectChapterDetail chapter in m_wndParent.m_data.ChapterInfo.ChapterList)
		{
			if (chapter.ChapterAward == ECommonState.Yes || chapter.ChapterAward == ECommonState.End)
			{
				result = chapter.ChapterId;
			}
		}
		return result;
	}

	private int GetCurChapterLevel()
	{
		int num = 0;
		zbWallSubjectLevelCsvRecord[] levelRecords = zbSelf.Instance.wallSubjectHandler.m_levelRecords;
		foreach (zbWallSubjectLevelCsvRecord zbWallSubjectLevelCsvRecord2 in levelRecords)
		{
			if (zbWallSubjectLevelCsvRecord2.chapter == m_levelRecord.chapter - 1 && zbWallSubjectLevelCsvRecord2.level > num)
			{
				num = zbWallSubjectLevelCsvRecord2.level;
			}
		}
		return m_curLevel - num;
	}

	private void RefreshAwardList()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_awardItems[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		int[][] award = m_levelRecord.award;
		foreach (int[] array in award)
		{
			if (num <= 4)
			{
				m_awardItems[num].Refresh(new zbItemData((EResourceType)array[0], array[1], array[2]));
				((VisualElement)m_awardItems[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				num++;
				continue;
			}
			break;
		}
	}

	public void OnChapters(ClickEvent evt, int index)
	{
		if (index + 1 <= GetPassChapter())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11398"));
		}
		else if (index + 1 == m_levelRecord.chapter)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11400"));
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11399"));
		}
	}

	private void OnSkill()
	{
		zbUIMgr.Instance.ShowWallSkill(m_wallId, isPreview: true);
	}

	private void OnFight()
	{
		zbWsMsgHandler.Instance.Operate_WallSubjectFightStart_Req(m_wndParent.m_data.BaseInfo.UUID, m_curLevel);
	}

	private void OnRelic()
	{
		zbUIMgr.Instance.ShowWallSubjectRelic(m_wndParent.m_data);
	}

	private void OnBar()
	{
		zbUIMgr.Instance.ShowWallSubjectBar(m_wndParent.m_data);
	}

	public void Update()
	{
		m_holidayEventView.Update();
	}

	public zbWallSubjectTabLevel()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		m_targets = new zbItem[5];
		m_awardItems = new zbItem[5];
		m_btnChapters = (Button[])(object)new Button[5];
		currentPos = (Vector3[])(object)new Vector3[5]
		{
			new Vector3(1.9f, -0.2f, -5.8f),
			new Vector3(-1.7f, 0.5f, -5.8f),
			new Vector3(1.45f, 1.5f, -5.8f),
			new Vector3(-1f, 3.2f, -5.8f),
			new Vector3(1.8f, 4f, -5.8f)
		};
		base._002Ector();
	}
}
