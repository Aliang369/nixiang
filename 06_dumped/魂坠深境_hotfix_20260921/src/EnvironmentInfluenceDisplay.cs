using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentInfluenceDisplay : UIBase
{
	private enum DisplayType
	{
		Environment,
		DisorderCritical
	}

	private const float DefaultHeight = 169.8f;

	private const float CompactHeight = 128f;

	private const float PanelTopY = -29.1f;

	private const float CompactSpacing = 4f;

	[SerializeField]
	private DisplayType displayType;

	public TextMeshProUGUI nameText;

	public TextMeshProUGUI descriptionText;

	public Image iconDisplay;

	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	protected override void Init()
	{
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.SetEnvironmentInfluence, OnBattleRuleChange);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, OnBattleRuleChange);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.ExitLocation, OnBattleRuleChange);
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(UIBase.applicationState, descriptionText, base.rectTransform);
		RefreshVisibility();
	}

	protected override void Dispose()
	{
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.SetEnvironmentInfluence, OnBattleRuleChange);
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, OnBattleRuleChange);
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.ExitLocation, OnBattleRuleChange);
	}

	private void OnBattleRuleChange(GameEvent.Type type, GameEvent eve)
	{
		RefreshVisibility();
	}

	protected override void UpdateDisplay()
	{
		TextDataGenerator generator;
		if (displayType == DisplayType.DisorderCritical)
		{
			InfluenceDef influenceDef = UIBase.gameState?.dungeonMapManager?.currentLocation?.disorderCriticalInfluence;
			if (influenceDef == null)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			Influence influence = new Influence(influenceDef);
			iconDisplay.sprite = influenceDef.Icon;
			nameText.text = influenceDef.InfluenceName;
			descriptionText.text = influence.GetInfluenceDescription(UIBase.applicationState, out generator);
		}
		else
		{
			EnvironmentInfluence environmentInfluence = UIBase.gameState?.currentEncounter?.environmentInfluence;
			if (environmentInfluence == null)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			iconDisplay.sprite = environmentInfluence.def.icon;
			iconDisplay.color = Color.white;
			nameText.text = environmentInfluence.def.environmentName;
			descriptionText.text = environmentInfluence.GetDescription(UIBase.gameState, out generator);
		}
		textLinkObjectDisplayer.SetGenerator(generator);
	}

	private void RefreshVisibility()
	{
		bool flag = UIBase.gameState?.currentEncounter?.environmentInfluence != null;
		bool flag2 = UIBase.gameState?.dungeonMapManager?.currentLocation?.disorderCriticalInfluence != null;
		GameStateInfo gameStateInfo = UIBase.gameState;
		bool flag3 = gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true;
		bool flag4 = flag3 & flag & flag2;
		float num = (flag4 ? 128f : 169.8f);
		Vector2 sizeDelta = base.rectTransform.sizeDelta;
		sizeDelta.y = num;
		base.rectTransform.sizeDelta = sizeDelta;
		Vector2 anchoredPosition = base.rectTransform.anchoredPosition;
		anchoredPosition.y = -29.1f - num / 2f;
		if ((displayType == DisplayType.DisorderCritical) & flag)
		{
			anchoredPosition.y -= num + (flag4 ? 4f : 8f);
		}
		base.rectTransform.anchoredPosition = anchoredPosition;
		bool flag5 = ((displayType == DisplayType.DisorderCritical) ? flag2 : flag) & flag3;
		base.gameObject.SetActive(flag5);
		if (flag5)
		{
			SetDirty();
		}
	}
}
