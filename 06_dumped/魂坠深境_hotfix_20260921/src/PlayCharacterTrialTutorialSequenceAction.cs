using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayCharacterTrialTutorialSequenceAction : AbstractEnumeratorAction
{
	protected override IEnumerator Run()
	{
		PlayerCharacterDef trialCharacterDef = gameState.newGameConfigData.encounterSetupConfig.trialCharacterDef;
		AssetReference tutorialPrefabRef = ScriptableObjectSingleton<CharacterTrialConfig>.instance.GetEntry(trialCharacterDef).tutorialGuideHintPanelSetPrefab;
		if (tutorialPrefabRef != null && tutorialPrefabRef.RuntimeKeyIsValid())
		{
			AsyncOperationHandle<GameObject> loadHandle = Addressables.LoadAssetAsync<GameObject>(tutorialPrefabRef.RuntimeKey);
			yield return loadHandle;
			if (loadHandle.Status != AsyncOperationStatus.Succeeded)
			{
				Addressables.Release(loadHandle);
				throw new InvalidOperationException($"Failed to load character trial tutorial prefab: {tutorialPrefabRef.RuntimeKey}");
			}
			GameObject result = loadHandle.Result;
			if (result == null)
			{
				Addressables.Release(loadHandle);
				throw new NullReferenceException($"Character trial tutorial prefab is null: {tutorialPrefabRef.RuntimeKey}");
			}
			yield return PlayGuideHintPanelSet(result);
			Addressables.Release(loadHandle);
		}
	}

	private IEnumerator PlayGuideHintPanelSet(GameObject prefab)
	{
		GuideUIPanel guideUIPanel = gameState.UIManager.guideUIPanel;
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, GetPrefabParent(guideUIPanel.transform));
		GuideHintPanelSet panel = gameObject.GetComponent<GuideHintPanelSet>();
		if (panel == null)
		{
			UnityEngine.Object.Destroy(gameObject);
			throw new NullReferenceException("Character trial tutorial prefab is missing GuideHintPanelSet: " + prefab.name);
		}
		panel.name = prefab.name;
		panel.transform.SetAsLastSibling();
		panel.transform.localScale = Vector3.one;
		panel.gameObject.SetActive(value: false);
		guideUIPanel.InitUI(panel);
		bool confirmed = false;
		panel.Show(delegate
		{
			confirmed = true;
		});
		yield return new WaitUntil(() => confirmed);
		guideUIPanel.DisposeUI(panel);
		UnityEngine.Object.Destroy(panel.gameObject);
	}

	private Transform GetPrefabParent(Transform guideUIPanelTrans)
	{
		UISafeArea componentInChildren = guideUIPanelTrans.GetComponentInChildren<UISafeArea>();
		if (componentInChildren != null)
		{
			return componentInChildren.transform;
		}
		return guideUIPanelTrans;
	}
}
