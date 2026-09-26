using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbZhanLingKillTargetItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbZhanLingKillTargetItem>
	{
	}

	private zbItem[] m_items = new zbItem[5];

	private Label m_lbDesc;

	private Label m_lbProgress;

	private VisualElement m_spriteProgress;

	private Button m_btnGoto;

	private VisualElement m_notify;

	private JourneyZhanLingDetail m_journeyOneZhanLing;

	private ZhanLingKillDetail m_operateZhanLing;

	public zbZhanLingKillTargetItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_zhanLingKillTarget.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(wndParent);
		}
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_progress", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_goto", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action)OnGoto, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGoto, (Vector2?)new Vector2(5f, 5f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (zbServerData.sGameData.MainQuestInfo.MainLevel < 5 || zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Fail)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		m_journeyOneZhanLing = zbSelf.Instance.journeyOneHandler.GetJourneyOneZhanLingDetail();
		m_operateZhanLing = zbSelf.Instance.operateHandler.GetZhanLingKillDetailByUUID();
		if (m_journeyOneZhanLing != null)
		{
			RefreshJourneyOneZhanLing();
		}
		else if (m_operateZhanLing != null)
		{
			RefreshZhanLingKill();
		}
		else
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void RefreshJourneyOneZhanLing()
	{
		int num = zbDataConfig.Instance.chapterOneZhanLingCsvTable.GetRecords().records.Values.Max((zbChapterOneZhanLingCsvRecord r) => r.level);
		zbChapterOneZhanLingCsvRecord recordByKey = zbDataConfig.Instance.chapterOneZhanLingCsvTable.GetRecordByKey(m_journeyOneZhanLing.Level);
		if (m_journeyOneZhanLing.Level < num)
		{
			recordByKey = zbDataConfig.Instance.chapterOneZhanLingCsvTable.GetRecordByKey(m_journeyOneZhanLing.Level + 1);
		}
		if (recordByKey != null)
		{
			List<zbItemData> list = new List<zbItemData>();
			list.Add(new zbItemData((EResourceType)recordByKey.lowAward[0], recordByKey.lowAward[1], recordByKey.lowAward[2]));
			list.Add(new zbItemData((EResourceType)recordByKey.mediumAward[0], recordByKey.mediumAward[1], recordByKey.mediumAward[2]));
			list.Add(new zbItemData((EResourceType)recordByKey.highAward[0], recordByKey.highAward[1], recordByKey.highAward[2]));
			RefreshDetail(m_journeyOneZhanLing.Level >= num, list.ToArray(), m_journeyOneZhanLing.Reach, recordByKey.killCount, zbSelf.Instance.journeyOneHandler.HasZhanlingRedPoint());
		}
	}

	private void RefreshZhanLingKill()
	{
		int num = zbDataConfig.Instance.zhanLingKillTargetCsvTable.GetRecords().records.Values.Max((zbZhanLingKillTargetCsvRecord r) => r.lv);
		zbZhanLingKillTargetCsvRecord recordByKey = zbDataConfig.Instance.zhanLingKillTargetCsvTable.GetRecordByKey(m_operateZhanLing.ZhanLingInfo.Level);
		if (m_operateZhanLing.ZhanLingInfo.Level < num)
		{
			recordByKey = zbDataConfig.Instance.zhanLingKillTargetCsvTable.GetRecordByKey(m_operateZhanLing.ZhanLingInfo.Level + 1);
		}
		if (recordByKey == null)
		{
			return;
		}
		List<zbItemData> list = new List<zbItemData>();
		int[][] lowAward = recordByKey.lowAward;
		foreach (int[] array in lowAward)
		{
			list.Add(new zbItemData((EResourceType)array[0], array[1], array[2]));
		}
		lowAward = recordByKey.highAward;
		foreach (int[] array2 in lowAward)
		{
			list.Add(new zbItemData((EResourceType)array2[0], array2[1], array2[2]));
		}
		foreach (OperateZhanLingExtraDetail grandExtra in m_operateZhanLing.GrandExtraList)
		{
			if (grandExtra.Level == recordByKey.lv)
			{
				list.Add(new zbItemData(grandExtra.Award.Type, grandExtra.Award.Tid, grandExtra.Award.Count));
			}
		}
		RefreshDetail(m_operateZhanLing.ZhanLingInfo.Level >= num, list.ToArray(), m_operateZhanLing.ZhanLingInfo.Reach, recordByKey.kill, zbSelf.Instance.operateHandler.HasZhanLingKillNotify(m_operateZhanLing.BaseInfo.UUID));
	}

	private void RefreshDetail(bool isMax, zbItemData[] awardList, int reach, int target, bool isNotify)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		if (isMax && !isNotify)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((TextElement)m_lbProgress).text = $"{reach}/{target}";
		float num = Mathf.Min((float)reach / (float)target, 1f);
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num * 200f, (LengthUnit)0));
		((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.11095"), target);
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num2 = 0;
		foreach (zbItemData data in awardList)
		{
			((VisualElement)m_items[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_items[num2].Refresh(data);
			num2++;
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!isNotify));
	}

	public void OnGoto()
	{
		if (m_journeyOneZhanLing != null || m_operateZhanLing != null)
		{
			zbUIMgr.Instance.HideWnd(EWndId.CombatBalance);
			zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
			if (m_journeyOneZhanLing != null)
			{
				zbUIMgr.Instance.ShowJourneyOneZhanling();
			}
			else if (m_operateZhanLing != null)
			{
				zbUIMgr.Instance.ShowZhanLingKill(m_operateZhanLing);
			}
		}
	}
}
