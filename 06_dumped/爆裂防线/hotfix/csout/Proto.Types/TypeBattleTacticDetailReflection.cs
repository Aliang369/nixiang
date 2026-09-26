using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleTacticDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleTacticDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkJhdHRsZVRhY3RpY0RldGFpbC5wcm90bxIFdHlwZXMiWAoSQmF0" + "dGxlVGFjdGljRGV0YWlsEhEKCVRhY3RpY1RpZBgBIAEoBRITCgtUYWN0aWNN" + "b2RJZBgCIAEoBRINCgVNb2RJZBgDIAEoBRILCgNQY3QYBCABKAJCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleTacticDetail), (MessageParser)(object)BattleTacticDetail.Parser, new string[4] { "TacticTid", "TacticModId", "ModId", "Pct" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
