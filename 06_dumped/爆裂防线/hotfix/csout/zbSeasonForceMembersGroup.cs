using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonForceMembersGroup : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbSeasonForceMembersGroup>
	{
	}

	private zbSeasonForceMembersWnd m_wndParent;

	private VisualElement m_spriteOffice;

	private Label m_lbOffice;

	private Label m_lbCount;

	private Button m_btnArrow;

	private VisualElement m_content;

	private bool m_expanded = true;

	private ESeasonOffice m_office;

	public zbSeasonForceMembersGroup()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/tb_seasonForceMembersGroup.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbSeasonForceMembersWnd wndParent, ESeasonOffice office)
	{
		m_wndParent = wndParent;
		m_office = office;
		m_spriteOffice = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_office", (string)null);
		m_lbOffice = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_office", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_btnArrow = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_arrow", (string)null);
		m_content = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "content", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnArrow, (Action)OnArrow, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		m_spriteOffice.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/season/season_r{(int)m_office}.png"));
		RefreshList();
		RefreshExpanded();
	}

	private void RefreshList()
	{
		m_content.Clear();
		int num = 0;
		foreach (SeasonMemberInfo member in zbSelf.Instance.seasonHandler.memberList)
		{
			if (member.Office == m_office)
			{
				zbSeasonForceMembersItem zbSeasonForceMembersItem2 = new zbSeasonForceMembersItem();
				m_content.Add((VisualElement)(object)zbSeasonForceMembersItem2);
				zbSeasonForceMembersItem2.Refresh(member);
				num++;
			}
		}
		((TextElement)m_lbCount).text = num.ToString();
	}

	private void RefreshExpanded()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (m_expanded)
		{
			((VisualElement)m_btnArrow).style.scale = StyleScale.op_Implicit(new Vector2(1f, -1f));
			m_content.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((VisualElement)m_btnArrow).style.scale = StyleScale.op_Implicit(new Vector2(1f, 1f));
			m_content.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void OnArrow()
	{
		m_expanded = !m_expanded;
		RefreshExpanded();
	}
}
