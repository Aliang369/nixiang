using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEndlessMineMapCellDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessMineMapCellDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkVuZGxlc3NNaW5lTWFwQ2VsbERldGFpbC5wcm90bxIFdHlwZXMa" + "FWVudW0uRUJvb2xTdGF0ZS5wcm90bxofZW51bS5FRW5kbGVzc01pbmVQbG90" + "VHlwZS5wcm90byKaAQoYRW5kbGVzc01pbmVNYXBDZWxsRGV0YWlsEg0KBUlu" + "ZGV4GAEgASgFEjgKBFBsb3QYAiABKA4yKi5FRW5kbGVzc01pbmVQbG90VHlw" + "ZS5FRW5kbGVzc01pbmVQbG90VHlwZRIMCgRQcm9nGAMgASgCEicKB1Zpc2li" + "bGUYBCABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3RhdGVCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumEEndlessMinePlotTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessMineMapCellDetail), (MessageParser)(object)EndlessMineMapCellDetail.Parser, new string[4] { "Index", "Plot", "Prog", "Visible" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
