using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class zbWorldLevelWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbCur;

	private Label m_lbNext;

	private VisualElement m_progress;

	private Label m_lbProgress;

	private Label m_lbCurLevel;

	private Label m_lbNextLevel;

	private Label[] m_lbCurRates = (Label[])(object)new Label[7];

	private Label[] m_lbNextRates = (Label[])(object)new Label[7];

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbCur = UQueryExtensions.Q<Label>(m_RootElement, "lb_cur", (string)null);
		m_lbNext = UQueryExtensions.Q<Label>(m_RootElement, "lb_next", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_RootElement, "lb_progress_value", (string)null);
		m_lbCurLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_curLevel", (string)null);
		m_lbNextLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_nextLevel", (string)null);
		for (int i = 0; i < 7; i++)
		{
			m_lbCurRates[i] = UQueryExtensions.Q<Label>(m_RootElement, $"lb_curRates{i}", (string)null);
			m_lbNextRates[i] = UQueryExtensions.Q<Label>(m_RootElement, $"lb_nextRates{i}", (string)null);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int exploredCount = zbSelf.Instance.worldHandler.GetExploredCount();
		foreach (KeyValuePair<int, zbWorldLevelCsvRecord> record in zbDataConfig.Instance.worldLevelCsvTable.GetRecords().records)
		{
			if (exploredCount >= record.Value.num)
			{
				num = record.Value.lv;
			}
		}
		((TextElement)m_lbCur).text = num.ToString();
		((TextElement)m_lbCurLevel).text = "Lv." + num;
		float[] levelRates = GetLevelRates(num);
		for (int i = 0; i < levelRates.Length; i++)
		{
			((TextElement)m_lbCurRates[i]).text = levelRates[i] + "%";
		}
		int num2 = num + 1;
		zbWorldLevelCsvRecord recordByKey = zbDataConfig.Instance.worldLevelCsvTable.GetRecordByKey(num2);
		if (recordByKey != null)
		{
			((TextElement)m_lbNext).text = num2.ToString();
			((TextElement)m_lbNextLevel).text = "Lv." + num2;
			((TextElement)m_lbProgress).text = exploredCount + "/" + recordByKey.num;
			float num3 = Math.Clamp((float)exploredCount / (float)recordByKey.num, 0f, 1f);
			m_progress.style.width = StyleLength.op_Implicit(new Length(num3 * 371f, (LengthUnit)0));
			float[] levelRates2 = GetLevelRates(num2);
			for (int j = 0; j < levelRates2.Length; j++)
			{
				((TextElement)m_lbNextRates[j]).text = levelRates2[j] + "%";
			}
		}
		else
		{
			((TextElement)m_lbNext).text = "Max";
			((TextElement)m_lbNextLevel).text = "Max";
			((TextElement)m_lbProgress).text = "Max";
			m_progress.style.width = StyleLength.op_Implicit(new Length(371f, (LengthUnit)0));
			for (int k = 0; k < levelRates.Length; k++)
			{
				((TextElement)m_lbNextRates[k]).text = "Max";
			}
		}
	}

	private float[] GetLevelRates(int level)
	{
		zbWorldLevelCsvRecord recordByKey = zbDataConfig.Instance.worldLevelCsvTable.GetRecordByKey(level);
		int[] array = new int[7] { recordByKey.weight1, recordByKey.weight2, recordByKey.weight3, recordByKey.weight4, recordByKey.weight5, recordByKey.weight6, recordByKey.weight7 };
		int num = 0;
		int[] array2 = array;
		foreach (int num2 in array2)
		{
			num += num2;
		}
		float[] array3 = new float[7];
		for (int j = 0; j < 7; j++)
		{
			double value = (double)array[j] / (double)num * 100.0;
			array3[j] = (float)Math.Round(value, 2);
		}
		return array3;
	}

	private void OnClose()
	{
		Hide();
	}
}
