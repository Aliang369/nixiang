using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbEquipDecomposeSelectWnd : zbBaseWnd
{
	private Button[] m_btnQualitys = (Button[])(object)new Button[5];

	private VisualElement[] m_spriteOks = (VisualElement[])(object)new VisualElement[5];

	private Button m_btnCancel;

	private Button m_btnConfirm;

	private Button m_btnClose;

	private Button m_btnTouch;

	private List<int> m_listQuality = new List<int>();

	private int m_index;

	private Action<List<EquipDetail>> m_callback;

	protected override void OnEnable()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		for (int i = 0; i < 5; i++)
		{
			m_btnQualitys[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_quality" + i, (string)null);
			m_spriteOks[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnQualitys[i], "sprite_ok", (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnQualitys[i], (Action<ClickEvent>)delegate
			{
				OnQualitys(curIndex);
			}, "", (TrickleDown)0);
			if (i > 0)
			{
				m_spriteOks[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				m_spriteOks[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
		}
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(int index, Action<List<EquipDetail>> callback)
	{
		m_index = index;
		m_callback = callback;
	}

	private void OnQualitys(int index)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_spriteOks[index].style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spriteOks[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
	}

	private void OnConfirm()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_listQuality.Clear();
		for (int i = 0; i < 5; i++)
		{
			if (m_spriteOks[i].style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				m_listQuality.Add(i + 1);
			}
		}
		bool flag = false;
		foreach (int item in m_listQuality)
		{
			if (item > 4)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10271"), Onconfirm);
		}
		else
		{
			Onconfirm(isOk: true);
		}
	}

	private void Onconfirm(bool isOk)
	{
		if (!isOk)
		{
			return;
		}
		OnClose();
		List<EquipDetail> list = new List<EquipDetail>();
		foreach (EquipDetail equip in zbServerData.sGameData.EquipList)
		{
			if (!IsContain(equip))
			{
				continue;
			}
			foreach (int item in m_listQuality)
			{
				if (equip.Quality == (EQuality)item)
				{
					list.Add(equip);
				}
			}
		}
		m_callback?.Invoke(list);
	}

	private bool IsContain(EquipDetail equip)
	{
		if (m_index == 0)
		{
			return true;
		}
		zbEquipCsvRecord recordByKey = zbDataConfig.Instance.equipCsvTable.GetRecordByKey(equip.EquipTid);
		if (recordByKey != null && recordByKey.type == m_index)
		{
			return true;
		}
		return false;
	}

	private void OnClose()
	{
		Hide();
	}
}
