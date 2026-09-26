using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbElementTrialItem : zbViewBaseItem
{
	private zbElementTrialWnd m_wndParent;

	private Label m_lbTrialName;

	private Label m_lbLv;

	private VisualElement m_banner;

	private zbItem[] m_awards = new zbItem[4];

	private Label m_lbDesc;

	private Button m_btnSweep;

	private Button m_btnFight;

	private Sprite spriteGray;

	private Sprite spriteYellow;

	private Sprite spriteGreen;

	private Button[] m_btnSkillTypes = (Button[])(object)new Button[6];

	private List<int> m_skillTypeList = new List<int>();

	private zbElementTrialCsvRecord m_record;

	private TrialDetail m_trialDetail;

	public zbElementTrialItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/heroTrial/tb_elementTrial.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbElementTrialWnd wndParent)
	{
		m_wndParent = wndParent;
		m_lbTrialName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_trialName", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_lv", (string)null);
		m_banner = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "banner", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, $"award{i}", (string)null);
			m_awards[i].OnEnable(wndParent);
		}
		for (int j = 0; j < 6; j++)
		{
			m_btnSkillTypes[j] = UQueryExtensions.Q<Button>((VisualElement)(object)this, $"btn_skillType{j}", (string)null);
			int currentIndex = j;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkillTypes[j], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkillTypes(evt, currentIndex);
			}, "", (TrickleDown)0);
		}
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_fight", (string)null);
		m_btnSweep = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_sweep", (string)null);
		spriteGray = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_gray.png");
		spriteYellow = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png");
		spriteGreen = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_green.png");
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweep, (Action)OnSweep, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbElementTrialCsvRecord record)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		((TextElement)m_lbTrialName).text = zbUtil.Language(record.name);
		m_trialDetail = zbSelf.Instance.elementTrialHandler.GetTrialInfoByAttackType((ESkillAttackType)m_record.trialType);
		m_banner.style.backgroundImage = new StyleBackground(zbSelf.Instance.elementTrialHandler.GetBannerSpriteByAttackType((ESkillAttackType)m_record.trialType));
		((TextElement)m_lbLv).text = m_trialDetail.HistoryLevel.ToString();
		((VisualElement)m_btnSweep).style.backgroundImage = new StyleBackground((m_trialDetail.Level > 0) ? spriteGreen : spriteGray);
		((VisualElement)m_btnFight).style.backgroundImage = new StyleBackground((!zbSelf.Instance.elementTrialHandler.CheckElementTrialIsUnLock((ESkillAttackType)m_record.trialType) || m_trialDetail.Level >= zbSelf.Instance.elementTrialHandler.GetMaxLevel() || !zbSelf.Instance.elementTrialHandler.CheckCanFight(m_trialDetail.Level + 1)) ? spriteGray : spriteYellow);
		InitAward();
		InitDesc();
		InitBtnType();
	}

	private void InitBtnType()
	{
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		m_skillTypeList.Clear();
		switch ((ESkillAttackType)m_record.trialType)
		{
		case ESkillAttackType.Ice:
			m_skillTypeList.Add(10);
			m_skillTypeList.Add(4);
			break;
		case ESkillAttackType.Fire:
			m_skillTypeList.Add(2);
			m_skillTypeList.Add(5);
			break;
		case ESkillAttackType.Phy:
			m_skillTypeList.Add(1);
			m_skillTypeList.Add(9);
			m_skillTypeList.Add(6);
			m_skillTypeList.Add(8);
			break;
		case ESkillAttackType.Energy:
			m_skillTypeList.Add(3);
			m_skillTypeList.Add(11);
			break;
		case ESkillAttackType.Elec:
			m_skillTypeList.Add(7);
			m_skillTypeList.Add(12);
			break;
		case ESkillAttackType.Wind:
			m_skillTypeList.Add(14);
			m_skillTypeList.Add(13);
			break;
		}
		for (int i = 0; i < 6; i++)
		{
			bool flag = i < m_skillTypeList.Count;
			((VisualElement)m_btnSkillTypes[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbSkillTypeCsvRecord recordByKey = zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey(m_skillTypeList[i]);
				Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon1);
				((VisualElement)m_btnSkillTypes[i]).style.backgroundImage = new StyleBackground(val);
			}
		}
	}

	private void InitAward()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			bool flag = i < m_record.awardShow.Length;
			((VisualElement)m_awards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData((EResourceType)m_record.awardShow[i][0], m_record.awardShow[i][1], m_record.awardShow[i][2] + m_record.awardShow[i][3] * m_trialDetail.Level);
				m_awards[i].Refresh(data);
			}
		}
	}

	private void InitDesc()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (!zbSelf.Instance.elementTrialHandler.CheckElementTrialIsUnLock((ESkillAttackType)m_record.trialType))
		{
			((VisualElement)m_lbDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			switch ((ESkillAttackType)m_record.trialType)
			{
			case ESkillAttackType.Ice:
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.10753");
				break;
			case ESkillAttackType.Fire:
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.10748");
				break;
			case ESkillAttackType.Phy:
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.10751");
				break;
			case ESkillAttackType.Energy:
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.10752");
				break;
			case ESkillAttackType.Elec:
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.10750");
				break;
			case ESkillAttackType.Wind:
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.10749");
				break;
			}
		}
		else if (m_trialDetail.Level + 1 >= zbSelf.Instance.elementTrialHandler.GetMaxLevel())
		{
			((VisualElement)m_lbDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.10755");
		}
		else if (m_trialDetail.Level > 0)
		{
			((VisualElement)m_lbDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.10766");
		}
	}

	private void OnFight()
	{
		if (!zbSelf.Instance.elementTrialHandler.CheckElementTrialIsUnLock((ESkillAttackType)m_record.trialType))
		{
			switch ((ESkillAttackType)m_record.trialType)
			{
			case ESkillAttackType.Ice:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10753"));
				break;
			case ESkillAttackType.Fire:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10748"));
				break;
			case ESkillAttackType.Phy:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10751"));
				break;
			case ESkillAttackType.Energy:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10752"));
				break;
			case ESkillAttackType.Elec:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10750"));
				break;
			case ESkillAttackType.Wind:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10749"));
				break;
			}
		}
		else if (!zbSelf.Instance.elementTrialHandler.CheckCanFight(m_trialDetail.Level + 1))
		{
			zbElementTrialLevelCsvRecord recordByKey = zbDataConfig.Instance.elementTrialLevelCsvTable.GetRecordByKey(m_trialDetail.Level + 1);
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10754"), recordByKey.unlockTotal));
		}
		else if (zbSelf.Instance.elementTrialHandler.CanElementTrialCombat() && zbSelf.Instance.elementTrialHandler.CheckCanFight(m_trialDetail.Level + 1))
		{
			zbWsMsgHandler.Instance.ElementTrial_FightStart_Req((ESkillAttackType)m_record.trialType, m_trialDetail.Level + 1);
		}
		else if (zbSelf.Instance.elementTrialHandler.CanElementTrialBuyCount())
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.ElementTrialPhy);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10700"));
		}
	}

	private void OnSweep()
	{
		if (m_trialDetail.Level <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10756"));
		}
		else if (zbSelf.Instance.elementTrialHandler.CanElementTrialCombat())
		{
			zbWsMsgHandler.Instance.ElementTrial_SaoDang_Req((ESkillAttackType)m_record.trialType);
		}
		else if (zbSelf.Instance.elementTrialHandler.CanElementTrialBuyCount())
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.ElementTrialPhy);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10700"));
		}
	}

	public void OnSkillTypes(ClickEvent evt, int index)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (m_skillTypeList[index] > 0)
		{
			Vector2 val = VisualElementExtensions.WorldToLocal(m_wndParent.GetRootElement(), Vector2.op_Implicit(((PointerEventBase<ClickEvent>)(object)evt).position));
			val.x = Mathf.Clamp(val.x - 160f, 20f, 340f);
			val.y += 575f;
			zbUIMgr.Instance.ShowTipSkillType(m_skillTypeList[index], val);
		}
	}
}
