using System.Collections.Generic;

public class EmojiMapper
{
	private Dictionary<string, string> emojiDictionary;

	public EmojiMapper()
	{
		emojiDictionary = new Dictionary<string, string>
		{
			{ "::010::", "ui/image/chat/icon_010.png" },
			{ "::020::", "ui/image/chat/icon_020.png" },
			{ "::030::", "ui/image/chat/icon_030.png" },
			{ "::040::", "ui/image/chat/icon_040.png" },
			{ "::050::", "ui/image/chat/icon_050.png" },
			{ "::060::", "ui/image/chat/icon_060.png" },
			{ "::070::", "ui/image/chat/icon_070.png" },
			{ "::080::", "ui/image/chat/icon_080.png" },
			{ "::090::", "ui/image/chat/icon_090.png" },
			{ "::0100::", "ui/image/chat/icon_0100.png" },
			{ "::110::", "ui/image/chat/icon_110.png" },
			{ "::120::", "ui/image/chat/icon_120.png" },
			{ "::130::", "ui/image/chat/icon_130.png" },
			{ "::140::", "ui/image/chat/icon_140.png" },
			{ "::150::", "ui/image/chat/icon_150.png" },
			{ "::160::", "ui/image/chat/icon_160.png" },
			{ "::170::", "ui/image/chat/icon_170.png" },
			{ "::180::", "ui/image/chat/icon_180.png" },
			{ "::190::", "ui/image/chat/icon_190.png" },
			{ "::1100::", "ui/image/chat/icon_1100.png" },
			{ "::210::", "ui/image/chat/icon_210.png" },
			{ "::220::", "ui/image/chat/icon_220.png" },
			{ "::230::", "ui/image/chat/icon_230.png" },
			{ "::240::", "ui/image/chat/icon_240.png" },
			{ "::250::", "ui/image/chat/icon_250.png" },
			{ "::260::", "ui/image/chat/icon_260.png" },
			{ "::270::", "ui/image/chat/icon_270.png" },
			{ "::280::", "ui/image/chat/icon_280.png" },
			{ "::290::", "ui/image/chat/icon_290.png" },
			{ "::2100::", "ui/image/chat/icon_2100.png" },
			{ "::310::", "ui/image/chat/icon_310.png" },
			{ "::320::", "ui/image/chat/icon_320.png" },
			{ "::330::", "ui/image/chat/icon_330.png" },
			{ "::340::", "ui/image/chat/icon_340.png" },
			{ "::350::", "ui/image/chat/icon_350.png" },
			{ "::360::", "ui/image/chat/icon_360.png" },
			{ "::370::", "ui/image/chat/icon_370.png" },
			{ "::380::", "ui/image/chat/icon_380.png" },
			{ "::390::", "ui/image/chat/icon_390.png" },
			{ "::3100::", "ui/image/chat/icon_3100.png" },
			{ "::410::", "ui/image/chat/icon_410.png" },
			{ "::420::", "ui/image/chat/icon_420.png" },
			{ "::430::", "ui/image/chat/icon_430.png" },
			{ "::440::", "ui/image/chat/icon_440.png" },
			{ "::450::", "ui/image/chat/icon_450.png" },
			{ "::460::", "ui/image/chat/icon_460.png" },
			{ "::470::", "ui/image/chat/icon_470.png" },
			{ "::480::", "ui/image/chat/icon_480.png" },
			{ "::490::", "ui/image/chat/icon_490.png" },
			{ "::4100::", "ui/image/chat/icon_4100.png" },
			{ "::510::", "ui/image/chat/icon_510.png" },
			{ "::520::", "ui/image/chat/icon_520.png" },
			{ "::530::", "ui/image/chat/icon_530.png" },
			{ "::540::", "ui/image/chat/icon_540.png" },
			{ "::550::", "ui/image/chat/icon_550.png" },
			{ "::560::", "ui/image/chat/icon_560.png" },
			{ "::570::", "ui/image/chat/icon_570.png" },
			{ "::580::", "ui/image/chat/icon_580.png" },
			{ "::590::", "ui/image/chat/icon_590.png" },
			{ "::5100::", "ui/image/chat/icon_5100.png" }
		};
	}

	public string GetImagePath(string emojiCode)
	{
		if (emojiDictionary.TryGetValue(emojiCode, out var value))
		{
			return value;
		}
		return "ui/image/chat/icon_030.png";
	}
}
