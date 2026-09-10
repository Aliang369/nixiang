using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;

public class ItemDisplayBase : CellUI<Item>, IDisplayModulesController
{
	[Serializable]
	public class PlayerAnimConfig
	{
		[SerializeField]
		public Vector3 idleRotation;

		[SerializeField]
		public Vector3 idleScale;

		[SerializeField]
		public float itemIdleSpace;

		[SerializeField]
		public float drawTime;

		[SerializeField]
		public Vector3 unDrawRotation;

		[SerializeField]
		public Vector3 unDrawScale;

		[SerializeField]
		public float addItemInterval;

		[SerializeField]
		public float preSpawnItemInterval = 0.8f;

		[SerializeField]
		public float removeInterval = 0.05f;

		[SerializeField]
		public float hintOverloadTime1;

		[SerializeField]
		public float hintOverloadTime2;

		[SerializeField]
		public Vector2 hintOverloadOffset;

		[SerializeField]
		public float shuffleTime1;

		[SerializeField]
		public float shuffleTime2;

		[SerializeField]
		public float shuffleTime3;

		[SerializeField]
		public float selectTime1;

		[SerializeField]
		public float selectTime2;

		[SerializeField]
		public Vector3 selectAnimScale1;

		[SerializeField]
		public Vector3 selectAnimScale2;

		[SerializeField]
		public Vector3 selectMaxScale;

		[SerializeField]
		public Vector3 selectRotation;

		[SerializeField]
		public float selectPointerMinOffset;

		[SerializeField]
		public float selectPointerMaxOffset;

		[SerializeField]
		public float selectSpellOffset;

		[SerializeField]
		public float overOffset;

		[SerializeField]
		public Vector3 overRotation;

		[SerializeField]
		public Vector3 overScale;

		[SerializeField]
		public float itemOffset;

		[SerializeField]
		public float rightShiftOffset;

		[SerializeField]
		public float rightShiftTime;

		[SerializeField]
		public float leftShiftTime;

		[SerializeField]
		public float unselectTime;

		[SerializeField]
		public Vector3 addedScale;

		[SerializeField]
		public float multiAddedTotalOffset;

		[SerializeField]
		public float addTime;

		[SerializeField]
		public float spellPositionY;

		[SerializeField]
		public float passiveSpellTime1;

		[SerializeField]
		public Vector2 passiveOffset1;

		[SerializeField]
		public Vector2 passiveOffset2;

		[SerializeField]
		public float switchTime;

		[SerializeField]
		public Color triggerAbleColor;

		[SerializeField]
		public Color positiveEffectColor;

		[SerializeField]
		public Color negativeEffectColor;

		[SerializeField]
		public float hideIntentTime;

		[SerializeField]
		[Range(0f, 1f)]
		public float waveGap;

		[SerializeField]
		public Color costImageColor = Color.white;

		[SerializeField]
		public Color costImageColorHighLight = Color.white;

		[SerializeField]
		public Color costLineColor = Color.white;

		[SerializeField]
		public Color costLineColorHighLight = Color.white;
	}

	[Serializable]
	public class EnemyAnimConfig
	{
		[SerializeField]
		public Vector3 idleRotation;

		[SerializeField]
		public Vector3 idleScale;

		[SerializeField]
		public Vector2 drawPosition1;

		[SerializeField]
		public Vector3 drawRotation1;

		[SerializeField]
		public Vector3 drawScale1;

		[SerializeField]
		public float drawTime1_1;

		[SerializeField]
		public float drawTime1_2;

		[SerializeField]
		public Vector2 drawPosition2;

		[SerializeField]
		public Vector3 drawScale2;

		[SerializeField]
		public float drawTime2;

		[SerializeField]
		public float moveTime;

		public Vector3 showInfoRotate;

		public Vector3 showInfoScale;

		public float showInfoTime;

		[SerializeField]
		public float hideIntentTime;

		[SerializeField]
		public float overTime1;

		[SerializeField]
		public float overTime2;

		[SerializeField]
		public float overOffset;

		[SerializeField]
		public Vector3 overRotation;

		[SerializeField]
		public Vector3 overScale;
	}

	[Serializable]
	public class ItemImageAtlas
	{
		[SerializeField]
		public Sprite bg_Companion;

		[SerializeField]
		public Sprite bg_Health;

		[SerializeField]
		public Sprite bg_Component;

		[SerializeField]
		public Sprite bg_Attack;

		[SerializeField]
		public Sprite bg_Defence;

		[SerializeField]
		public Sprite bg_Blessing;

		[SerializeField]
		public Sprite bg_Curse;

		[SerializeField]
		public Sprite bg_Ability;

		[SerializeField]
		public Sprite icon_Counter_Health;

		[SerializeField]
		public Sprite icon_Counter_Defence;

		[SerializeField]
		public Sprite icon_Counter_Curse;

		[SerializeField]
		public Sprite icon_Counter_Music;

		[SerializeField]
		public Sprite icon_Counter_None;

		[SerializeField]
		public Sprite icon_Cost_Default;

		[SerializeField]
		public Sprite icon_Cost_Companion;

		[SerializeField]
		public Sprite icon_Cost_Burn;

		[SerializeField]
		public Material material_Cost_Default;

