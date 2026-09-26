using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMainQuestInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMainQuestInfoReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLk1haW5RdWVzdEluZm8ucHJvdG8SBXR5cGVzGhp0eXBlLk1haW5R" + "dWVzdERldGFpbC5wcm90byJ1Cg1NYWluUXVlc3RJbmZvEhEKCU1haW5MZXZl" + "bBgBIAEoBRIRCglOZXh0TGV2ZWwYAiABKAUSKQoJUXVlc3RMaXN0GAMgAygL" + "MhYudHlwZXMuTWFpblF1ZXN0RGV0YWlsEhMKC0RvdWJsZUF3YXJkGAQgASgF" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeMainQuestDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MainQuestInfo), (MessageParser)(object)MainQuestInfo.Parser, new string[4] { "MainLevel", "NextLevel", "QuestList", "DoubleAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
