using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbTilibankItem : zbViewBaseItem
{
	private VisualElement m_nodeTime;

	private VisualElement m_nodeGray;

	private Label m_lbltime;

	private Label m_lbltili;

	private VisualElement m_btntouch;

	private TiliBankDetail m_tiliBankDetail1;

	public zbTilibankItem(zbBaseWnd baseWnd)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/activity/tb_tilibankitem.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.CloneTree());
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		m_nodeTime = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "timeroot", (string)null);
		m_lbltime = UQueryExtensions.Q<Label>(m_nodeTime, "lbltime", (string)null);
		m_lbltili = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lblget", (string)null);
		m_btntouch = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "root", (string)null);
		m_nodeGray = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "nodegray", (string)null);
		baseWnd.m_uiEvent.RegisterCallback<ClickEvent>(m_btntouch, (Action)OnTouch, "", (TrickleDown)0);
	}

	public void Init(TiliBankDetail tiliBankDetail)
	{
		m_tiliBankDetail1 = tiliBankDetail;
		if (m_tiliBankDetail1 != null)
		{
			((TextElement)m_lbltili).text = $"+{tiliBankDetail.TiliCount}";
			RefreshLeftTime();
		}
	}

	private void RefreshLeftTime()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (m_tiliBankDetail1 != null)
		{
			DisplayStyle val = (DisplayStyle)1;
			if (m_tiliBankDetail1.TiliTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				val = (DisplayStyle)0;
				string text = zbUtil.FormatTimeHHMMSS(m_tiliBankDetail1.TiliTm - zbSelf.Instance.timeHandler.GetServerTime());
				((TextElement)m_lbltime).text = text;
			}
			m_nodeTime.style.display = StyleEnum<DisplayStyle>.op_Implicit(val);
			m_nodeGray.style.display = StyleEnum<DisplayStyle>.op_Implicit(val);
		}
	}

	private void OnTouch()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		if (m_tiliBankDetail1 != null && (int)m_nodeTime.resolvedStyle.display == 1)
		{
			zbWsMsgHandler.Instance.Activity_TiliBankAward_Req(m_tiliBankDetail1.TiliTm);
		}
	}

	public void UpdatePerSecond()
	{
		RefreshLeftTime();
	}
}
