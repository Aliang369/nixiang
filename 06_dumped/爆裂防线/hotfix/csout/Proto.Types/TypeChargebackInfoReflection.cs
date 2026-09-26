using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeChargebackInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeChargebackInfoReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkNoYXJnZWJhY2tJbmZvLnByb3RvEgV0eXBlcxobZW51bS5FQ2hh" + "cmdlYmFja1N0YXRlLnByb3RvIo8BCg5DaGFyZ2ViYWNrSW5mbxIxCgVTdGF0" + "ZRgBIAEoDjIiLkVDaGFyZ2ViYWNrU3RhdGUuRUNoYXJnZWJhY2tTdGF0ZRIR" + "CglWb2lkQ291bnQYAiABKAISEgoKVm9pZEFtb3VudBgDIAEoAhIRCglSZXBh" + "eW1lbnQYBCABKAISEAoIRXhwaXJlVG0YBSABKANCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEChargebackStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ChargebackInfo), (MessageParser)(object)ChargebackInfo.Parser, new string[5] { "State", "VoidCount", "VoidAmount", "Repayment", "ExpireTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
