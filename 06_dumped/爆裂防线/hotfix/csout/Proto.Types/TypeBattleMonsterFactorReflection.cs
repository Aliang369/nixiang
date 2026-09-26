using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleMonsterFactorReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleMonsterFactorReflection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkJhdHRsZU1vbnN0ZXJGYWN0b3IucHJvdG8SBXR5cGVzIlEKE0Jh" + "dHRsZU1vbnN0ZXJGYWN0b3ISFAoMRGFtYWdlRmFjdG9yGAEgASgCEhAKCEhw" + "RmFjdG9yGAIgASgCEhIKCkRpZmZGYWN0b3IYAyABKAJCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleMonsterFactor), (MessageParser)(object)BattleMonsterFactor.Parser, new string[3] { "DamageFactor", "HpFactor", "DiffFactor" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
