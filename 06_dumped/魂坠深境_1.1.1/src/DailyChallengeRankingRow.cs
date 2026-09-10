using System;
using UnityEngine;

[Serializable]
public sealed class DailyChallengeRankingRow
{
	public int rank;

	public long playerId;

	public string recordId;

	public string playerName;

	public string characterName;

	public string difficultyName;

	public int score;

	public Sprite avatar;

	public bool hasRecord = true;
}
