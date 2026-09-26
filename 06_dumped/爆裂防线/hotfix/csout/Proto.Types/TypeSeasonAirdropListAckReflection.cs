using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonAirdropListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonAirdropListAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlNlYXNvbkFpcmRyb3BMaXN0QWNrLnByb3RvEgV0eXBlcxocdHlw" + "ZS5TZWFzb25BaXJkcm9wSW5mby5wcm90byJ7ChRTZWFzb25BaXJkcm9wTGlz" + "dEFjaxImCgRMaXN0GAEgAygLMhgudHlwZXMuU2Vhc29uQWlyZHJvcEluZm8S" + "FQoNTmV4dFJlZnJlc2hUbRgCIAEoAxIQCghEYWlseVdpbhgDIAEoBRISCgpE" + "YWlseUxpbWl0GAQgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonAirdropInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonAirdropListAck), (MessageParser)(object)SeasonAirdropListAck.Parser, new string[4] { "List", "NextRefreshTm", "DailyWin", "DailyLimit" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
