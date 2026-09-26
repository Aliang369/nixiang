using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbGemDecomposeWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private zbItem[] m_items = new zbItem[2];

	private VisualElement m_arrow;

	private Label m_lbDesc;

	private Button m_btnDecompose;

	private Button m_btnClose;

	private ResourceDetail m_data;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_arrow = UQueryExtensions.Q<VisualElement>(m_RootElement, "arrow", (string)null);
		zbUtil.CreateRowMoveAction(m_arrow);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnDecompose = UQueryExtensions.Q<Button>(m_RootElement, "btn_decompose", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDecompose, (Action)OnDecompose, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Gem, OnEventUpdateGem);
	}

	public void RefreshCustom(ResourceDetail data)
	{
		m_data = data;
		m_items[0].Refresh(new zbItemData(EResourceType.Gem, data.Tid, 1L));
		JObject record = zbDataConfig.Instance.resourceBaseJsonTable.GetRecord();
		m_items[1].Refresh(new zbItemData((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)record["gemDecompose"][(object)"award"][(object)0]), Extensions.Value<int>((IEnumerable<JToken>)record["gemDecompose"][(object)"award"][(object)1]), Extensions.Value<int>((IEnumerable<JToken>)record["gemDecompose"][(object)"award"][(object)2])));
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Gem, data.Tid);
		((TextElement)m_lbDesc).text = zbUtil.Language("lang.10539") + resorceIconPath.strDesc;
	}

	private void OnDecompose()
	{
		zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(m_data.Tid);
		if (recordByKey != null && recordByKey.quality == 7)
		{
			zbUIMgr.Instance.ShowVerificationCode(zbUtil.Language("lang.10560"), zbUtil.Language("lang.10565"), delegate(bool isOK)
			{
				if (isOK)
				{
					zbWsMsgHandler.Instance.Gem_Decompose_Req(m_data.Tid);
				}
			});
		}
		else
		{
			zbWsMsgHandler.Instance.Gem_Decompose_Req(m_data.Tid);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdateGem(object data)
	{
		OnClose();
	}
}
