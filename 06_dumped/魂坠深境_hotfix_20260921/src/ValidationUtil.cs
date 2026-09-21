using System.Threading.Tasks;
using ChillyRoom.Censor.V1;
using Generated.ChillyRoomSdkClient;

public static class ValidationUtil
{
	public static async Task<TextValidationResult> Validate(string s, TextUsageHint hint)
	{
		return (await ChillyRoomService.GetSdk().CensorValidation.ValidateTextAsync(new ValidateTextRequest
		{
			UsageHint = hint,
			Text = s
		})).Result;
	}
}