		[SerializeField]
		public Material material_Cost_Companion;

		[SerializeField]
		public Material material_Cost_Burn;

		[SerializeField]
		public Sprite icon_Ability;

		[SerializeField]
		public Sprite bgl_Companion;

		[SerializeField]
		public Sprite bgl_Health;

		[SerializeField]
		public Sprite bgl_Component;

		[SerializeField]
		public Sprite bgl_Attack;

		[SerializeField]
		public Sprite bgl_Defence;

		[SerializeField]
		public Sprite bgl_Blessing;

		[SerializeField]
		public Sprite bgl_Curse;

		[SerializeField]
		public Sprite bgl_Ability;

		[SerializeField]
		public Sprite iconl_Counter_Health;

		[SerializeField]
		public Sprite iconl_Counter_Defence;

		[SerializeField]
		public Sprite iconl_Counter_Curse;

		[SerializeField]
		public Sprite iconl_Counter_Music;

		[SerializeField]
		public Sprite iconl_Counter_None;

		[SerializeField]
		public Sprite iconl_Cost_Default;

		[SerializeField]
		public Sprite iconl_Cost_Companion;

		[SerializeField]
		public Sprite iconl_Cost_Burn;

		[SerializeField]
		public Sprite bgl_TextArea_Spell;

		[SerializeField]
		public Sprite bgl_TextArea_Companion;

		[SerializeField]
		public Color color_Cost_Default;

		[SerializeField]
		public Color color_Cost_Companion;

		[SerializeField]
		public Sprite yinYangIcon_Convert_Yang;

		[SerializeField]
		public Sprite yinYangIcon_Convert_Yin;

		[SerializeField]
		public Sprite yinYangIcon_Add_Yin;

		[SerializeField]
		public Sprite yinYangIcon_Add_Yang;

		[SerializeField]
		public Sprite yinYangIcon_Consume_Yin;

		[SerializeField]
		public Sprite yinYangIcon_Consume_Yang;

		[SerializeField]
		public Material yinYangMaterial_Convert_Yin;

		[SerializeField]
		public Material yinYangMaterial_Convert_Yang;
	}

	public abstract class DisplayModule : UIDisplayModule
	{
		protected ItemDisplayBase slotDisplay => parent as ItemDisplayBase;

		protected Item item => slotDisplay.item;
	}

	public class ItemDefinitionConvertor : CellUIPrefabPool<ItemDisplayBase, Item>.IDataConvertor<ItemDefinition>
	{
		public Item ToSetupData(ItemDefinition t)
		{
			return new Item(t);
		}

		public ItemDefinition FromSetupData(Item setupData)
		{
			return setupData.def;
		}
	}

	public static ItemDefinitionConvertor itemDefinitionConvertor = new ItemDefinitionConvertor();

	public static List<ItemDisplayBase> allItemSlotDisplays = new List<ItemDisplayBase>();

	[SerializeField]
	public InputModule itemSlotInputModule;

	[SerializeField]
	public bool isBattleDisplay;

	[NonSerialized]
	private ConvertablePivot _pivot;

	[NonSerialized]
	public bool enableUpdate;

	[NonSerialized]
	public bool indexHide;

	[NonSerialized]
	public Item item;

	public ConvertablePivot pivot
	{
		get
		{
			if (_pivot == null)
			{
				_pivot = new ConvertablePivot(base.transform);
			}
			return _pivot;
		}
	}

	protected override bool shouldUpdate
	{
		get
		{
			if (enableUpdate)
			{
				return base.shouldUpdate;
			}
			return false;
		}
	}

	public bool IsDiscovered => item.GetDef().IsDiscovered(CellUI<Item>.applicationState);

	public bool IsUnlocked => item.GetDef().IsUnlocked(CellUI<Item>.applicationState);

	public GameEvent.TriggerType triggerType
	{
		get
		{
			if (!isBattleDisplay)
			{
				return GameEvent.TriggerType.OutBattleDisplay;
			}
			return GameEvent.TriggerType.InBattleDisplay;
		}
	}

	protected override void OnSetup(Item dailyContract)
	{
		item = dailyContract;
		if (dailyContract != null && !allItemSlotDisplays.Contains(this))
		{
			allItemSlotDisplays.Add(this);
		}
		else if (dailyContract == null && allItemSlotDisplays.Contains(this))
		{
			allItemSlotDisplays.Remove(this);
		}
		enableUpdate = true;
		ForceUpdate();
	}

	protected override void OnUnset()
	{
		enableUpdate = false;
		item = null;
	}

	public static ItemDisplayBase GetActiveItemSlotDisplay(Item item)
	{
		if (item == null)
		{
			return null;
		}
		ItemDisplayBase itemDisplayBase = allItemSlotDisplays.Find((ItemDisplayBase d) => d != null && d.gameObject.activeInHierarchy && d.item == item);
		if (itemDisplayBase != null)
		{
			return itemDisplayBase;
		}
		return null;
	}

	public ConvertablePivot GetItemPivot()
	{
		return pivot;
	}

	public Vector3 GetMainCanvasPosition()
	{
		return GetItemPivot().ToCanvasWorldPosition(CellUI<Item>.gameState.UIManager.mainCanvas);
	}
}
