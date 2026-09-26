using System;

[Serializable]
public class zbMonsterCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public int type { get; set; }

	public int elementType { get; set; }

	public float[][] attackTypeWeak { get; set; }

	public float[][] skillTagWeak { get; set; }

	public int monsterSkill1 { get; set; }

	public int monsterSkill2 { get; set; }

	public int monsterSkill3 { get; set; }

	public int avatarId { get; set; }

	public int sortId { get; set; }

	public int hp { get; set; }

	public int damage { get; set; }

	public float atkSpeed { get; set; }

	public int atkRange { get; set; }

	public int atkType { get; set; }

	public int moveType { get; set; }

	public float damageTaken { get; set; }

	public int moveSpeed { get; set; }

	public int artSpeed { get; set; }

	public float bodyScale { get; set; }

	public int mass { get; set; }

	public int[] adviceType { get; set; }

	public int handbookId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMonsterCsvRecord Read(zbBinReader r)
	{
		return new zbMonsterCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			type = r.ReadInt(),
			elementType = r.ReadInt(),
			attackTypeWeak = r.ReadFloatArray2(),
			skillTagWeak = r.ReadFloatArray2(),
			monsterSkill1 = r.ReadInt(),
			monsterSkill2 = r.ReadInt(),
			monsterSkill3 = r.ReadInt(),
			avatarId = r.ReadInt(),
			sortId = r.ReadInt(),
			hp = r.ReadInt(),
			damage = r.ReadInt(),
			atkSpeed = r.ReadFloat(),
			atkRange = r.ReadInt(),
			atkType = r.ReadInt(),
			moveType = r.ReadInt(),
			damageTaken = r.ReadFloat(),
			moveSpeed = r.ReadInt(),
			artSpeed = r.ReadInt(),
			bodyScale = r.ReadFloat(),
			mass = r.ReadInt(),
			adviceType = r.ReadIntArray(),
			handbookId = r.ReadInt()
		};
	}
}
