using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBossSubjectMainWnd
{
	private zbBossSubjectWnd m_wndParent;

	private VisualElement m_RootElement;

	private zbHolidayEventView m_holidayEventView;

	private Button m_btnLeft;

	private Button m_btnRight;

	private Button m_btnSweep;

	private Button m_btnFight;

	private Button m_btnFight2;

	private VisualElement m_veCanSweep;

	private VisualElement m_veTili;

	private VisualElement m_veFuncCard;

	private zbItem[] m_awardItems = new zbItem[5];

	private Label m_lbTitle;

	private Label m_lbName;

	private Label m_lbTime;

	private Label m_lbDiff;

	private Label m_lbTili;

	private Label m_lbLevel;

	private Label m_lbTili1;

	private Label m_lbTili2;

	private VisualElement m_lbTimeBg;

	private VisualElement m_lbAwardBg;

	private SpriteRenderer sceneBg;

	private SkeletonAnimation skeletonAnimation;

	private int currDiff;

	private BossSubjectDetail bossSubjectDetail;

	private zbBossSubjectCsvRecord bossSubjectRecord;

	private string lastBossSkin = "";

	private Button[] m_skillIcon = (Button[])(object)new Button[3];

	private Button m_btnMask;

	private Label m_lbSkillDesc;

	private Button m_btnSkillIcon;

	private zbMonsterPreview m_monster;

	private bool firstIn = true;

	private int m_costTili = 20;

	private zbBossLevelCsvRecord bossLevelRecord;

	public void OnEnable(zbBossSubjectWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "button_fight", (string)null);
		m_btnSweep = UQueryExtensions.Q<Button>(m_RootElement, "btn_sweep", (string)null);
		m_btnFight2 = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbDiff = UQueryExtensions.Q<Label>(m_RootElement, "lb_diff", (string)null);
		m_lbTili = UQueryExtensions.Q<Label>(m_RootElement, "lb_tili", (string)null);
		m_lbTili1 = UQueryExtensions.Q<Label>(m_RootElement, "lb_tili", (string)null);
		m_lbTili2 = UQueryExtensions.Q<Label>(m_RootElement, "lb_tili", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_lbTimeBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "lb_timebg", (string)null);
		m_lbAwardBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardPreview", (string)null);
		m_veCanSweep = UQueryExtensions.Q<VisualElement>(m_RootElement, "canSweep", (string)null);
		m_veTili = UQueryExtensions.Q<VisualElement>(m_RootElement, "veTili", (string)null);
		m_veFuncCard = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_funcCard", (string)null);
		((TextElement)m_lbTili).text = "x20";
		((TextElement)m_lbTili1).text = "x20";
		((TextElement)m_lbTili2).text = "x20";
		m_holidayEventView = new zbHolidayEventView();
		m_holidayEventView.OnEnable(wndParent, UQueryExtensions.Q<VisualElement>(m_RootElement, "holidayEvent", (string)null));
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_awardItems[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_awardItems[i].OnEnable(wndParent);
		}
		for (int j = 0; j < 3; j++)
		{
			int i2 = j + 1;
			m_skillIcon[j] = UQueryExtensions.Q<Button>(UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_Skillframe" + i2, (string)null), "btn_Skillicon" + i2, (string)null);
			wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_skillIcon[j], (Action<ClickEvent>)delegate
			{
				OnSkill(i2);
			}, "", (TrickleDown)0);
		}
		m_btnMask = UQueryExtensions.Q<Button>(m_RootElement, "btn_mask", (string)null);
		m_lbSkillDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_attackDesc", (string)null);
		m_btnSkillIcon = UQueryExtensions.Q<Button>(m_RootElement, "btn_Skillicon", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action<ClickEvent>)delegate
		{
			OnGotoNext(-1);
		}, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action<ClickEvent>)delegate
		{
			OnGotoNext(1);
		}, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action<ClickEvent>)OnFight, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight2, (Action<ClickEvent>)OnFight, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMask, (Action)OnCloseMask, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweep, (Action<ClickEvent>)OnSweep, "", (TrickleDown)0);
		Transform val = m_wndParent.GetMyGameObjectSceneRoot().transform.Find("mainObject");
		m_monster = zbUtil.FindGameObjectRecursive(val, "monsterPreview").GetComponent<zbMonsterPreview>();
		sceneBg = ((Component)val.Find("spineScene").Find("SceneBg")).GetComponent<SpriteRenderer>();
		GameObject gameObject = ((Component)val.Find("spineScene").Find("SkeletonAnimation")).gameObject;
		skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
		m_wndParent.CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
		Refresh();
	}

	public void Refresh()
	{
		firstIn = true;
		Init();
		OnCloseMask();
	}

	private void Init()
	{
		zbSelf.Instance.bossSubjectHandler.SetVisualElementSprite((VisualElement)(object)m_lbTitle, "main_title_bg.png");
		zbSelf.Instance.bossSubjectHandler.SetVisualElementSprite((VisualElement)(object)m_lbName, "main_name_bg.png");
		zbSelf.Instance.bossSubjectHandler.SetVisualElementSprite(m_lbTimeBg, "main_time_bg.png");
		zbSelf.Instance.bossSubjectHandler.SetVisualElementSprite((VisualElement)(object)m_lbDiff, "main_diff_bg.png");
		zbSelf.Instance.bossSubjectHandler.SetVisualElementSprite(m_lbAwardBg, "main_award_bg.png");
		zbSelf.Instance.bossSubjectHandler.SetGameObjectSprite(sceneBg, "main_bg.jpg");
		zbSelf.Instance.bossSubjectHandler.SetSkeletonAnimation(skeletonAnimation, "bossSubjectTiaozhanbg");
	}

	public void RefreshCustom(BossSubjectDetail _bossSubjectDetail, bool fromOut = true)
	{
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		bossSubjectDetail = _bossSubjectDetail;
		bossSubjectRecord = zbDataConfig.Instance.bossSubjectCsvTable.GetRecordByKey(bossSubjectDetail.BossSubject.SubjectId);
		m_holidayEventView.Refresh(bossSubjectDetail.BaseInfo.PlanId);
		if (fromOut)
		{
			currDiff = Math.Clamp(bossSubjectDetail.BossSubject.PassLevel + 1, 1, zbSelf.Instance.bossSubjectHandler.maxLevel);
		}
		m_monster.Refresh(bossSubjectRecord.boss);
		if (lastBossSkin == "")
		{
			ActionMonsterSkin(1);
		}
		if (firstIn)
		{
			firstIn = false;
			m_monster.m_spineAnimation.AnimationState.SetAnimation(0, "skill2", false);
			m_monster.m_spineAnimation.AnimationState.AddAnimation(0, "idle", true, 0f);
		}
		((TextElement)m_lbName).text = zbUtil.Language(bossSubjectRecord.name);
		for (int i = 0; i < 3; i++)
		{
			string resName = i switch
			{
				0 => bossSubjectRecord.skillIcon1, 
				1 => bossSubjectRecord.skillIcon2, 
				_ => bossSubjectRecord.skillIcon3, 
			};
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(resName);
			((VisualElement)m_skillIcon[i]).style.backgroundImage = new StyleBackground(val);
		}
		RefreshDetail();
	}

	public void RefreshDetail()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		currDiff = Math.Clamp(currDiff, 1, bossSubjectDetail.BossSubject.PassLevel + 1);
		currDiff = Math.Clamp(currDiff, 1, zbSelf.Instance.bossSubjectHandler.maxLevel);
		((TextElement)m_lbDiff).text = string.Format(zbUtil.Language("lang.10842"), currDiff);
		if (currDiff <= 1)
		{
			((VisualElement)m_btnLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_btnLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		if (currDiff >= bossSubjectDetail.BossSubject.PassLevel + 1 || currDiff >= zbSelf.Instance.bossSubjectHandler.maxLevel)
		{
			((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		((VisualElement)m_btnFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((currDiff < bossSubjectDetail.BossSubject.PassLevel + 1) ? 1 : 0));
		m_veCanSweep.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((currDiff >= bossSubjectDetail.BossSubject.PassLevel + 1) ? 1 : 0));
		m_veTili.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.GetVipLevel() < 3));
		m_veFuncCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.GetVipLevel() >= 3));
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_awardItems[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		bossLevelRecord = zbDataConfig.Instance.bossLevelCsvTable.GetRecordByKey(currDiff);
		int[][] awardShow = bossLevelRecord.awardShow;
		foreach (int[] array in awardShow)
		{
			if (num > 4)
			{
				break;
			}
			m_awardItems[num].Refresh(new zbItemData((EResourceType)array[0], array[1], array[2]));
			((VisualElement)m_awardItems[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			num++;
		}
		bool flag = bossLevelRecord.level != 1;
		((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			((TextElement)m_lbLevel).text = zbUtil.Language("lang.11342") + ":Day" + bossLevelRecord.adviceLevel;
		}
	}

	private void ActionMonsterSkin(int i)
	{
		lastBossSkin = "skin" + i;
		if (((SkeletonRenderer)m_monster.m_spineAnimation).Skeleton.Data.FindAnimation(lastBossSkin) != null)
		{
			m_monster.m_spineAnimation.AnimationState.SetAnimation(1, lastBossSkin, true);
		}
	}

	public void OnFight(ClickEvent evt)
	{
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 4, m_costTili))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, m_costTili);
		}
		else if (currDiff < bossSubjectDetail.BossSubject.PassLevel + 1 && currDiff != zbSelf.Instance.bossSubjectHandler.maxLevel && zbSelf.Instance.bossSubjectHandler.shouldShowBossSubjectTip)
		{
			zbUIMgr.Instance.ShowBossSubjectPerfectPassTip(bossSubjectDetail, currDiff);
		}
		else
		{
			zbWsMsgHandler.Instance.Operate_BossSubFightStart_Req(bossSubjectDetail.BaseInfo.UUID, currDiff);
		}
	}

	public void OnSweep(ClickEvent evt)
	{
		if (zbSelf.Instance.GetVipLevel() >= 3)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 4, m_costTili))
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, m_costTili);
			}
			else
			{
				zbWsMsgHandler.Instance.Operate_BossSubSaoDang_Req(bossSubjectDetail.BaseInfo.UUID);
			}
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), 3));
		}
	}

	private void OnGotoNext(int index)
	{
		currDiff += index;
		RefreshDetail();
	}

	private void OnSkill(int i)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		ActionMonsterSkin(i);
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnMask).SetEnabled(true);
		((VisualElement)m_btnMask).pickingMode = (PickingMode)0;
		string resName;
		string key;
		switch (i)
		{
		case 1:
			resName = bossSubjectRecord.skillIcon1;
			key = bossSubjectRecord.skilldesc1;
			break;
		case 2:
			resName = bossSubjectRecord.skillIcon2;
			key = bossSubjectRecord.skilldesc2;
			break;
		default:
			resName = bossSubjectRecord.skillIcon3;
			key = bossSubjectRecord.skilldesc3;
			break;
		}
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(resName);
		((VisualElement)m_btnSkillIcon).style.backgroundImage = new StyleBackground(val);
		((TextElement)m_lbSkillDesc).text = zbUtil.Language(key);
	}

	private void OnCloseMask()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnMask).SetEnabled(false);
		((VisualElement)m_btnMask).pickingMode = (PickingMode)1;
	}

	public void UpdateAtShow()
	{
		m_holidayEventView.Update();
	}

	public void UpdatePerSecond()
	{
		RefreshTime();
	}

	private void RefreshTime()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (bossSubjectDetail != null)
		{
			long num = bossSubjectDetail.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(num);
			((VisualElement)m_lbTime).style.color = StyleColor.op_Implicit((num < 86400) ? Color.red : Color.white);
		}
	}

	public void SetCurrDiff(int _currDiff)
	{
		currDiff = _currDiff;
		bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
		if (bossSubjectDetail != null)
		{
			RefreshCustom(bossSubjectDetail, fromOut: false);
		}
	}
}
