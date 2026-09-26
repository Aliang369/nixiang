using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldLairHelpFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldLairHelpFightStartAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLldvcmxkTGFpckhlbHBGaWdodFN0YXJ0QWNrLnByb3RvEgV0eXBl" + "cxoVdHlwZS5CYXR0bGVJbmZvLnByb3RvIlEKGldvcmxkTGFpckhlbHBGaWdo" + "dFN0YXJ0QWNrEgwKBFVVSUQYASABKAkSJQoKQmF0dGxlSW5mbxgCIAEoCzIR" + "LnR5cGVzLkJhdHRsZUluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldLairHelpFightStartAck), (MessageParser)(object)WorldLairHelpFightStartAck.Parser, new string[2] { "UUID", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
