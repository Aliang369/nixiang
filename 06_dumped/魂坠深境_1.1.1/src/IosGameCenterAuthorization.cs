using System.Threading.Tasks;
using UnityEngine;

public static class IosGameCenterAuthorization
{
	public readonly struct Result
	{
		public readonly bool IsAuthenticated;

		public readonly bool IsSkipped;

		public readonly string GamePlayerId;

		public readonly string DisplayName;

		public readonly string ErrorMessage;

		private Result(bool isAuthenticated, bool isSkipped, string gamePlayerId, string displayName, string errorMessage)
		{
			IsAuthenticated = isAuthenticated;
			IsSkipped = isSkipped;
			GamePlayerId = gamePlayerId;
			DisplayName = displayName;
			ErrorMessage = errorMessage;
		}

		public static Result Authenticated(string gamePlayerId, string displayName)
		{
			return new Result(isAuthenticated: true, isSkipped: false, gamePlayerId, displayName, string.Empty);
		}

		public static Result Skipped(string message)
		{
			return new Result(isAuthenticated: false, isSkipped: true, string.Empty, string.Empty, message);
		}

		public static Result Failed(string message)
		{
			return new Result(isAuthenticated: false, isSkipped: false, string.Empty, string.Empty, message);
		}
	}

	private static bool IsIosPlayer => false;

	public static Task<Result> AuthorizeAsync()
	{
		Debug.Log("[IosGameCenterAuthorization] AuthorizeAsync() - begin");
		if (!IsIosPlayer)
		{
			Debug.Log("[IosGameCenterAuthorization] AuthorizeAsync() - skipped because current platform is not iOS player");
			return Task.FromResult(Result.Skipped("Game Center authorization is only required on iOS player."));
		}
		return Task.FromResult(Result.Skipped("Game Center authorization is only required on iOS player."));
	}
}
