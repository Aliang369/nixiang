using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMonsterItem : VisualElement
{
	private Label m_lbName;

	private SpineVisualElement m_spineMonster;

	private Button m_btnTouch;

	private VisualElement m_yingzi;

	private VisualElement m_redDot;

	private Button m_btnType;

	private zbEncyclopediaWnd m_wndParent;

	private zbHandbookCsvRecord m_record;

	private zbMonsterAvatarCsvRecord m_avatarRecord;

	private MonsterHandbookDetail m_monsterDetail;

	private bool m_isUnlocked;

	private bool m_isBoss;

	private static readonly Queue<zbMonsterItem> s_pendingSpineInits = new Queue<zbMonsterItem>();

	private static int s_lastPumpFrame = -1;

	private const float kSpineInitFrameBudgetMs = 8f;

	private const long kSpinePumpIntervalMs = 16L;

	private IVisualElementScheduledItem m_pumpItem;

	private bool m_isSpineQueued;

	private const long kVisibilityPollIntervalMs = 100L;

	private const float kViewportCullMargin = 60f;

	private ScrollView m_scrollView;

	private bool m_isOutsideViewport;

	public zbMonsterItem(zbEncyclopediaWnd wndParent, bool isBoss = false)
	{
		m_wndParent = wndParent;
		m_isBoss = isBoss;
		string resName = (isBoss ? "ui/uxml/sub/tb_monsterBossItem.uxml" : "ui/uxml/sub/tb_monster.uxml");
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>(resName);
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_spineMonster = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine_monster", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_yingzi = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "yingzi", (string)null);
		m_redDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "red_dot", (string)null);
		m_btnType = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_type", (string)null);
		((VisualElement)m_btnType).pickingMode = (PickingMode)1;
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_spineMonster.pendingAssetUrl = null;
		m_spineMonster.skeletonDataAsset = null;
		((VisualElement)this).schedule.Execute((Action)UpdateSpineVisibility).Every(100L);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbHandbookCsvRecord record)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_isUnlocked = IsMonsterUnlocked(record.id, out var detail);
		m_monsterDetail = detail;
		m_avatarRecord = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(m_avatarRecord.shadow);
		m_yingzi.style.backgroundImage = new StyleBackground(val);
		if (m_record.elementType == 0)
		{
			((VisualElement)m_btnType).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_btnType).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnType).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/monster/monster_type{m_record.elementType}.png"));
		}
		RefreshMonsterStatus(m_monsterDetail);
		ScheduleSpineInit();
	}

	private void RefreshMonsterScale()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		float num = ((m_record.bodyScale > 0f) ? m_record.bodyScale : 1f);
		float num2 = (m_isBoss ? 0.5f : 1f);
		float num3 = Mathf.Max(num * num2, 0.01f);
		if (m_record.id == 84 || m_record.id == 85)
		{
			((VisualElement)m_spineMonster).style.scale = StyleScale.op_Implicit(new Scale(new Vector2(num3 * 0.6f, num3 * 0.6f)));
		}
		else
		{
			((VisualElement)m_spineMonster).style.scale = StyleScale.op_Implicit(new Scale(new Vector2(num3, num3)));
		}
		m_yingzi.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(num3, num3)));
	}

	private void ScheduleSpineInit()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		RefreshMonsterScale();
		string text = m_avatarRecord.fightBone + "_SkeletonData.asset";
		if (m_spineMonster.userCacheAssetName != text)
		{
			((VisualElement)m_spineMonster).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (!m_isSpineQueued)
		{
			m_isSpineQueued = true;
			s_pendingSpineInits.Enqueue(this);
		}
		if (m_pumpItem == null)
		{
			m_pumpItem = ((VisualElement)this).schedule.Execute((Action)PumpSpineInitQueue).Every(16L);
		}
		else
		{
			m_pumpItem.Resume();
		}
	}

	private void PumpSpineInitQueue()
	{
		if (s_lastPumpFrame == Time.frameCount)
		{
			return;
		}
		s_lastPumpFrame = Time.frameCount;
		float num = Time.realtimeSinceStartup * 1000f;
		while (s_pendingSpineInits.Count > 0)
		{
			zbMonsterItem zbMonsterItem2 = s_pendingSpineInits.Dequeue();
			zbMonsterItem2.m_isSpineQueued = false;
			if (((VisualElement)zbMonsterItem2).panel != null)
			{
				zbMonsterItem2.DoSpineInit();
				if (Time.realtimeSinceStartup * 1000f - num >= 8f)
				{
					break;
				}
			}
		}
		if (s_pendingSpineInits.Count == 0 && m_pumpItem != null)
		{
			m_pumpItem.Pause();
		}
	}

	private void DoSpineInit()
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		zbMonsterAvatarCsvRecord recordByKey = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		zbUtil.RefreshSpineUI(m_spineMonster, recordByKey.fightBone + "_SkeletonData.asset");
		m_isOutsideViewport = ComputeOutsideViewport();
		if (!m_isUnlocked)
		{
			if (m_spineMonster.IsValid)
			{
				m_spineMonster.AnimationState.ClearTracks();
				m_spineMonster.timeScale = 0f;
				SkeletonExtensions.SetColor(m_spineMonster.skeleton, Color.black);
				m_spineMonster.rendererElement.MarkDirtyRepaint();
			}
		}
		else if (m_spineMonster.IsValid)
		{
			m_spineMonster.timeScale = 1f;
			m_spineMonster.AnimationState.SetAnimation(0, "move", true);
			SkeletonExtensions.SetColor(m_spineMonster.skeleton, Color.white);
		}
		ApplySpineFreeze();
		((VisualElement)m_spineMonster).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private void ApplySpineFreeze()
	{
		m_spineMonster.freeze = !m_isUnlocked || m_isOutsideViewport;
	}

	private void UpdateSpineVisibility()
	{
		if (m_isUnlocked && m_spineMonster.IsValid)
		{
			bool flag = ComputeOutsideViewport();
			if (flag != m_isOutsideViewport)
			{
				m_isOutsideViewport = flag;
				ApplySpineFreeze();
			}
		}
	}

	private bool ComputeOutsideViewport()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		if (m_scrollView == null)
		{
			m_scrollView = ((VisualElement)this).GetFirstAncestorOfType<ScrollView>();
			if (m_scrollView == null)
			{
				return false;
			}
		}
		Rect worldBound = m_scrollView.contentViewport.worldBound;
		Rect worldBound2 = ((VisualElement)m_spineMonster).worldBound;
		if (((Rect)(ref worldBound)).width <= 0f || ((Rect)(ref worldBound)).height <= 0f || ((Rect)(ref worldBound2)).width <= 0f || ((Rect)(ref worldBound2)).height <= 0f)
		{
			return false;
		}
		((Rect)(ref worldBound))._002Ector(((Rect)(ref worldBound)).x - 60f, ((Rect)(ref worldBound)).y - 60f, ((Rect)(ref worldBound)).width + 120f, ((Rect)(ref worldBound)).height + 120f);
		return !((Rect)(ref worldBound2)).Overlaps(worldBound);
	}

	private void RefreshMonsterStatus(MonsterHandbookDetail detail)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (m_isUnlocked)
		{
			((TextElement)m_lbName).text = m_record.name;
			((VisualElement)m_btnType).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_record.elementType <= 0));
			m_redDot.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((detail.Level != 0) ? 1 : 0));
		}
		else
		{
			((TextElement)m_lbName).text = "???";
			((VisualElement)m_btnType).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_redDot.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	public bool IsMonsterUnlocked(int handbookId, out MonsterHandbookDetail detail)
	{
		RepeatedField<MonsterHandbookDetail> monsterList = zbServerData.sGameData.HandbookInfo.MonsterList;
		detail = null;
		if (monsterList == null)
		{
			return false;
		}
		foreach (MonsterHandbookDetail item in monsterList)
		{
			if (item.HandbookId == handbookId)
			{
				detail = item;
				return true;
			}
		}
		return false;
	}

	public void OnTouch(ClickEvent evt)
	{
		if (!m_isUnlocked)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10146"));
		}
		else if (m_monsterDetail != null && (Object)(object)m_wndParent != (Object)null && m_record != null)
		{
			zbUIMgr.Instance.ShowMonsterDetail(m_record, m_monsterDetail);
		}
	}
}
