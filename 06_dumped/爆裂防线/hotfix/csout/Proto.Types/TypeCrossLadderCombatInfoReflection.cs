using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeCrossLadderCombatInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderCombatInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkNyb3NzTGFkZGVyQ29tYmF0SW5mby5wcm90bxIFdHlwZXMaF2Vu" + "dW0uRUxhZGRlclN0YXRlLnByb3RvIm4KFUNyb3NzTGFkZGVyQ29tYmF0SW5m" + "bxIpCgVTdGF0ZRgBIAEoDjIaLkVMYWRkZXJTdGF0ZS5FTGFkZGVyU3RhdGUS" + "FAoMVmljdG9yeUNvdW50GAIgASgFEhQKDFVwZ3JhZGVTdGF0ZRgDIAMoBUIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumELadderStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderCombatInfo), (MessageParser)(object)CrossLadderCombatInfo.Parser, new string[3] { "State", "VictoryCount", "UpgradeState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
