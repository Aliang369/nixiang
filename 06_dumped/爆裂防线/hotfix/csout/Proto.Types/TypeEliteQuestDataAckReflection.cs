using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEliteQuestDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEliteQuestDataAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkVsaXRlUXVlc3REYXRhQWNrLnByb3RvEgV0eXBlcxoZdHlwZS5F" + "bGl0ZVF1ZXN0SW5mby5wcm90bxoTdHlwZS5IZWxwSW5mby5wcm90byJlChFF" + "bGl0ZVF1ZXN0RGF0YUFjaxItCg5FbGl0ZVF1ZXN0SW5mbxgBIAEoCzIVLnR5" + "cGVzLkVsaXRlUXVlc3RJbmZvEiEKCEhlbHBJbmZvGAIgASgLMg8udHlwZXMu" + "SGVscEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeEliteQuestInfoReflection.Descriptor,
			TypeHelpInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EliteQuestDataAck), (MessageParser)(object)EliteQuestDataAck.Parser, new string[2] { "EliteQuestInfo", "HelpInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
