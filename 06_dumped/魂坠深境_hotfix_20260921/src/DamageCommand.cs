using System.Collections.Generic;

public class DamageCommand
{
	public class OverflowLastDamageInfo
	{
		public List<Item> damagedItems;

		public Item lastDamageItem;
	}

	public int baseDamage;

	public OverflowLastDamageInfo lastDamageInfo;

	public bool overflow;

	public bool pierce;

	public bool ignoreSourceDamageModifiers;

	public object source;

	public BattleFieldTransform sourceOwnerOverride;

	public SpellCastInfo spellCastInfo;

	public BattleFieldTransform targetBFT;

	public TransformNFXConfig transformSetup;

	public TransformNFX3DConfig transform3DSetup;

	public HitNFXConfig hitSetup;

	public HitNFX3DConfig hit3DSetup;

	public PersistentHitNFX3DConfig persistentHit3DSetup;
}
