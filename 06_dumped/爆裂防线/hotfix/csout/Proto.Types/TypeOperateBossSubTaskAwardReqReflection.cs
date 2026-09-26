using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOperateBossSubTaskAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateBossSubTaskAwardReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLk9wZXJhdGVCb3NzU3ViVGFza0F3YXJkUmVxLnByb3RvEgV0eXBl" + "cxoUZW51bS5FVGFza1R5cGUucHJvdG8iUgoaT3BlcmF0ZUJvc3NTdWJUYXNr" + "QXdhcmRSZXESDAoEVVVJRBgBIAEoCRImCghUYXNrVHlwZRgCIAEoDjIULkVU" + "YXNrVHlwZS5FVGFza1R5cGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumETaskTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateBossSubTaskAwardReq), (MessageParser)(object)OperateBossSubTaskAwardReq.Parser, new string[2] { "UUID", "TaskType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
