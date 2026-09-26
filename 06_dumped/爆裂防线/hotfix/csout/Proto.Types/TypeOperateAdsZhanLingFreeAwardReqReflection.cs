using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsZhanLingFreeAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsZhanLingFreeAwardReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVBZHNaaGFuTGluZ0ZyZWVBd2FyZFJlcS5wcm90bxIF" + "dHlwZXMiPQoeT3BlcmF0ZUFkc1poYW5MaW5nRnJlZUF3YXJkUmVxEgwKBFVV" + "SUQYASABKAkSDQoFTGV2ZWwYAiABKAVCLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsZhanLingFreeAwardReq), (MessageParser)(object)OperateAdsZhanLingFreeAwardReq.Parser, new string[2] { "UUID", "Level" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
