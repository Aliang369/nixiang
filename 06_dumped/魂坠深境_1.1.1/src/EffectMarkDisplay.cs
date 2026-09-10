using UnityEngine.UI;

public class EffectMarkDisplay : CellUI<EffectMarkAbility.EffectMarkInfo>
{
	public Image icon;

	protected override void OnSetup(EffectMarkAbility.EffectMarkInfo info)
	{
		icon.sprite = info?.GetIcon();
	}
}
