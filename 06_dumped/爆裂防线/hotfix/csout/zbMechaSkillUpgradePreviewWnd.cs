using System;
using System.Linq;
using UnityEngine.UIElements;
using zbType;

public class zbMechaSkillUpgradePreviewWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private ScrollView m_scrollView;

	public int m_id;

	public EPreviewType m_type;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.horizontalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(int id)
	{
		m_id = id;
		UpdateListView();
	}

	private void UpdateListView()
	{
		zbMechaCsvRecord recordByKey = zbDataConfig.Instance.mechaCsvTable.GetRecordByKey(m_id);
		if (recordByKey != null)
		{
			int[] array = recordByKey.skillList.ToArray();
			((VisualElement)m_scrollView).Clear();
			int[] array2 = array;
			foreach (int skillId in array2)
			{
				zbMechaSkillUpgradePreviewItem zbMechaSkillUpgradePreviewItem2 = new zbMechaSkillUpgradePreviewItem(this);
				zbMechaSkillUpgradePreviewItem2.Init(skillId);
				((VisualElement)m_scrollView).Add((VisualElement)(object)zbMechaSkillUpgradePreviewItem2);
			}
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
