using System;
using UnityEngine;

[Serializable]
public class RiftCharacterBlessingConfig
{
	public PlayerCharacterDef character;

	public InfluenceDef blessing;

	public RiftBlessingType type;

	public SkillWordDef skillWord;

	public Sprite portraitOverride;

	public Sprite previewSprite;
}
