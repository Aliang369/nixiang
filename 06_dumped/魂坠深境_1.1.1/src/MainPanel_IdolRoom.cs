using ContainerOfSoul.Display.Fortress.DotManagers;
using TMPro;
using UnityEngine;

public class MainPanel_IdolRoom : UIBase
{
	[SerializeField]
	private GameButton Idol_Musicianl;

	[SerializeField]
	private HintRedDot redDot_Idol_Musicianl;

	[SerializeField]
	private GameButton Idol_Wizard;

	[SerializeField]
	private HintRedDot redDot_Idol_Wizard;

	[SerializeField]
	private GameButton Idol_Warrior;

	[SerializeField]
	private HintRedDot redDot_Idol_Warrior;

	[SerializeField]
	private GameButton Idol_XiaKe;

	[SerializeField]
	private HintRedDot redDot_Idol_XiaKe;

	[SerializeField]
	private GameButton Idol_Dancer;

	[SerializeField]
	private HintRedDot redDot_Idol_Dancer;

	[SerializeField]
	private GameButton forgetShrineButton;

	[SerializeField]
	private TextMeshProUGUI forgetShrineTitle;

	[SerializeField]
	private GameObject forgetShrineRedDot;

	public RectTransform IdolWarriorButtonRect
	{
		get
		{
			if (!(Idol_Warrior != null))
			{
				return null;
			}
			return Idol_Warrior.transform as RectTransform;
		}
	}

	protected override void Init()
	{
		InitUI(redDot_Idol_Musicianl);
		InitUI(redDot_Idol_Wizard);
		InitUI(redDot_Idol_Warrior);
		InitUI(redDot_Idol_XiaKe);
		InitUI(redDot_Idol_Dancer);
		Idol_Musicianl.onClick.AddListener(delegate
		{
			ShowIdol(CharacterClassType.Musician);
		});
		Idol_Wizard.onClick.AddListener(delegate
		{
			ShowIdol(CharacterClassType.Wizard);
		});
		Idol_Warrior.onClick.AddListener(delegate
		{
			ShowIdol(CharacterClassType.Warrior);
		});
		Idol_XiaKe.onClick.AddListener(delegate
		{
			ShowIdol(CharacterClassType.XiaKe);
		});
		Idol_Dancer.onClick.AddListener(delegate
		{
			ShowIdol(CharacterClassType.Dancer);
		});
		redDot_Idol_Musicianl.Setup(new CustomModule
		{
			shouldShow = () => ShouldShowRedDot(CharacterClassType.Musician)
		});
		redDot_Idol_Wizard.Setup(new CustomModule
		{
			shouldShow = () => ShouldShowRedDot(CharacterClassType.Wizard)
		});
		redDot_Idol_Warrior.Setup(new CustomModule
		{
			shouldShow = () => ShouldShowRedDot(CharacterClassType.Warrior)
		});
		redDot_Idol_XiaKe.Setup(new CustomModule
		{
			shouldShow = () => ShouldShowRedDot(CharacterClassType.XiaKe)
		});
		redDot_Idol_Dancer.Setup(new CustomModule
		{
			shouldShow = () => ShouldShowRedDot(CharacterClassType.Dancer)
		});
		forgetShrineButton.onClick.AddListener(OnForgetShrineButtonClick);
		if (forgetShrineTitle != null)
		{
			forgetShrineTitle.text = base.textData.term_forget_shrine_title;
		}
		void OnForgetShrineButtonClick()
		{
			if (!IsForgetShrineUnlocked())
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(base.textData.term_forget_shrine_locked.FormatWith(ScriptableObjectSingleton<GameDataManager>.instance.lockForgetShrine.NameWithColor));
			}
			else
			{
				UIBase.fortressState.uiManager.forgetShrineBasePanel.ShowUI(UIBase.applicationState.characterManager.currentCharacter.characterClass);
			}
		}
		static bool ShouldShowRedDot(CharacterClassType classType)
		{
			return UIBase.applicationState.idolManager.GetIdolState(classType)?.CanUpgrade(UIBase.applicationState) ?? false;
		}
		static void ShowIdol(CharacterClassType classType)
		{
			UIBase.fortressState.uiManager.idolBasePanel.ShowUI(UIBase.applicationState.characterManager.GetCharacterClassState(classType));
		}
	}

	protected override void UpdateDisplay()
	{
		UpdateForgetShrineButton();
	}

	private bool IsForgetShrineUnlocked()
	{
		return UIBase.applicationState.idolManager.forgetShrineUnlocked;
	}

	private void UpdateForgetShrineButton()
	{
		if (!IsForgetShrineUnlocked())
		{
			forgetShrineButton.interactable = true;
			UpdateForgetShrineRedDot(show: false);
		}
		else
		{
			forgetShrineButton.interactable = true;
			UpdateForgetShrineRedDot(UIBase.applicationState.playerInfoManager.HasAnyUnviewedForgetShrinePoint());
		}
		void UpdateForgetShrineRedDot(bool show)
		{
			if (!(forgetShrineRedDot == null))
			{
				forgetShrineRedDot.SetActive(!UIBase.fortressState.uiManager.mainUIPanel.SuppressMainInterfaceRedDots & show);
			}
		}
	}
}
