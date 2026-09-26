using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonAirdropDecryptAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonAirdropDecryptAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLlNlYXNvbkFpcmRyb3BEZWNyeXB0QWNrLnByb3RvEgV0eXBlcxoc" + "dHlwZS5TZWFzb25BaXJkcm9wSW5mby5wcm90bxocdHlwZS5TZWFzb25BaXJk" + "cm9wUHJvZy5wcm90byJpChdTZWFzb25BaXJkcm9wRGVjcnlwdEFjaxImCgRJ" + "bmZvGAEgASgLMhgudHlwZXMuU2Vhc29uQWlyZHJvcEluZm8SJgoEUHJvZxgC" + "IAEoCzIYLnR5cGVzLlNlYXNvbkFpcmRyb3BQcm9nQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeSeasonAirdropInfoReflection.Descriptor,
			TypeSeasonAirdropProgReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonAirdropDecryptAck), (MessageParser)(object)SeasonAirdropDecryptAck.Parser, new string[2] { "Info", "Prog" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
