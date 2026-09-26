using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldEventFightWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbName;

	private Label m_lbDesc;

	private VisualElement m_monsterPreview;

	private Label m_lbAwardTitle;

	private VisualElement m_awardList;

	private Button m_btnCancel;

	private Button m_btnConfirm;

	private zbWorldEventFightCsvRecord m_record;

	private zbItem[] m_awards = new zbItem[4];

	private WorldEventDetail m_eventDetail;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_monsterPreview = UQueryExtensions.Q<VisualElement>(m_RootElement, "monsterPreview", (string)null);
		m_lbAwardTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_awardTitle", (string)null);
		m_awardList = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardList", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>(m_awardList, $"award{i}", (string)null);
			m_awards[i].OnEnable(this);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_CommFight_Ack, OnCommFightAck);
	}

	public void RefreshCustom(WorldEventDetail eventDetail)
	{
		m_eventDetail = eventDetail;
		InitFight();
		InitMonster();
	}

	private void InitMonster()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<int, int> dictionary = new Dictionary<int, int>
		{
			[0] = m_record.mst1,
			[1] = m_record.mst2,
			[2] = m_record.mst3
		};
		for (int i = 0; i < 3; i++)
		{
			SpineVisualElement val = UQueryExtensions.Q<SpineVisualElement>(m_monsterPreview, $"SpineVisualElement{i}", (string)null);
			zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(dictionary[i]);
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey.avatarId).shadow);
			VisualElement obj = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val, "yingzi", (string)null);
			obj.style.scale = StyleScale.op_Implicit(new Vector2(1f, 1f));
			obj.style.scale = new StyleScale(new Vector2(recordByKey.bodyScale, recordByKey.bodyScale));
			IStyle style = UQueryExtensions.Q<VisualElement>(obj, "yingzi", (string)null).style;
			Rect rect = val2.rect;
			style.width = StyleLength.op_Implicit(((Rect)(ref rect)).width);
			IStyle style2 = UQueryExtensions.Q<VisualElement>(obj, "yingzi", (string)null).style;
			rect = val2.rect;
			style2.height = StyleLength.op_Implicit(((Rect)(ref rect)).height);
			UQueryExtensions.Q<VisualElement>(obj, "yingzi", (string)null).style.backgroundImage = new StyleBackground(val2);
			IStyle style3 = obj.style;
			rect = val2.rect;
			float height = ((Rect)(ref rect)).height;
			rect = val2.rect;
			style3.top = StyleLength.op_Implicit((height > ((Rect)(ref rect)).width) ? 54f : 115f);
			string resName = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey.avatarId).fightBone + "_SkeletonData.asset";
			SkeletonDataAsset skeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(resName);
			if (val != null)
			{
				val.skeletonDataAsset = skeletonDataAsset;
				val.Initialize(true);
				val.unscaledTime = true;
				val.AnimationState.SetAnimation(0, "idle", true);
			}
		}
	}

	private void InitFight()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		m_record = zbDataConfig.Instance.worldEventFightCsvTable.GetRecordByKey(m_eventDetail.EventId);
		((TextElement)m_lbName).text = zbUtil.Language(m_record.name);
		((TextElement)m_lbDesc).text = zbUtil.Language(m_record.desc);
		for (int i = 0; i < 4; i++)
		{
			bool flag = i < m_record.awardList.Length;
			((VisualElement)m_awards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData((EResourceType)m_record.awardList[i][0], m_record.awardList[i][1], m_record.awardList[i][2]);
				m_awards[i].Refresh(data);
			}
		}
	}

	private void OnConfirm()
	{
		zbWsMsgHandler.Instance.WorldEvent_FightStart_Req(m_eventDetail.GridId);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnCommFightAck(object obj)
	{
		OnClose();
	}
}
