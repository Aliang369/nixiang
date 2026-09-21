using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillBreakthroughPanel : UIBase, IShowAsync, IHideAsync
{
	[SerializeField]
	private SkillDisplay skillDisplay;

	[SerializeField]
	private SkillDisplay skillDisplay1;

	[SerializeField]
	private SkillDisplay skillDisplay2;

	[SerializeField]
	private SkillInfoDisplay skillInfoDisplay_Before;

	[SerializeField]
	private SkillInfoDisplay skillInfoDisplay_After;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton selectSkill1Button;

	[SerializeField]
	private GameButton selectSkill2Button;

	[SerializeField]
	private GameButton autoSelectButton;

	[SerializeField]
	private MatterCell matterCell;

	[SerializeField]
	private TextMeshProUGUI hintText;

	[SerializeField]
	private Image upgradeIcon;

	[SerializeField]
	private GameButton helpButton;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	public SkillDisplay selecting;

	private SkillUpgradePanel skillUpgradePanel => parent as SkillUpgradePanel;

	private ProfileSkill baseSkill => skillUpgradePanel.selectingSkill;

	public RectTransform AutoSelectButtonRect
	{
		get
		{
			if (!(autoSelectButton != null))
			{
				return null;
			}
			return autoSelectButton.transform as RectTransform;
		}
	}

	public RectTransform ConfirmButtonRect
	{
		get
		{
			if (!(confirmButton != null))
			{
				return null;
			}
			return confirmButton.transform as RectTransform;
		}
	}

	private ProfileSkill skill1 => skillDisplay1.setupData?.profileSkill;

	private ProfileSkill skill2 => skillDisplay2.setupData?.profileSkill;

	private ProfileSkill selectedSkill => selecting?.setupData?.profileSkill;

	private bool canBreakthrough
	{
		get
		{
			if (baseSkill == null || skill1 == null || skill2 == null)
			{
				return false;
			}
			if (!IsValidBreakthroughMaterial(skill1, skill2) || !IsValidBreakthroughMaterial(skill2, skill1))
			{
				return false;
			}
			if (!baseSkill.CanBreakthrough())
			{
				return false;
			}
			MatterInfo breakthroughCost = UIBase.applicationState.globalSkillManager.GetBreakthroughCost(baseSkill);
			if (breakthroughCost != null && !UIBase.applicationState.matterManager.HasEnoughMatter(breakthroughCost))
			{
				return false;
			}
			return true;
		}
	}

	protected override void Init()
	{
		InitUI(skillDisplay);
		InitUI(skillDisplay1);
		InitUI(skillDisplay2);
		InitUI(skillInfoDisplay_Before);
		InitUI(skillInfoDisplay_After);
		InitUI(matterCell);
		confirmButton.onClick.AddListener(OnConfirm);
		selectSkill1Button.onClick.AddListener(delegate
		{
			ShowSkillSelect(skillDisplay1);
		});
		selectSkill2Button.onClick.AddListener(delegate
		{
			ShowSkillSelect(skillDisplay2);
		});
		autoSelectButton.onClick.AddListener(AutoSelect);
		helpButton.onClick.AddListener(delegate
		{
			UIBase.fortressState.uiManager.fortressGuidePanel.skillBreakThroughGuideHint.Show();
		});
		void AutoSelect()
		{
			skillDisplay1.Unset();
			skillDisplay2.Unset();
			List<ProfileSkill> list = UIBase.applicationState.globalSkillManager.allSkills.Where(CanSelect).OrderBy(delegate(ProfileSkill s)
			{
				if (s.hexShape == null)
				{
					return 100;
				}
				return (!s.skillGemsMounted.IsNullOrEmpty()) ? 1 : 10;
			}).ToList();
			if (list.Count > 0)
			{
				skillDisplay1.Setup(new GameSkill(list[0]));
			}
			if (list.Count > 1)
			{
				skillDisplay2.Setup(new GameSkill(list[1]));
			}
			UIBase.fortressState.uiManager.quickHintPanel.HintText((list.Count > 1) ? ScriptableObjectSingleton<TextDataPreset>.instance.term_with_enough_skill_on_breakthrough_autoSelect : ScriptableObjectSingleton<TextDataPreset>.instance.term_without_enough_skill_on_breakthrough_autoSelect);
			SetDirty();
		}
		bool CanSelect(ProfileSkill skill)
		{
			if (skill == baseSkill)
			{
				return false;
			}
			if (skill.isFavorite)
			{
				return false;
			}
			if (skill.QualityLevel != baseSkill.QualityLevel)
			{
				return false;
			}
			if (skill.def != baseSkill.def)
			{
				return false;
			}
			return true;
		}
		void OnConfirm()
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			if (canBreakthrough)
			{
				UIBase.applicationState.globalSkillManager.Breakthrough(baseSkill, skill1, skill2, delegate(ProfileSkill s)
				{
					skillUpgradePanel.selectingSkill = s;
					skillDisplay1.Unset();
					skillDisplay2.Unset();
					CloseSkillSelect();
					RefreshUI();
				});
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_amulet_breakthrough);
			}
		}
	}

	void IShowAsync.OnShowStart()
	{
		base.gameObject.SetActive(value: true);
		CloseSkillSelect();
		if (!UIBase.fortressState.uiManager.fortressGuidePanel.IsPlayingSkillBreakthroughGuide)
		{
			UIBase.fortressState.uiManager.fortressGuidePanel.skillBreakThroughGuideHint.TryFirstOpenAutoPopup();
		}
		RefreshUI();
	}

	void IHideAsync.OnHideStart()
	{
		CloseSkillSelect();
	}

	public IEnumerator ShowAnim()
	{
		if (!((UnityEngine.Object)(object)animator == null))
		{
			((Behaviour)(object)animator).enabled = true;
			animator.SetTrigger(AnimKeywords.show);
			yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		}
	}

	public IEnumerator HideAnim()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: false);
			yield break;
		}
		if ((UnityEngine.Object)(object)animator == null)
		{
			base.gameObject.SetActive(value: false);
			yield break;
		}
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.hide);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		confirmButton.interactable = canBreakthrough;
		autoSelectButton.gameObject.SetActive(skill1 == null || skill2 == null);
		SetHint();
		skillInfoDisplay_After.gameObject.SetActive(value: false);
		upgradeIcon.gameObject.SetActive(value: false);
		if (baseSkill.CanBreakthrough())
		{
			skillInfoDisplay_After.gameObject.SetActive(value: true);
			upgradeIcon.gameObject.SetActive(value: true);
		}
	}

	private void RefreshUI()
	{
		GameSkill setupData = new GameSkill(baseSkill);
		GameSkill setupData2 = new GameSkill(baseSkill.GetBreakthroughPreview());
		skillDisplay.Setup(setupData);
		skillDisplay1.Unset();
		skillDisplay2.Unset();
		skillInfoDisplay_Before.Setup(setupData);
		skillInfoDisplay_After.Setup(setupData2);
		MatterInfo breakthroughCost = UIBase.applicationState.globalSkillManager.GetBreakthroughCost(baseSkill);
		if (breakthroughCost != null)
		{
			matterCell.gameObject.SetActive(value: true);
			matterCell.Setup(breakthroughCost);
		}
		else
		{
			matterCell.gameObject.SetActive(value: false);
			matterCell.Unset();
		}
		SetDirty();
	}

	private void SetHint()
	{
		hintText.gameObject.SetActive(value: false);
		if (baseSkill == null)
		{
			return;
		}
		if (!baseSkill.CanBreakthrough())
		{
			hintText.gameObject.SetActive(value: true);
			hintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_max_level;
			return;
		}
		if (skill1 == null || skill2 == null)
		{
			hintText.gameObject.SetActive(value: true);
			hintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_two_skill_required;
			return;
		}
		if (!IsValidBreakthroughMaterial(skill1, skill2) || !IsValidBreakthroughMaterial(skill2, skill1))
		{
			hintText.gameObject.SetActive(value: true);
			hintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_two_skill_required;
			return;
		}
		MatterInfo breakthroughCost = UIBase.applicationState.globalSkillManager.GetBreakthroughCost(baseSkill);
		if (breakthroughCost != null && !UIBase.applicationState.matterManager.HasEnoughMatter(breakthroughCost))
		{
			hintText.gameObject.SetActive(value: true);
			MatterDef matterDef = breakthroughCost.matterDef;
			hintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_matter_arg0_is_not_enough.FormatWith(matterDef.DisplayMatterName);
		}
		else
		{
			hintText.gameObject.SetActive(value: true);
			hintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_breakthrough_skill;
		}
	}

	private void ShowSkillSelect(SkillDisplay targetSkillDisplay)
	{
		if (targetSkillDisplay == null)
		{
			Debug.LogError("SkillBreakthroughPanel missing target skill display.", this);
			return;
		}
		selecting = targetSkillDisplay;
		skillUpgradePanel.skillUpgradeSelectPanel.ShowForBreakthroughMaterial(this, targetSkillDisplay);
	}

	private void CloseSkillSelect()
	{
		selecting = null;
		if (skillUpgradePanel != null && skillUpgradePanel.skillUpgradeSelectPanel != null)
		{
			manager.HideUI(skillUpgradePanel.skillUpgradeSelectPanel);
		}
	}

	public bool CanSelect(ProfileSkill profileSkill)
	{
		if (baseSkill == null || profileSkill == null)
		{
			return false;
		}
		if (IsSameSkill(baseSkill, profileSkill))
		{
			return false;
		}
		if (profileSkill.isFavorite)
		{
			return false;
		}
		if (!IsSameSkill(selectedSkill, skill1) && IsSameSkill(skill1, profileSkill))
		{
			return false;
		}
		if (!IsSameSkill(selectedSkill, skill2) && IsSameSkill(skill2, profileSkill))
		{
			return false;
		}
		if (profileSkill.def != baseSkill.def)
		{
			return false;
		}
		if (profileSkill.QualityLevel != baseSkill.QualityLevel)
		{
			return false;
		}
		return true;
	}

	private static bool IsSameSkill(ProfileSkill a, ProfileSkill b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(a.profileSkillGUID))
		{
			return a.profileSkillGUID == b.profileSkillGUID;
		}
		return false;
	}

	private bool IsValidBreakthroughMaterial(ProfileSkill material, ProfileSkill otherMaterial)
	{
		if (baseSkill == null || material == null)
		{
			return false;
		}
		if (IsSameSkill(baseSkill, material))
		{
			return false;
		}
		if (IsSameSkill(material, otherMaterial))
		{
			return false;
		}
		if (material.isFavorite)
		{
			return false;
		}
		if (material.def != baseSkill.def)
		{
			return false;
		}
		if (material.QualityLevel != baseSkill.QualityLevel)
		{
			return false;
		}
		return true;
	}
}
