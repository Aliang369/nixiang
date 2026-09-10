using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;

public class CharacterClassState : IValidate
{
	[SaveField(null, null)]
	public CharacterClassDef classDef;

	[SaveField(null, null)]
	public List<ItemDefinition> forgetShrineItems;

	public int forgetShrinePoint => GetIdolState().GetForgetShrinePoint();

	private CharacterClassState()
	{
	}

	public CharacterClassState(CharacterClassDef character)
	{
		classDef = character;
		forgetShrineItems = new List<ItemDefinition>();
	}

	public IdolDef GetIdolDef()
	{
		return Bootstrap.applicationState.assetCatalog.idolDefCatalog.ValidAssets.Find((IdolDef i) => i.attachClass == classDef.mClass);
	}

	public IdolState GetIdolState()
	{
		return Bootstrap.applicationState.idolManager.idolStates.Find((IdolState i) => i.idolDef == GetIdolDef());
	}

	public List<ItemDefinition> GetForgetShrineItems()
	{
		if (forgetShrineItems == null)
		{
			forgetShrineItems = new List<ItemDefinition>();
		}
		return forgetShrineItems;
	}

	public void SetForgetShrineItems(List<ItemDefinition> newItems, Action onComplete = null)
	{
		if (newItems == null)
		{
			newItems = new List<ItemDefinition>();
		}
		List<ItemDefinition> oldItems = forgetShrineItems ?? new List<ItemDefinition>();
		if (GlobalConfigManager.instance.enableNetwork)
		{
			SetForgetShrineRequest setForgetShrineRequest = new SetForgetShrineRequest();
			setForgetShrineRequest.CharacterClassId = classDef.FullID;
			setForgetShrineRequest.Items = newItems.Select((ItemDefinition i) => i.FullID).ToList();
			new GeneralGameAsyncOperationRequestOnly<SetForgetShrineRequest>(setForgetShrineRequest, Bootstrap.applicationState.ChillyRoomSDK.Character.SetForgetShrineAsync).OnComplete(OnComplete).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_saving_forget_shrine);
		}
		void OnComplete()
		{
			forgetShrineItems = newItems;
			Bootstrap.SetProfileDirtyToSave();
			foreach (ItemDefinition item in oldItems)
			{
				if (!newItems.Contains(item))
				{
					ThinkingAnalyticsExtends.TrackForgetShrineChange(Bootstrap.applicationState, classDef, item.FullID, "移除");
				}
			}
			foreach (ItemDefinition newItem in newItems)
			{
				if (!oldItems.Contains(newItem))
				{
					ThinkingAnalyticsExtends.TrackForgetShrineChange(Bootstrap.applicationState, classDef, newItem.FullID, "添加");
				}
			}
			onComplete?.Invoke();
		}
	}

	bool IValidate.IsValidate()
	{
		return classDef != null;
	}
}
