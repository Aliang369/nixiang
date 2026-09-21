using System;
using System.Linq;
using Common.UI;
using UnityEngine;

public class TalentTree : CTUIBaseAsset
{
	[Serializable]
	public class LevelLockConfig
	{
		public int level;

		public int position;

		public string title;

		[TextArea]
		public string description;
	}

	public class TalentSlotConnection
	{
		public TalentSlotCell from;

		public TalentSlotCell to;

		public TalentSlotConnection(TalentSlotCell from, TalentSlotCell to)
		{
			this.from = from;
			this.to = to;
		}

		public bool IsComplete(ApplicationStateInfo applicationState, PlayerCharacterState character)
		{
			if (character.unlockedTalents.Contains(from.talent))
			{
				return character.unlockedTalents.Contains(to.talent);
			}
			return false;
		}

		public bool IsLocked(ApplicationStateInfo applicationState, PlayerCharacterState character)
		{
			if (!character.unlockedTalents.Contains(from.talent))
			{
				return !character.unlockedTalents.Contains(to.talent);
			}
			return false;
		}
	}

	public TalentTreeLevelSplitLine levelSplitLinePrefab;

	public RectTransform splitLinePivot;

	public CellUIPrefabPool<TalentTreeLevelSplitLine, LevelLockConfig> levelSplitLinePool;

	public TalentSlotConnectLine connectLinePrefab;

	public RectTransform connectLinePivot;

	public CellUIPrefabPool<TalentSlotConnectLine, (TalentSlotConnection, PlayerCharacterState)> connectLinePool;

	public Vector2 spacing;

	public override string CTAssetIDPrepend => "TALTREE";

	public TalentSlotCell[] TalentSlotCells => GetComponentsInChildren<TalentSlotCell>();

	public Vector3 talentSlotSize => TalentSlotCells.First().GetComponent<RectTransform>().rect.size;

	protected override void Init()
	{
		foreach (TalentSlotCell item in TalentSlotCells.OrEmptyIfNull())
		{
			InitUI(item);
			if (item.talent == null || !item.talent.IsValidAsset())
			{
				item.gameObject.SetActive(value: false);
			}
		}
		levelSplitLinePool = GetPrefabPool<TalentTreeLevelSplitLine, LevelLockConfig>(levelSplitLinePrefab, splitLinePivot);
		connectLinePool = GetPrefabPool<TalentSlotConnectLine, (TalentSlotConnection, PlayerCharacterState)>(connectLinePrefab, connectLinePivot);
		SetDirty();
	}

	public void Setup(PlayerCharacterState character)
	{
		TalentSlotConnection[] array = TalentSlotCells.SelectMany((TalentSlotCell t) => t.connectTalents.Select((TalentSlotCell c) => new TalentSlotConnection(t, c))).ToArray();
		connectLinePool.DespawnAll();
		TalentSlotConnection[] array2 = array;
		foreach (TalentSlotConnection item in array2)
		{
			connectLinePool.Spawn((item, character)).transform.SetAsFirstSibling();
		}
		levelSplitLinePool.DespawnAll();
	}

	public Vector2 GetPosition(Vector2 treePos)
	{
		Vector2 vector = new Vector2(talentSlotSize.x + spacing.x, 0f);
		Vector2 vector2 = new Vector2(0f, 0f - (talentSlotSize.y + spacing.y));
		return treePos.x * vector + treePos.y * vector2;
	}
}
