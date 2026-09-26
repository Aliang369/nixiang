using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbWorldLairPassAwardWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnAllClose;

	private zbItem[] m_items = new zbItem[5];

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnAllClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_allClose", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, $"item{i}", (string)null);
			m_items[i].OnEnable(this);
		}
		AddDark();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAllClose, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(int lairLv)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		zbWorldLairLevelCsvRecord recordByKey = zbDataConfig.Instance.worldLairLevelCsvTable.GetRecordByKey(lairLv);
		for (int i = 0; i < 5; i++)
		{
			bool flag = i < recordByKey.passAwardList.Length;
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData((EResourceType)recordByKey.passAwardList[i][0], recordByKey.passAwardList[i][1], recordByKey.passAwardList[i][2]);
				m_items[i].Refresh(data);
			}
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
