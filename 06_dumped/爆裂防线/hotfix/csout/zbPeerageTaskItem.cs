using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

internal class zbPeerageTaskItem : zbViewBaseItem
{
	private zbItem m_item;

	private Label m_lbDesc;

	private Label m_lbProval;

	private Label m_lbVal;

	private Button m_btnTo;

	private Button m_btnReceive;

	private TaskDetail m_data;

	private VisualElement m_iconDone;

	private VisualElement m_mask;

	private zbTaskTypeCsvRecord m_taskTypeRecord;

	public zbPeerageTaskItem(VisualTreeAsset tAssets, zbPeerageWnd wndParent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAssets.Instantiate());
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_lbProval = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_proVal", (string)null);
		m_lbVal = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_val", (string)null);
		m_btnTo = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_to", (string)null);
		m_btnReceive = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_receive", (string)null);
		m_iconDone = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_done", (string)null);
		m_mask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "mask", (string)null);
		m_item.OnEnable(wndParent);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTo, (Action)OnGoTo, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReceive, (Action)OnReceive, "", (TrickleDown)0);
	}

	public void Init(zbPeerageWnd wndParent, TaskDetail data)
	{
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		zbPeerageTaskCsvRecord recordByKey = zbDataConfig.Instance.peerageTaskCsvTable.GetRecordByKey(data.TaskId);
		m_taskTypeRecord = zbDataConfig.Instance.taskTypeCsvTable.GetRecordByKey(data.TaskType);
		int[] array = recordByKey.awardList[0];
		zbItemData data2 = new zbItemData((EResourceType)array[0], array[1], array[2]);
		m_item.Refresh(data2);
		string desc = m_taskTypeRecord.desc;
		int target = recordByKey.target;
		if (target > 0)
		{
			if (recordByKey.type == 113)
			{
				string name = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(target).name;
				((TextElement)m_lbDesc).text = string.Format(desc, name);
			}
			else if (recordByKey.type == 118 || recordByKey.type == 142 || recordByKey.type == 119)
			{
				string arg = zbUtil.Language(zbDataConfig.Instance.qualityCsvTable.GetRecordByKey(recordByKey.target).name);
				((TextElement)m_lbDesc).text = string.Format(desc, arg);
			}
			else
			{
				((TextElement)m_lbDesc).text = string.Format(desc, target);
			}
		}
		else
		{
			((TextElement)m_lbDesc).text = desc;
		}
		bool flag = data.State == EActiveState.Active;
		bool flag2 = data.State == EActiveState.Done;
		m_mask.visible = flag2;
		((VisualElement)m_btnTo).visible = !flag && !flag2;
		((VisualElement)m_btnReceive).visible = flag && !flag2;
		m_iconDone.visible = flag2 && !flag;
		((TextElement)m_lbVal).text = $"{data.Reach} / {data.Target}";
		((VisualElement)m_lbProval).style.width = StyleLength.op_Implicit(new Length(flag ? 100f : Mathf.Min((float)data.Reach / (float)data.Target * 100f, 100f), (LengthUnit)1));
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void OnReceive()
	{
		zbWsMsgHandler.Instance.Peerage_UpgradeTask_Req(m_data.TaskId);
	}

	private void OnGoTo()
	{
		zbProgressEventHelper.Instance.SetProgressTrack(615);
		zbLinkHelper.OpenLink(m_taskTypeRecord.link, m_data.Param);
	}
}
