using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonRecruitAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonRecruitAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLlNlYXNvblJlY3J1aXRBY2sucHJvdG8SBXR5cGVzIm4KEFNlYXNv" + "blJlY3J1aXRBY2sSDQoFTWVyaXQYASABKAUSEgoKR3VhcmRDb3VudBgCIAEo" + "BRIQCghHdWFyZE1heBgDIAEoBRIRCglEYWlseVVzZWQYBCABKAUSEgoKRGFp" + "bHlMaW1pdBgFIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonRecruitAck), (MessageParser)(object)SeasonRecruitAck.Parser, new string[5] { "Merit", "GuardCount", "GuardMax", "DailyUsed", "DailyLimit" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
