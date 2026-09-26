using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTacticItemGroup : zbViewBaseItem
{
	private zbTacticWnd m_wndParent;

	private VisualElement m_skillContent;

	private VisualElement m_skillNormal;

	private VisualElement m_skillSelected;

	private Button m_btnTouch;

	public ESkillType skillType { get; set; }

	public zbTacticItemGroup(zbTacticWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualElement val = (VisualElement)(object)zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/tactic/tb_tacticItemGroup.uxml").Instantiate();
		((VisualElement)this).Add(val);
		m_btnTouch = UQueryExtensions.Q<Button>(val, "btn_touch", (string)null);
		((CallbackEventHandler)m_btnTouch).RegisterCallback<ClickEvent>((EventCallback<ClickEvent>)OnTouch, (TrickleDown)0);
		m_skillContent = UQueryExtensions.Q<VisualElement>(val, "skill_content", (string)null);
		m_skillNormal = UQueryExtensions.Q<VisualElement>(val, "skill_normal", (string)null);
		m_skillSelected = UQueryExtensions.Q<VisualElement>(val, "skill_selected", (string)null);
		RefreshStatusToDefault();
	}

	private void OnTouch(ClickEvent evt)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent.RefreshScrollViewItemsToDefault();
		m_skillNormal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_skillSelected.style.visibility == StyleEnum<Visibility>.op_Implicit((Visibility)0))
		{
			m_skillSelected.style.visibility = StyleEnum<Visibility>.op_Implicit((Visibility)1);
			((VisualElement)this).transform.scale = Vector2.op_Implicit(new Vector2(0.8f, 0.8f));
		}
		else
		{
			m_skillSelected.style.visibility = StyleEnum<Visibility>.op_Implicit((Visibility)0);
			m_wndParent.OnScrollItemSkillSelected(this);
		}
	}

	public void OnEnable()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		string skillImagePathForSkillType = m_wndParent.GetSkillImagePathForSkillType(skillType);
		if (string.Empty != skillImagePathForSkillType)
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(skillImagePathForSkillType);
			m_skillContent.style.backgroundImage = new StyleBackground(val);
		}
	}

	public void RefreshStatusToDefault()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_skillNormal.style.visibility = StyleEnum<Visibility>.op_Implicit((Visibility)0);
		m_skillSelected.style.visibility = StyleEnum<Visibility>.op_Implicit((Visibility)1);
	}
}
