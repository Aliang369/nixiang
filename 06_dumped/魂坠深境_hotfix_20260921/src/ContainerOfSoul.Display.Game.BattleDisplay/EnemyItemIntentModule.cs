using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

[Serializable]
public class EnemyItemIntentModule : ItemDisplayBase.DisplayModule
{
	private const float valueChangeStartHintDeltaScale = 2f;

	private const float valueChangeStartHintTime = 0.2f;

	public int currentSpellValue;

	public Image intentTypeBaseImage;

	public TextMeshProUGUI spellLevelText;

	public TextMeshProUGUI spellLevelTextShadow;

	public int targetSpellValue;

	public Image valeDeltaHint;

	public Color valueDeltaNegativeColor;

	public Color valueDeltaPositiveColor;

	public ItemSpell lastSpell;

	private Sequence sequence;

	private float deafultDarkenScale => 0.5f;

	public override void Init()
	{
		base.Init();
		UIDisplayModule.gameState.actionManager.actionFrameFinishEvent += _OnActionFrameFinish;
	}

	public override void Dispose()
	{
		base.Dispose();
		UIDisplayModule.gameState.actionManager.actionFrameFinishEvent -= _OnActionFrameFinish;
	}

	private bool CheckWithSpellIntent()
	{
		return base.slotDisplay?.item?.CurrentSpell?.spellNode != null;
	}

	private bool CheckCastedThisTurn()
	{
		CharacterAI characterAI = base.slotDisplay.item.OwnerBft?.characterAI;
		if (characterAI == null)
		{
			return false;
		}
		if (characterAI.castedItemThisTurn.Contains(base.slotDisplay.item))
		{
			return true;
		}
		return false;
	}

	public override void UpdateDisplay()
	{
		UpdateSpellValueText();
		UpdateValueDeltaHint();
	}

	public NodeDisplayInfo GetSpellNodeInfo(Item item, ItemSpell spell)
	{
		DisplaySpellCastInfo displaySpellCastInfo = new DisplaySpellCastInfo(UIDisplayModule.gameState)
		{
			spellItem = item
		};
		displaySpellCastInfo.SetDefaultTargetItem();
		GameEvent eve = Effect.GenerateDisplayEvent(UIDisplayModule.gameState, item, displaySpellCastInfo, GameEvent.TriggerType.InBattleDisplay);
		NodeDisplayInfo spellIntentEffectDisplayInfo = spell.GetSpellIntentEffectDisplayInfo(eve);
		if (spellIntentEffectDisplayInfo != null)
		{
			NodeDisplayInfo.Element<int?> value = spellIntentEffectDisplayInfo.value;
			if (value != null && value.value.HasValue)
			{
				return spellIntentEffectDisplayInfo;
			}
		}
		List<Ability> list = item?.def?.abilities;
		if (list == null)
		{
			return spellIntentEffectDisplayInfo;
		}
		foreach (Ability item2 in list)
		{
			if (item2 is IEnemyIntentDisplayAbility enemyIntentDisplayAbility)
			{
				return enemyIntentDisplayAbility.GetNodeDisplayInfo(eve) ?? spellIntentEffectDisplayInfo;
			}
		}
		return spellIntentEffectDisplayInfo;
	}

