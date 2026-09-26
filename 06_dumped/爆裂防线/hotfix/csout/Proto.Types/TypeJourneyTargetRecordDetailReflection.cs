using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeJourneyTargetRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyTargetRecordDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkpvdXJuZXlUYXJnZXRSZWNvcmREZXRhaWwucHJvdG8SBXR5cGVz" + "GhdlbnVtLkVDb21tb25TdGF0ZS5wcm90byJlChlKb3VybmV5VGFyZ2V0UmVj" + "b3JkRGV0YWlsEg0KBUluZGV4GAEgASgFEg4KBlRhcmdldBgCIAEoBRIpCgVT" + "dGF0ZRgDIAEoDjIaLkVDb21tb25TdGF0ZS5FQ29tbW9uU3RhdGVCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumECommonStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyTargetRecordDetail), (MessageParser)(object)JourneyTargetRecordDetail.Parser, new string[3] { "Index", "Target", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
