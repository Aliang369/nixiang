using System.Collections.Generic;

public class ItemTagInstance : IAttachment, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber, IBftOwnable
{
	[SaveField(null, null)]
	public Item item;

	[SaveField(null, null)]
	public ItemTag tag;

	[SaveField(null, null)]
	public Dictionary<string, object> tempValues;

	public object AttachTarget => item;

	public BattleFieldTransform OwnerBft => item.OwnerBft;

	public ItemTagInstance(Item item, ItemTag tag)
	{
		this.item = item;
		this.tag = tag;
		tempValues = new Dictionary<string, object>();
	}

	private ItemTagInstance()
	{
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return tag.GetNodeDisplayInfo(eve);
	}

	public List<IEffectNode> GetChildren()
	{
		return ((IChildren<IEffectNode>)tag).GetChildren();
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		return ((IGameEventSubscriber)tag).GetAllSubscribeGameEventType();
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		return ((IGameEventSubscriber)tag).CheckTriggerGameEventAble(eve);
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		((IGameEventSubscriber)tag).TriggerGameEvent(eve);
	}

	public ItemTagInstance CopyToItem(Item item)
	{
		ItemTagInstance obj = MemberwiseClone() as ItemTagInstance;
		obj.item = item;
		obj.tempValues = new Dictionary<string, object>(tempValues);
		return obj;
	}
}
