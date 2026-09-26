using System;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbTopLeftPlayerAvatar : VisualElement
{
	public class UxmlFactory : UxmlFactory<zbTopLeftPlayerAvatar>
	{
	}

	private VisualElement m_RootElement;

	private VisualElement m_headFrame;

	private VisualElement m_head;

	private Label m_lbLevel;

	private VisualElement m_progress;

	private Button m_btnHead;

	private zbUIEvent m_uiEvent;

	private Label m_lbName;

	private VisualElement m_notifyHead;

	public zbTopLeftPlayerAvatar()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/topLeftPlayerAvatar.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		m_uiEvent = wndParent.m_uiEvent;
		m_RootElement = wndParent.GetRootElement();
		m_headFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "headFrame", (string)null);
		m_head = UQueryExtensions.Q<VisualElement>(m_headFrame, "head", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_headFrame, "lb_level", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_headFrame, "progress", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>(m_headFrame, "btn_head", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_headFrame, "lb_name", (string)null);
		m_notifyHead = zbUtil.CreateNotifyTag(m_headFrame);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action<ClickEvent>)OnHead, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(zbSelf.Instance.GetHeadId());
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(playerHeadTex);
		if ((Object)(object)val != (Object)null)
		{
			m_head.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_head.MarkDirtyRepaint();
		}
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(zbSelf.Instance.GetHeadFrameId());
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(playerHeadFrameTex);
		if ((Object)(object)val2 != (Object)null)
		{
			m_headFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_headFrame.MarkDirtyRepaint();
		}
		((TextElement)m_lbLevel).text = zbSelf.Instance.GetLevel().ToString();
		zbPlayerUpgradeCsvRecord recordByKey = zbDataConfig.Instance.playerUpgradeCsvTable.GetRecordByKey(zbSelf.Instance.GetLevel() + 1);
		if (recordByKey != null)
		{
			float num = Mathf.Clamp((float)zbServerData.sGameData.RoleCommonInfo.RoleExp / (float)recordByKey.exp, 0f, 1f);
			m_progress.style.width = StyleLength.op_Implicit(new Length(num * 96f, (LengthUnit)0));
		}
		else
		{
			m_progress.style.width = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
		}
		((TextElement)m_lbName).text = zbSelf.Instance.GetRoleName().ToString();
	}

	public void OnHead(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowSelf();
	}

	public void UpdateRedPoint()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		m_notifyHead.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbServerData.sGameData.RoleCommonInfo.RoleNameChange != 0 && (!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Head) || zbServerData.sGameData.MainQuestInfo.MainLevel < 4)) ? 1 : 0));
	}
}
