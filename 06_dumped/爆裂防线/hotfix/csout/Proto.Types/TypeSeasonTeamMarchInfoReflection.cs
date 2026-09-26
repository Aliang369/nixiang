using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonTeamMarchInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonTeamMarchInfoReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlNlYXNvblRlYW1NYXJjaEluZm8ucHJvdG8SBXR5cGVzIpMBChNT" + "ZWFzb25UZWFtTWFyY2hJbmZvEg4KBkFjdGlvbhgBIAEoBRIQCghJc1JldHVy" + "bhgCIAEoBRINCgVGcm9tWBgDIAEoAhINCgVGcm9tWRgEIAEoAhILCgNUb1gY" + "BSABKAISCwoDVG9ZGAYgASgCEg8KB1N0YXJ0VG0YByABKAMSEQoJQXJyaXZh" + "bFRtGAggASgDQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonTeamMarchInfo), (MessageParser)(object)SeasonTeamMarchInfo.Parser, new string[8] { "Action", "IsReturn", "FromX", "FromY", "ToX", "ToY", "StartTm", "ArrivalTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
