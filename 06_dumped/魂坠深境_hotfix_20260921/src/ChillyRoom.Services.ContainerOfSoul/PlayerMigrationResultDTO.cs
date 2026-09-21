using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class PlayerMigrationResultDTO
{
	[JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
	public bool Success { get; set; }

	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public PlayerDataMigrationStatus Status { get; set; }

	[JsonProperty("originalShardId", NullValueHandling = NullValueHandling.Ignore)]
	public short OriginalShardId { get; set; }

	[JsonProperty("targetShardId", NullValueHandling = NullValueHandling.Ignore)]
	public short TargetShardId { get; set; }

	[JsonProperty("affectedEntityCount", NullValueHandling = NullValueHandling.Ignore)]
	public int AffectedEntityCount { get; set; }

	[JsonProperty("migratedAt", NullValueHandling = NullValueHandling.Ignore)]
	public DateTime? MigratedAt { get; set; }

	[JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
	public string Reason { get; set; }
}
