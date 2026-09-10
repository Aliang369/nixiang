using System;

[Serializable]
public class RiftBossEffectState
{
	[SaveField(null, null)]
	public RiftBossEffectType type;

	[SaveField(null, null)]
	public int value;
}
