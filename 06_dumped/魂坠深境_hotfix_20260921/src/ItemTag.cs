using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemTag", menuName = "ItemTag", order = 0)]
public class ItemTag : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber
{
	public enum Type
	{
		CompanionDefence = 2,
		Health = 3,
		Component = 7,
		Attack = 8,
		Defence = 9,
		Companion = 10,
		Blessing = 14,
		Curse = 17,
		Ability = 20,
		Music = 21
	}

	[SerializeField]
	public bool majorTag;

	[SerializeField]
	public bool hideTag;

	[SerializeField]
	public Sprite textSpriteIcon;

	[SerializeField]
	public Color textColor;

	[SerializeField]
	public Type type;

	[SerializeField]
	public string tagDescription;

	[LocalizeText("卡牌标签名字")]
	[CTSOAssetName]
	public string tagName;

	[SerializeField]
	public Sprite counterIcon;

	[SerializeField]
	public Color counterTextColor;

	[SerializeField]
	public List<Ability> attachAbility;

	public IOutGameEffectNode addEffectNode;

	public override string CTAssetIDPrepend => "ITTG";

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (attachAbility != null)
		{
			foreach (Ability item in attachAbility)
			{
				list.Add(item);
			}
		}
		list.Add(addEffectNode);
		return list;
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		if (attachAbility == null)
		{
			yield break;
		}
		foreach (Ability item in attachAbility)
		{
			if (item == null)
			{
				continue;
			}
			foreach (GameEvent.Type item2 in item.GetAllSubscribeGameEventType())
			{
				yield return item2;
			}
		}
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		if (attachAbility == null)
		{
			return;
		}
		foreach (Ability item in attachAbility.OrEmptyIfNull())
		{
			item.TriggerGameEvent(eve);
		}
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (attachAbility != null)
		{
			foreach (Ability item in attachAbility.OrEmptyIfNull())
			{
				if (item.CheckTriggerGameEventAble(eve))
				{
					return true;
				}
			}
		}
		return false;
	}

	public string GetNameText()
	{
		string text = tagName;
		if (majorTag)
		{
			text = text.BoldText();
		}
		return text;
	}
}
