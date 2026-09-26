using System;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallSubjectRelicItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbWallSubjectRelicItem>
	{
	}

	private zbWallSubjectRelicWnd m_wndParent;

	private SpineVisualElement m_spine;

	private VisualElement m_spriteIcon;

	private VisualElement m_newRelic;

	private Button m_btnTouch;

	private zbWallSubjectRelicCsvRecord m_relicRecord;

	private bool m_isActive;

	public zbWallSubjectRelicItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/wallSubject/tb_wallSubjectRelic.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbWallSubjectRelicWnd wndParent)
	{
		m_wndParent = wndParent;
		m_spine = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_newRelic = zbUtil.CreateNewTag(m_spriteIcon);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int id)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		m_relicRecord = zbSelf.Instance.wallSubjectHandler.m_relicRecords.FirstOrDefault((zbWallSubjectRelicCsvRecord r) => r.subjectId == m_wndParent.m_data.SubjectId && r.id == id);
		if (m_relicRecord != null)
		{
			m_spine.AnimationState.SetAnimation(0, m_relicRecord.classId.ToString(), true);
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_relicRecord.icon));
			m_isActive = m_wndParent.m_activeRelics.Contains(m_relicRecord.id);
			if (m_isActive)
			{
				m_spriteIcon.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
				SkeletonExtensions.SetColor(m_spine.skeleton, Color.white);
			}
			else
			{
				m_spriteIcon.style.unityBackgroundImageTintColor = new StyleColor(Color.black);
				SkeletonExtensions.SetColor(m_spine.skeleton, Color.black);
			}
			m_newRelic.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.newTagHandler.IsNewRelic(m_relicRecord.id)));
		}
	}

	private void OnTouch()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (m_newRelic.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			m_newRelic.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (m_relicRecord != null)
		{
			zbSelf.Instance.newTagHandler.RemoveNewRelic(m_relicRecord.id);
		}
		zbUIMgr.Instance.ShowWallSubjectRelicTip(m_relicRecord, m_isActive);
	}
}
