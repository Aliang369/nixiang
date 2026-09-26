using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbAuxResourceConvertWnd : zbBaseWnd
{
	private zbItem m_item0;

	private zbItem m_item1;

	private Button m_btnConfirm;

	private zbResourceConvertCsvRecord m_record;

	private int m_count;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_item0 = UQueryExtensions.Q<zbItem>(m_RootElement, "item0", (string)null);
		m_item0.OnEnable(this);
		m_item1 = UQueryExtensions.Q<zbItem>(m_RootElement, "item1", (string)null);
		m_item1.OnEnable(this);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(zbResourceConvertCsvRecord record, int count)
	{
		m_record = record;
		m_count = count;
		if (record != null)
		{
			m_item0.Refresh(new zbItemData((EResourceType)record.resType, record.resId, record.resCount * count));
			m_item1.Refresh(new zbItemData((EResourceType)record.convertType, record.convertId, record.convertCount * count));
		}
	}

	private void OnClose()
	{
		if (m_record != null)
		{
			zbWsMsgHandler.Instance.Aux_ResourceConvert_Req(m_record.id, m_count);
		}
		Hide();
	}
}
