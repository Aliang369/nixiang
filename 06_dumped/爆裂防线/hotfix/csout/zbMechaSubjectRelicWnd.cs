using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaSubjectRelicWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbTime;

	private Label m_lbTip;

	private Label m_lbTarget;

	private Label m_lbTargetDesc;

	private VisualElement m_spriteProgress;

	private VisualElement[] m_spriteSkills = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbTargets = (Label[])(object)new Label[4];

	private Button m_btnTarget;

	private ListView m_listView;

	private Button m_btnClose;

	private zbMechaSubjectRelicCsvRecord[] m_records;

	private int[][] m_groupList;

	public MechaSubjectDetail m_data;

	public HashSet<int> m_activeRelics = new HashSet<int>();

	private zbMechaSubjectCsvRecord m_record;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_RootElement, "lb_tip", (string)null);
		m_lbTarget = UQueryExtensions.Q<Label>(m_RootElement, "lb_target", (string)null);
		m_lbTargetDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_targetDesc", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_spriteSkills[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill" + i, (string)null);
			m_lbTargets[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_target" + i, (string)null);
		}
		m_btnTarget = UQueryExtensions.Q<Button>(m_RootElement, "btn_target", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTarget, (Action<ClickEvent>)OnTarget, "", (TrickleDown)0);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		InitRecords();
	}

	private void InitRecords()
	{
		m_records = zbDataConfig.Instance.mechaSubjectRelicCsvTable.GetRecords().records;
		m_groupList = GroupByFive(m_records.Select((zbMechaSubjectRelicCsvRecord r) => r.id).ToArray());
		RefreshItmes();
	}

	public void RefreshCustom(MechaSubjectDetail data)
	{
		if (data == null)
		{
			return;
		}
		m_data = data;
		m_activeRelics.Clear();
		foreach (int relic in data.BarInfo.RelicList)
		{
			m_activeRelics.Add(relic);
		}
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		RefreshTarget();
		RefreshTime();
	}

	public int[][] GroupByFive(int[] relicList)
	{
		if (relicList == null)
		{
			return Array.Empty<int[]>();
		}
		int num = relicList.Length;
		int num2 = (num + 4) / 5;
		int[][] array = new int[num2][];
		for (int i = 0; i < num2; i++)
		{
			int num3 = i * 5;
			int num4 = Math.Min(5, num - num3);
			array[i] = new int[num4];
			Array.Copy(relicList, num3, array[i], 0, num4);
		}
		return array;
	}

	private void RefreshItmes()
	{
		m_listView.makeItem = delegate
		{
			zbMechaSubjectRelicCell obj = new zbMechaSubjectRelicCell();
			obj.OnEnable(this);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbMechaSubjectRelicCell).Refresh(m_groupList[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_groupList;
	}

	private void RefreshTarget()
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbTarget).text = m_data.BarInfo.RelicList.Count.ToString();
		m_record = zbDataConfig.Instance.mechaSubjectCsvTable.GetRecordByKey(m_data.SubjectId);
		if (m_record == null)
		{
			return;
		}
		zbMechaCsvRecord recordByKey = zbDataConfig.Instance.mechaCsvTable.GetRecordByKey(m_record.mechaId);
		if (recordByKey != null)
		{
			List<int> list = new List<int>();
			list.Add(recordByKey.skill2);
			list.Add(recordByKey.skill3);
			list.Add(recordByKey.skill4);
			list.Add(recordByKey.skill5);
			for (int i = 0; i < 4; i++)
			{
				zbMechaSkillCsvRecord recordByKey2 = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(list[i]);
				m_spriteSkills[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon));
				zbUtil.SetElementGray(m_spriteSkills[i], zbSelf.Instance.mechaSubjectHandler.m_skillUnlock[i] > m_data.BarInfo.RelicList.Count);
				((TextElement)m_lbTargets[i]).text = zbSelf.Instance.mechaSubjectHandler.m_skillUnlock[i].ToString();
			}
			m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(GetProgress() * 536f, (LengthUnit)0));
		}
	}

	private float GetProgress()
	{
		int num = 4;
		float num2 = 1f / (float)num;
		for (int i = 0; i < num; i++)
		{
			int num3 = zbSelf.Instance.mechaSubjectHandler.m_skillUnlock[i];
			if (m_data.BarInfo.RelicList.Count < num3)
			{
				int num4 = ((i != 0) ? zbSelf.Instance.mechaSubjectHandler.m_skillUnlock[i - 1] : 0);
				float num5 = (float)(m_data.BarInfo.RelicList.Count - num4) / (float)(num3 - num4);
				return Mathf.Clamp01((float)i * num2 + num5 * num2);
			}
		}
		return 1f;
	}

	public void OnTarget(ClickEvent evt)
	{
		if (m_record != null)
		{
			zbUIMgr.Instance.ShowMechaSkillUpgradePreview(m_record.mechaId);
		}
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	public void OnClose(ClickEvent evt)
	{
		zbSelf.Instance.newTagHandler.RemoveAllNewMechaRelics();
		Hide();
	}
}