	private void UpdateSpellValueText()
	{
		if (CheckWithSpellIntent() && !CheckCastedThisTurn())
		{
			ItemSpell spell = base.slotDisplay.item.CurrentSpell;
			NodeDisplayInfo spellNodeInfo = GetSpellNodeInfo(base.slotDisplay.item, spell);
			if (spellNodeInfo != null)
			{
				spellLevelText.gameObject.SetActive(value: true);
				spellLevelTextShadow.gameObject.SetActive(value: true);
				int? previewIntentValue = GetPreviewIntentValue(spellNodeInfo);
				if (previewIntentValue.HasValue)
				{
					int value = previewIntentValue.Value;
					if (lastSpell == null || lastSpell != spell)
					{
						targetSpellValue = value;
						currentSpellValue = value;
						lastSpell = spell;
					}
					else if (value != targetSpellValue)
					{
						targetSpellValue = value;
						sequence?.Kill();
						sequence = DOTween.Sequence();
						sequence.timeScale = UIDisplayModule.gameState.actionManager.GetBattleScale();
						sequence.Append(spellLevelText.DOScale(2f, 0.1f));
						sequence.Join(spellLevelTextShadow.DOScale(2f, 0.1f));
						sequence.Append(spellLevelText.DOScale(1f, 0.1f));
						sequence.Join(spellLevelTextShadow.DOScale(1f, 0.1f));
						TweenerCore<int, int, NoOptions> t = DOTween.To(() => currentSpellValue, delegate(int v)
						{
							currentSpellValue = v;
							UpdateDisplay();
						}, targetSpellValue, 0.5f);
						sequence.Append(t);
						Sequence obj = sequence;
						obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
						{
							sequence = null;
							currentSpellValue = targetSpellValue;
						});
						sequence.Play();
					}
				}
				int num = spellNodeInfo.times?.value ?? 1;
				if (!previewIntentValue.HasValue)
				{
					spellLevelText.text = "";
					spellLevelTextShadow.text = spellLevelText.text;
				}
				else if (num != 1)
				{
					spellLevelText.text = TextDisplayHelper.ToNumSpriteString(currentSpellValue) + "<sprite=20 tint=1>" + TextDisplayHelper.ToNumSpriteString(num, 10);
					spellLevelTextShadow.text = spellLevelText.text;
				}
				else
				{
					spellLevelText.text = TextDisplayHelper.ToNumSpriteString(currentSpellValue) ?? "";
					spellLevelTextShadow.text = spellLevelText.text;
				}
			}
			else
			{
				spellLevelText.gameObject.SetActive(value: false);
				spellLevelTextShadow.gameObject.SetActive(value: false);
			}
			bool flag = (int)base.slotDisplay.item.cooldown == 0;
			VFXDataPreset.IntentTypeDisplayInfo intentTypeDisplayInfo = ScriptableObjectSingleton<VFXDataPreset>.instance.intentDisplayInfos.Find((VFXDataPreset.IntentTypeDisplayInfo intentTypeDisplayInfo2) => intentTypeDisplayInfo2.intentType == spell.IntentType);
			spellLevelText.color = (flag ? intentTypeDisplayInfo.textColor : intentTypeDisplayInfo.inactiveTextColor);
			intentTypeBaseImage.sprite = (flag ? intentTypeDisplayInfo.activeSlotIntentImage : intentTypeDisplayInfo.inactiveSlotIntentImage);
		}
		else
		{
			spellLevelText.gameObject.SetActive(value: false);
			spellLevelTextShadow.gameObject.SetActive(value: false);
		}
	}

	private void UpdateValueDeltaHint()
	{
		if (!CheckWithSpellIntent() || CheckCastedThisTurn())
		{
			intentTypeBaseImage.gameObject.SetActive(value: false);
			valeDeltaHint.gameObject.SetActive(value: false);
			return;
		}
		intentTypeBaseImage.gameObject.SetActive(value: true);
		Item item = base.slotDisplay.item;
		ItemSpell itemSpell = item?.CurrentSpell;
		NodeDisplayInfo nodeDisplayInfo = ((itemSpell == null) ? null : GetSpellNodeInfo(item, itemSpell));
		if (itemSpell != null && nodeDisplayInfo != null)
		{
			NodeDisplayInfo.Element<int?> deltaValue = nodeDisplayInfo.deltaValue;
			if (deltaValue != null && deltaValue.value.HasValue && nodeDisplayInfo.deltaValue.value != 0)
			{
				valeDeltaHint.gameObject.SetActive(value: true);
				bool flag = (int)base.slotDisplay.item.cooldown == 0;
				if (nodeDisplayInfo.deltaValue.value > 0)
				{
					valeDeltaHint.color = (flag ? valueDeltaPositiveColor : (deafultDarkenScale * valueDeltaPositiveColor));
					valeDeltaHint.transform.localScale = Vector3.one;
				}
				else
				{
					valeDeltaHint.color = (flag ? valueDeltaNegativeColor : (deafultDarkenScale * valueDeltaNegativeColor));
					valeDeltaHint.transform.localScale = new Vector3(1f, -1f, 1f);
				}
				return;
			}
		}
		valeDeltaHint.gameObject.SetActive(value: false);
	}

	private static int? GetPreviewIntentValue(NodeDisplayInfo info)
	{
		int? num = info?.baseValue?.value;
		if (info?.damage != null && num.HasValue)
		{
			return num.Value + (info.deltaValue?.value).GetValueOrDefault();
		}
		return info?.value?.value;
	}

	private void _OnActionFrameFinish()
	{
		base.slotDisplay.SetDirty();
	}

	public void OnExile()
	{
		spellLevelText.gameObject.SetActive(value: false);
		spellLevelTextShadow.gameObject.SetActive(value: false);
	}
}
