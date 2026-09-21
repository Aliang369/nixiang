using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultDisplay : UIBase, IStackPanel
{
	public GlobalItemDisplay gachaResultCellPrefab;

	public GlobalRewardPackageDisplay globalRewardPackageDisplay;

	public RectTransform gachaResultSummaryArea;

	public GameObject compensateTokenSummaryArea;

	public CellUIPrefabPoolConfig<GlobalItemDisplay, IGlobalItemDisplaySetup> appendMatterPool;

	public GameButton closeButton;

	public GameButton skipButton;

	public CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> pool;

	public GlobalRewardPackage gachaResult;

	public GachaAnimPlayer gachaAnimPlayer;

	public bool toSkip;

	private Action afterDraw;

	private GlobalRewardPackage pendingConvertObsessionRewardPackage;

	protected override void Init()
	{
		gachaAnimPlayer = new GachaAnimPlayer();
		Init(appendMatterPool);
		InitUI(globalRewardPackageDisplay);
		GetPrefabPool(ref pool, gachaResultCellPrefab);
		if (UIBase.fortressState?.uiManager.universalCommonInfoPanel != null)
		{
			pool.ConfigInfoPanel(UIBase.fortressState.uiManager.universalCommonInfoPanel);
		}
		closeButton.onClick.AddListener(CloseResultDisplay);
		skipButton.onClick.AddListener(delegate
		{
			toSkip = true;
		});
	}

	public void PlayDrawAnim(GlobalRewardPackage result, Action afterDraw)
	{
		if (UIBase.fortressState != null)
		{
			gachaResult = result;
			toSkip = false;
			this.afterDraw = afterDraw;
			pendingConvertObsessionRewardPackage = GlobalRewardPackage.CreateObsessionConvertPackage(gachaResult.rewards);
			ForceUpdate();
			UIBase.fortressState.StartCoroutine(DisplayGachaResultCor);
		}
		static IEnumerator DisplayCharacterAnim(GlobalRewardInfo reward)
		{
			UIBase.fortressState.uiManager.characterGetDisplay.Setup(reward);
			yield return new WaitUntil(() => !UIBase.fortressState.uiManager.characterGetDisplay.gameObject.activeInHierarchy);
		}
		IEnumerator DisplayGachaResultCor()
		{
			List<GlobalRewardInfo> rewards = gachaResult.rewards.Copy();
			skipButton.gameObject.SetActive(value: false);
			closeButton.gameObject.SetActive(value: false);
			if (toSkip)
			{
				gachaResultSummaryArea.gameObject.SetActive(value: false);
			}
			else
			{
				base.gameObject.SetActive(value: false);
				SetMatterInfo();
				Image gachaAnimPanel = UIBase.fortressState.uiManager.gachaAnimPanel;
				gachaAnimPanel.gameObject.SetActive(value: true);
				gachaAnimPanel.SetA(0f);
				yield return gachaAnimPlayer.DisplayGachaAnim(rewards, OnHideGame, OnRecoverGame);
				gachaAnimPanel.gameObject.SetActive(value: false);
				base.gameObject.SetActive(value: true);
				gachaResultSummaryArea.gameObject.SetActive(value: false);
				foreach (GlobalRewardInfo item in rewards)
				{
					if (!(item.refCharacter == null))
					{
						if (item.matter != null && item.matter.matterDef.matterType == MatterDef.MatterType.ObsessionDebris)
						{
							GlobalRewardGiveAppendInfo giveAppendInfo = item.giveAppendInfo;
							if (giveAppendInfo == null || !giveAppendInfo.convertObsessionToDebris)
							{
								continue;
							}
						}
						yield return DisplayCharacterAnim(item);
					}
				}
				gachaResultSummaryArea.gameObject.SetActive(value: true);
				pool.DespawnAll();
				foreach (GlobalRewardInfo item2 in rewards)
				{
					pool.Spawn(item2);
				}
				foreach (GlobalRewardInfo reward in rewards)
				{
					PlayGachaGetItemSound(reward);
					yield return new WaitForSeconds(0.1f);
					pool.GetActiveElement(reward).GetComponent<GachaResultDisplayModule>().PlayShowAnim();
				}
				ForceUpdate();
				afterDraw?.Invoke();
				this.afterDraw = null;
				yield return new WaitForSeconds(0.1f);
				closeButton.gameObject.SetActive(value: true);
				skipButton.gameObject.SetActive(value: false);
			}
		}
		static IEnumerator OnHideGame()
		{
			UIBase.fortressState.uiManager.gachaResultDisplay.gachaResultSummaryArea.gameObject.SetActive(value: false);
			UIBase.fortressState.uiManager.basePanels.gameObject.SetActive(value: false);
			UIBase.fortressState.uiManager.secondaryPanels.gameObject.SetActive(value: false);
			yield break;
		}
		static IEnumerator OnRecoverGame()
		{
			UIBase.fortressState.uiManager.basePanels.gameObject.SetActive(value: true);
			UIBase.fortressState.uiManager.secondaryPanels.gameObject.SetActive(value: true);
			UIBase.fortressState.uiManager.gachaResultDisplay.ShowUI();
			UIBase.fortressState.uiManager.gachaPanel.ResetAnim();
			UIBase.fortressState.uiManager.gachaResultDisplay.ForceUpdate();
			yield break;
		}
		static void PlayGachaGetItemSound(GlobalRewardInfo reward)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			if (reward.GlobalItemQuality == QualityLevel.Epic)
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_item_acquire_epic);
			}
			else if (reward.GlobalItemQuality == QualityLevel.Rare)
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_item_acquire_rare);
			}
			else
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_item_acquire);
			}
		}
		void SetMatterInfo()
		{
			compensateTokenSummaryArea.gameObject.SetActive(value: false);
			appendMatterPool.DespawnAll();
			if (gachaResult.SumTokenCompensate > 0)
			{
				compensateTokenSummaryArea.gameObject.SetActive(value: true);
				appendMatterPool.Spawn(new GlobalRewardInfo
				{
					matter = new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter, gachaResult.SumTokenCompensate)
				});
			}
			if (gachaResult.SumItemSkinTokenCompensate > 0)
			{
				compensateTokenSummaryArea.gameObject.SetActive(value: true);
				appendMatterPool.Spawn(new GlobalRewardInfo
				{
					matter = new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.itemSkinTokenMatter, gachaResult.SumItemSkinTokenCompensate)
				});
			}
			if (gachaResult.SumTokenSoul > 0)
			{
				compensateTokenSummaryArea.gameObject.SetActive(value: true);
				foreach (GlobalRewardInfo reward2 in gachaResult.rewards)
				{
					if (reward2.soul != null && reward2.soul.count != 0)
					{
						appendMatterPool.Spawn(new GlobalRewardInfo
						{
							matter = reward2.soul
						});
					}
				}
			}
		}
	}

	private void CloseResultDisplay()
	{
		base.gameObject.SetActive(value: false);
		afterDraw?.Invoke();
		afterDraw = null;
		if (pendingConvertObsessionRewardPackage != null)
		{
			GlobalRewardPackage package = pendingConvertObsessionRewardPackage;
			pendingConvertObsessionRewardPackage = null;
			UIBase.fortressState.StartCoroutine(() => ShowConvertObsessionRewardPackage(package));
		}
	}

	private IEnumerator ShowConvertObsessionRewardPackage(GlobalRewardPackage package)
	{
		yield return new WaitForSeconds(0.1f);
		UIBase.fortressState.uiManager.globalRewardPackageDisplay.Setup(package);
	}
}
