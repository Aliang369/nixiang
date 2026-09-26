using Spine.Unity;
using UnityEngine.UIElements;

public class zbLimitTimeTreasureItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbLimitTimeTreasureItem>
	{
	}

	private zbLimitTimeTreasureWnd m_wndParent;

	private zbItem m_item = new zbItem();

	private SpineVisualElement m_spineEffect;

	private VisualElement m_iconLight;

	public zbLimitTimeTreasureItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_limitTimeTreasure.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbLimitTimeTreasureWnd wndParent, int index)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_spineEffect = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine_effect", (string)null);
		m_iconLight = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_light", (string)null);
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

	public void SetLight(bool light)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_iconLight.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!light));
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
