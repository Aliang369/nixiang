using Spine.Unity;
using UnityEngine.UIElements;

public class zbLimitTimeTreasureGrandPrizeItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbLimitTimeTreasureGrandPrizeItem>
	{
	}

	private zbLimitTimeTreasureWnd m_wndParent;

	private zbItem m_item = new zbItem();

	private SpineVisualElement m_spineEffect;

	public zbLimitTimeTreasureGrandPrizeItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_limitTimeTreasureGrandPrize.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbLimitTimeTreasureWnd wndParent)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_spineEffect = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine_effect", (string)null);
		((VisualElement)m_spineEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbItemData zbItemData)
	{
		m_item.Refresh(zbItemData);
	}

	public void PlayEffect()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_spineEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spineEffect.AnimationState.SetAnimation(0, "enter", false);
	}

	public void PlayResult(bool needPlayAudio)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_spineEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spineEffect.AnimationState.SetAnimation(0, "enter2", false);
		m_spineEffect.AnimationState.AddAnimation(0, "loop", true, 0f);
		if (needPlayAudio)
		{
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
		}
	}

	public void ClearResult()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_spineEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}
}
