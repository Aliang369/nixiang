using UnityEngine;
using UnityEngine.UI;

public class CoinCrystalHintPanel : CommonInfoPanelBase
{
	public enum Type
	{
		Crystal = 1,
		Coin,
		Gem
	}

	[SerializeField]
	public Image coinHintImage;

	[SerializeField]
	public Image crystalHintImage;

	[SerializeField]
	public Image gemHintImage;

	protected override void Init()
	{
		base.Init();
		coinHintImage.gameObject.SetActive(value: false);
		crystalHintImage.gameObject.SetActive(value: false);
		gemHintImage.gameObject.SetActive(value: false);
	}

	protected override void UpdateInfo()
	{
		coinHintImage.gameObject.SetActive(value: false);
		crystalHintImage.gameObject.SetActive(value: false);
		gemHintImage.gameObject.SetActive(value: false);
		if (currentCommonInfoData?.target != null)
		{
			if (currentCommonInfoData.target.Equals(Type.Crystal))
			{
				crystalHintImage.gameObject.SetActive(value: true);
			}
			else if (currentCommonInfoData.target.Equals(Type.Coin))
			{
				coinHintImage.gameObject.SetActive(value: true);
			}
			else if (currentCommonInfoData.target.Equals(Type.Gem))
			{
				gemHintImage.gameObject.SetActive(value: true);
			}
		}
	}
}
