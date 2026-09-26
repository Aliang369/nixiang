using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPrayLanternItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbPrayLanternItem>
	{
	}

	private zbPrayLanternWnd m_wndParent;

	private int m_index;

	private zbItem m_item = new zbItem();

	private SpineVisualElement m_spineEffect;

	private Label m_lbCount;

	private bool m_isUnlock;

	public zbPrayLanternItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_prayLantern.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbPrayLanternWnd wndParent, int index)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_index = index;
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		zbPrayLanternAwardCsvRecord recordByKey = zbDataConfig.Instance.prayLanternAwardCsvTable.GetRecordByKey(index + 1);
		if (recordByKey != null)
		{
			m_item.Refresh(new zbItemData((EResourceType)recordByKey.award[0], recordByKey.award[1], recordByKey.award[2]));
		}
		m_spineEffect = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine_effect", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		((VisualElement)m_spineEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(bool isUnlock, bool isDrawAck, bool isGray)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		m_isUnlock = isUnlock;
		((Component)m_wndParent.m_spineAwards[m_index]).gameObject.SetActive(!isUnlock);
		if (!isGray)
		{
			SkeletonExtensions.SetColor(((SkeletonRenderer)m_wndParent.m_spineAwards[m_index]).Skeleton, Color.white);
			m_wndParent.m_spineAwards[m_index].AnimationState.SetAnimation(0, "loop", true);
		}
		else
		{
			SkeletonExtensions.SetColor(((SkeletonRenderer)m_wndParent.m_spineAwards[m_index]).Skeleton, new Color(0.5f, 0.5f, 0.5f, 1f));
			m_wndParent.m_spineAwards[m_index].AnimationState.SetAnimation(0, "lock", true);
		}
		((VisualElement)m_item).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!isUnlock));
		((VisualElement)m_spineEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(isDrawAck & isUnlock)));
		bool flag = ((VisualElement)m_lbCount).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_lbCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(isDrawAck & flag)));
	}

	public void PlayEffect()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_spineEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spineEffect.AnimationState.SetAnimation(0, "enter", false);
	}

	public void PlayResult(bool needPlayAudio)
	{
		if (m_isUnlock)
		{
			m_spineEffect.AnimationState.SetAnimation(0, "enter2", false);
			m_spineEffect.AnimationState.AddAnimation(0, "loop", true, 0f);
		}
		else
		{
			((Component)m_wndParent.m_spineAwards[m_index]).gameObject.SetActive(true);
			m_wndParent.m_spineAwards[m_index].AnimationState.SetAnimation(0, "enter2", false);
			m_wndParent.m_spineAwards[m_index].AnimationState.AddAnimation(0, "loop", true, 0f);
		}
		if (needPlayAudio)
		{
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
		}
	}

	public void SetCount(int count)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(count <= 1));
		((TextElement)m_lbCount).text = "x" + count;
	}

	public void ClearResult()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_spineEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}
}
