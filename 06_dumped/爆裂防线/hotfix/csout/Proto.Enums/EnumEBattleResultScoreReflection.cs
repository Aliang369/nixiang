using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEBattleResultScoreReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEBattleResultScoreReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch1lbnVtLkVCYXR0bGVSZXN1bHRTY29yZS5wcm90bxISRUJhdHRsZVJlc3Vs" + "dFNjb3JlKjwKEkVCYXR0bGVSZXN1bHRTY29yZRIICgRGYWlsEAASCAoEV2lu" + "MRABEggKBFdpbjIQAhIICgRXaW4zEANCLloJLi87cHJvdG9jqgILUHJvdG8u" + "RW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EBattleResultScore) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
