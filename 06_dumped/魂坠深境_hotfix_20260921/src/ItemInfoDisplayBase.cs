using System;
using Common.UI;
using UnityEngine;

public abstract class ItemInfoDisplayBase : CellUI<Item>, IDisplayModulesController
{
	public abstract class DisplayModule : UIDisplayModule
	{
		protected ItemInfoDisplayBase itemInfoDisplay => parent as ItemInfoDisplayBase;

		protected Item item => itemInfoDisplay.item;
	}

	[Serializable]
	public class Config
	{
		[SerializeField]
		public float showInfoOffsetFactor;

		[SerializeField]
		public Vector3 enemyShowInfoScale1;

		[SerializeField]
		public Vector3 enemyShowInfoRotation1;

		[SerializeField]
		public Vector3 enemyShowInfoScale2;

		[SerializeField]
		public Vector3 enemyShowInfoRotation2;

		[SerializeField]
		public float enemyShowInfoShowTime;

		[SerializeField]
		public float enemyShowInfoStayTime;

		[SerializeField]
		public float enemyShowInfoHideTime;

		[SerializeField]
		public Vector3 rotateAnimStartShowScale;

		[SerializeField]
		public Vector3 rotateAnimStartShowRotation;
	}

	public class AnimSetupInfo
	{
		public enum AnimType
		{
			Show,
			Hide,
			Stay
		}

		public AnimType animType;

		public bool flip;

		public Vector3 hidePosition;

		public bool interactable = true;

		public Vector3? startShowPosition;

		public Vector3 stayPosition;

		public static Vector3 GetHidePosition(GameStateInfo gameState, Vector3 stayPosition, RectTransform mainItemInfoDisplay, bool left = true)
		{
			Vector3 vector = mainItemInfoDisplay.WorldPositionToAnchored(stayPosition);
			if (left)
			{
				vector.x = 0f - mainItemInfoDisplay.GetWidth();
			}
			else
			{
				vector.x = ((Component)(object)gameState.UIManager.mainCanvas).transform.RectTransform().GetWidth() + mainItemInfoDisplay.GetWidth();
			}
			return mainItemInfoDisplay.AnchoredToWorldPosition(vector);
		}
	}

	[SerializeField]
	public bool isBattleDisplay;

	[SerializeField]
	public bool showStoryReference;

	[NonSerialized]
	private bool _enableInteract = true;

	[NonSerialized]
	public SpellTextModule.UpgradeTextGenerator upgradeTextGenerator;

	[NonSerialized]
	public bool showUncollectedStatus;

	public Item item => base.setupData;

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

	public bool enableInteract
	{
		get
		{
			return _enableInteract;
		}
		set
		{
			GetComponent<CanvasGroup>().interactable = value;
			_enableInteract = value;
		}
	}

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
		upgradeTextGenerator = new SpellTextModule.UpgradeTextGenerator();
	}

	protected override void OnSetup(Item item)
	{
		upgradeTextGenerator.before = null;
		upgradeTextGenerator.after = null;
		GetModule<ItemInfoReferenceModule>().HideAll();
		GetModule<ItemInfoReferenceModule>().ShowAllSideReference();
		base.gameObject.SetActive(value: true);
		enableInteract = _enableInteract;
	}

	protected override void OnUnset()
	{
		showUncollectedStatus = false;
		if (item != null)
		{
			GetModule<ItemInfoReferenceModule>().HideAll();
			base.gameObject.SetActive(value: false);
		}
	}

	public abstract Vector2 GetLastItemInfoDisplayPosition();

	public abstract bool IsReference(MainItemInfoDisplayBase mainItemInfoDisplay);
}
