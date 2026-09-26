using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBarTargetAwardReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBarTargetAwardReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkJhclRhcmdldEF3YXJkLnByb3RvEgV0eXBlcxoWdHlwZS5Bd2Fy" + "ZERldGFpbC5wcm90byJWCg5CYXJUYXJnZXRBd2FyZBINCgVDb3VudBgBIAEo" + "BRIOCgZUYXJnZXQYAiABKAUSJQoJQXdhcmRMaXN0GAMgAygLMhIudHlwZXMu" + "QXdhcmREZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BarTargetAward), (MessageParser)(object)BarTargetAward.Parser, new string[3] { "Count", "Target", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
