using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbModWheelWnd : zbBaseWnd
{
	private const int PanelCount = 4;

	[NonSerialized]
	private VisualElement node_bg;

	[NonSerialized]
	private Button m_btnRefresh;

	[NonSerialized]
	private Button m_btnVisible;

	private Button m_btnTouch;

	private Label m_lbTouch;

	[NonSerialized]
	private zbModHorizontalPanel[] m_ModPanels = new zbModHorizontalPanel[4];

	[NonSerialized]
	private zbModWheelScroll[] m_ModScrolls = new zbModWheelScroll[4];

	private Button m_btnModList;

	[NonSerialized]
	private JObject gameBaseRecord;

	[NonSerialized]
	public int[] selectMod = new int[4];

	private int monsterTid;

	private zbCombatTeam m_masterTeam;

	[NonSerialized]
	private zbMonsterCsvRecord monsterRecord;

	private Label m_lbTip;

	private int selectNum = 1;

	private readonly float iconHeight = 125f;

	private float[] durations = new float[4] { 0f, 0.3f, 0.5f, 0.7f };

	private List<float> iconY = new List<float>();

	private bool skipFlag;

	private bool[] allFinishs = new bool[4];

	private SpineVisualElement spineAward;

	private bool actionAnimation;

	private AudioSource m_audio;

	private readonly float wheelRunTime = 2f;

	private bool m_canSelect = true;

	private Button m_btnAuto;

	private VisualElement m_spriteOk;

	private int autoLeftTime;

	private VisualElement m_spriteLeft;

	private Label m_lbAutoTime;

	protected override void OnEnable()
	{
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 8; i++)
		{
			iconY.Add(60f - (float)i * iconHeight);
		}
		base.OnEnable();
		spineAward = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_award", (string)null);
		node_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "nodebg", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_RootElement, "lb_tip", (string)null);
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_refresh", (string)null);
		m_btnVisible = UQueryExtensions.Q<Button>(m_RootElement, "btn_visible", (string)null);
		m_btnModList = UQueryExtensions.Q<Button>(m_RootElement, "btn_modList", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTouch = UQueryExtensions.Q<Label>(m_RootElement, "lb_touch", (string)null);
		((VisualElement)m_btnVisible).visible = false;
		((VisualElement)m_btnModList).visible = false;
		((TextElement)m_lbTip).text = zbUtil.Language("lang.10408");
		m_spriteLeft = UQueryExtensions.Q<VisualElement>(m_RootElement, "lb_left", (string)null);
		m_lbAutoTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_leftNum", (string)null);
		m_btnAuto = UQueryExtensions.Q<Button>(m_RootElement, "btn_auto", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAuto, (Action)OnAuto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action)OnRefresh, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnSkip, "", (TrickleDown)0);
		for (int j = 0; j < 4; j++)
		{
			zbModHorizontalPanel zbModHorizontalPanel2 = new zbModHorizontalPanel(UQueryExtensions.Q<VisualElement>(m_RootElement, "mod_item" + j, (string)null));
			m_ModPanels[j] = zbModHorizontalPanel2;
			zbModWheelScroll zbModWheelScroll2 = new zbModWheelScroll(UQueryExtensions.Q<VisualElement>(m_RootElement, "scroll_" + j, (string)null));
			m_ModScrolls[j] = zbModWheelScroll2;
			m_ModScrolls[j].root.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(-320f)));
		}
		AddDark();
	}

	public void ResetValue()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		skipFlag = false;
		node_bg.visible = false;
		((VisualElement)m_btnTouch).visible = false;
		((VisualElement)spineAward).visible = false;
		actionAnimation = false;
		((VisualElement)m_btnRefresh).visible = false;
		gameBaseRecord = zbDataConfig.Instance.gameBaseJsonTable.GetRecord();
		((TextElement)m_lbTouch).text = zbUtil.Language("lang.10409");
		selectNum = 1;
		if (monsterRecord.type == 3)
		{
			JArray val = (JArray)gameBaseRecord["modFallDown"][(object)"bossModNum"];
			selectNum = Rand.Range(Extensions.Value<int>((IEnumerable<JToken>)val[0]), Extensions.Value<int>((IEnumerable<JToken>)val[1]) + 1);
		}
		else
		{
			JArray val2 = (JArray)gameBaseRecord["modFallDown"][(object)"eliteModNum"];
			selectNum = Rand.Range(Extensions.Value<int>((IEnumerable<JToken>)val2[0]), Extensions.Value<int>((IEnumerable<JToken>)val2[1]) + 1);
		}
		for (int i = 0; i < 4; i++)
		{
			selectMod[i] = 0;
			allFinishs[i] = false;
		}
		m_audio = zbAudioMgr.Instance.PlayAudio("audio/combat/laohuji.ogg", loop: true);
	}

	public void RefreshCustom(zbCombatTeam team, int _monsterTid)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		monsterTid = _monsterTid;
		m_masterTeam = team;
		monsterRecord = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid);
		if (monsterRecord == null)
		{
			OnHideSelf();
			return;
		}
		(zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd).SetAlertRandgeVisible(null, vis: false);
		((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbCombatSystem.Instance.refreshWheelNum <= 0));
		zbCombatSystem.Instance.Pause();
		zbAudioMgr.Instance.PauseAllAudio();
		ResetValue();
		FilterMods();
		for (int i = 0; i < 4; i++)
		{
			m_ModScrolls[i].RefreshDetail(m_masterTeam);
			((MonoBehaviour)this).StartCoroutine(SpinReel(m_ModScrolls[i], durations[i], i));
		}
		m_canSelect = true;
		((MonoBehaviour)this).StartCoroutine(OtherIEnumerator());
		zbCombatSystem.Instance.combatRecorder.NeedModWheelSave(_monsterTid, selectMod);
		RefreshAuto();
	}

	private bool IsAutoing()
	{
		return zbSelf.Instance.combatHandler.IsAutoSelectMod(zbCombatSystem.Instance.GetBattleType());
	}

	private void RefreshAuto()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		EBattleFuncType battleType = zbCombatSystem.Instance.GetBattleType();
		bool flag = zbSelf.Instance.combatHandler.IsCanAutoSelectMod(battleType);
		bool flag2 = IsAutoing();
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		m_spriteLeft.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(flag & flag2)));
		((VisualElement)m_btnAuto).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_lbTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		autoLeftTime = 0;
		UpdatePerSecond();
	}

	private void OnAuto()
	{
		EBattleFuncType battleType = zbCombatSystem.Instance.GetBattleType();
		if (zbSelf.Instance.combatHandler.IsCanAutoSelectMod(battleType))
		{
			if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.AutoFight, zbSelf.Instance.GetVipLevel()) <= 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.AutoFight)) + "【" + zbUtil.Language("lang.11494") + "】");
				return;
			}
			zbSelf.Instance.combatHandler.SetAutoSelectMod(battleType);
			RefreshAuto();
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		if (IsAutoing())
		{
			((TextElement)m_lbAutoTime).text = (10 - autoLeftTime).ToString();
			if (autoLeftTime >= 10)
			{
				autoLeftTime = 0;
				UseSelectMode();
			}
			autoLeftTime = Math.Min(autoLeftTime + 1, 10);
		}
	}

	public void RefreshModDetail()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			int num = selectMod[i];
			if (num > 0)
			{
				zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(num);
				m_ModPanels[i].RefreshDetail(m_masterTeam, recordByKey);
				((VisualElement)m_ModPanels[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)UQueryExtensions.Q<TemplateContainer>(m_RootElement, "mod_item" + i, (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				((VisualElement)UQueryExtensions.Q<TemplateContainer>(m_RootElement, "mod_item" + i, (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
		((TextElement)m_lbTouch).text = zbUtil.Language("lang.10412");
	}

	public void FilterMods(int manyTime = 0)
	{
		if (manyTime >= selectNum)
		{
			return;
		}
		List<zbModBattleRandCsvRecord> list = new List<zbModBattleRandCsvRecord>();
		zbModBattleRandCsvTableData records = zbDataConfig.Instance.modBattleRandCsvTable.GetRecords();
		zbCombatExp combatExp = zbCombatSystem.Instance.combatData.combatExp;
		zbModBattleRandCsvRecord[] records2 = records.records;
		foreach (zbModBattleRandCsvRecord record in records2)
		{
			if (record.randId != zbCombatSystem.Instance.battleInfo.BattleBaseInfo.ModRandId || m_masterTeam.modMgr.IsModIdSelMax(record.modId, record.maxNum) || (int)combatExp.curLevel + combatExp.initLevel < record.battleLv)
			{
				continue;
			}
			int num = selectMod.Count((int n) => n == record.modId);
			if (m_masterTeam.modMgr.IsModIdSelMax(record.modId, record.maxNum - num))
			{
				continue;
			}
			bool flag = true;
			int[] requireMod = record.requireMod;
			foreach (int num3 in requireMod)
			{
				if (!m_masterTeam.modMgr.HasMod(num3) && !Enumerable.Contains(selectMod, num3))
				{
					flag = false;
					break;
				}
			}
			requireMod = record.conflictMod;
			foreach (int num4 in requireMod)
			{
				if (m_masterTeam.modMgr.HasMod(num4) || Enumerable.Contains(selectMod, num4))
				{
					flag = false;
					break;
				}
			}
			requireMod = record.requireSkillType;
			foreach (int skillType in requireMod)
			{
				if (!m_masterTeam.HasSkillType((ESkillType)skillType))
				{
					flag = false;
					break;
				}
			}
			requireMod = record.conflictSkillId;
			foreach (int skillId in requireMod)
			{
				if (m_masterTeam.modMgr.HasSkillId(skillId))
				{
					flag = false;
					break;
				}
			}
			if (zbDataConfig.Instance.modCsvTable.GetRecordByKey(record.modId).modIndex != EModIndexDefine.Index1000 && flag)
			{
				list.Add(record);
			}
		}
		selectMod[manyTime] = WeightedPickExclude(list);
		FilterMods(manyTime + 1);
	}

	private int WeightedPickExclude(List<zbModBattleRandCsvRecord> list)
	{
		int num = 0;
		foreach (zbModBattleRandCsvRecord item in list)
		{
			num = ((!Enumerable.Contains(selectMod, item.modId)) ? (num + item.weight) : (num + 1));
		}
		if (num == 0)
		{
			return 0;
		}
		int num2 = Rand.Range(0, num);
		int num3 = 0;
		foreach (zbModBattleRandCsvRecord item2 in list)
		{
			num3 = ((!Enumerable.Contains(selectMod, item2.modId)) ? (num3 + item2.weight) : (num3 + 1));
			if (num2 < num3)
			{
				return item2.modId;
			}
		}
		return 0;
	}

	private void OnHideSelf()
	{
		if (Object.op_Implicit((Object)(object)m_audio))
		{
			zbAudioMgr.Instance.StopAudio(m_audio);
			m_audio = null;
		}
		if (!zbUIMgr.Instance.IsShow(EWndId.SelectMod))
		{
			zbCombatSystem.Instance.Resume();
			zbAudioMgr.Instance.ResumeAllAudio();
		}
		Hide();
	}

	private void OnSwitchConfirm(bool isOk)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (isOk)
		{
			zbCombatSystem.Instance.refreshWheelNum--;
			((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbCombatSystem.Instance.refreshWheelNum <= 0));
			ResetValue();
			FilterMods();
			for (int i = 0; i < 4; i++)
			{
				m_ModScrolls[i].RefreshDetail(m_masterTeam);
				((MonoBehaviour)this).StartCoroutine(SpinReel(m_ModScrolls[i], durations[i], i));
			}
			((MonoBehaviour)this).StartCoroutine(OtherIEnumerator());
			((TextElement)m_lbTouch).text = zbUtil.Language("lang.10409");
		}
	}

	public void OnRefresh()
	{
		if (zbCombatSystem.Instance.refreshWheelNum >= 0)
		{
			OnSwitchConfirm(isOk: true);
			RefreshAuto();
		}
	}

	public void OnSkip()
	{
		skipFlag = true;
		if (m_canSelect && allFinishs.Count((bool n) => n) >= 4)
		{
			UseSelectMode();
		}
	}

	public void UseSelectMode()
	{
		OnHideSelf();
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < 4; i++)
		{
			int num = selectMod[i];
			if (num != 0)
			{
				m_masterTeam.UseSelectMode(num);
				list.Add(num);
				list2.Add(i);
			}
		}
		zbUIMgr.Instance.ShowModSelectAction(list, list2);
		for (int j = 0; j < selectMod.Length; j++)
		{
			selectMod[j] = 0;
		}
		m_canSelect = false;
		zbCombatSystem.Instance.combatRecorder.Save();
		zbCombatWnd zbCombatWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
		if ((Object)(object)zbCombatWnd2 != (Object)null)
		{
			zbCombatWnd2.refreshSkill();
		}
	}

	protected override void UpdateAtShow()
	{
		if (allFinishs.Count((bool n) => n) >= 4 && !actionAnimation)
		{
			actionAnimation = true;
			node_bg.visible = true;
			((VisualElement)m_btnRefresh).visible = true;
			((VisualElement)spineAward).visible = true;
			spineAward.AnimationState.SetAnimation(0, "animation", false);
			RefreshModDetail();
			if (Object.op_Implicit((Object)(object)m_audio))
			{
				zbAudioMgr.Instance.StopAudio(m_audio);
				m_audio = null;
			}
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
		}
	}

	private IEnumerator SpinReel(zbModWheelScroll scrollItem, float duration, int index)
	{
		if (duration > 0f)
		{
			yield return (object)new WaitForSecondsRealtime(duration);
		}
		float actionTime = 0f;
		float speed;
		Translate value;
		Length y;
		do
		{
			actionTime += Time.unscaledDeltaTime;
			StyleTranslate translate = scrollItem.root.style.translate;
			speed = 1500f;
			value = ((StyleTranslate)(ref translate)).value;
			y = ((Translate)(ref value)).y;
			float num = ((Length)(ref y)).value + Time.unscaledDeltaTime * speed;
			if (num >= iconY[1])
			{
				scrollItem.root.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(iconY[6] + (num - iconY[1]))));
			}
			else
			{
				scrollItem.root.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(num)));
			}
			yield return null;
		}
		while (!(actionTime > wheelRunTime) && !skipFlag);
		StyleTranslate translate2 = scrollItem.root.style.translate;
		value = ((StyleTranslate)(ref translate2)).value;
		y = ((Translate)(ref value)).y;
		int selectIndex;
		if (((Length)(ref y)).value > iconY[3])
		{
			scrollItem.RefreshIcon(5, selectMod[index]);
			selectIndex = 5;
		}
		else
		{
			scrollItem.RefreshIcon(2, selectMod[index]);
			selectIndex = 2;
		}
		while (true)
		{
			actionTime += Time.unscaledDeltaTime;
			StyleTranslate translate3 = scrollItem.root.style.translate;
			speed = Math.Max(Math.Min(speed - Time.unscaledDeltaTime * 1000f, 1500f), 200f);
			value = ((StyleTranslate)(ref translate3)).value;
			y = ((Translate)(ref value)).y;
			float num2 = ((Length)(ref y)).value + Time.unscaledDeltaTime * speed;
			value = ((StyleTranslate)(ref translate3)).value;
			y = ((Translate)(ref value)).y;
			if (((Length)(ref y)).value <= iconY[selectIndex] && num2 >= iconY[selectIndex])
			{
				break;
			}
			if (num2 >= iconY[1])
			{
				scrollItem.root.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(iconY[6] + (num2 - iconY[1]))));
			}
			else
			{
				scrollItem.root.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(num2)));
			}
			yield return null;
		}
		scrollItem.root.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(iconY[selectIndex])));
		allFinishs[index] = true;
	}

	private IEnumerator OtherIEnumerator()
	{
		yield return (object)new WaitForSecondsRealtime(0.3f);
		((VisualElement)m_btnTouch).visible = true;
	}

	public void RefreshRestoreData(zbCombatTeam team, stCurrModWheel curModWheel)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		monsterTid = curModWheel.monsterTid;
		m_masterTeam = team;
		selectMod = curModWheel.selectMod.ToArray();
		monsterRecord = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid);
		((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbCombatSystem.Instance.refreshWheelNum <= 0));
		zbCombatSystem.Instance.Pause();
		zbAudioMgr.Instance.PauseAllAudio();
		((VisualElement)m_btnTouch).visible = true;
		for (int i = 0; i < 4; i++)
		{
			m_ModScrolls[i].RefreshDetail(m_masterTeam);
			m_ModScrolls[i].RefreshIcon(3, selectMod[i]);
		}
		RefreshModDetail();
		RefreshAuto();
		zbCombatSystem.Instance.combatRecorder.NeedModWheelSave(curModWheel.monsterTid, curModWheel.selectMod);
	}
}
