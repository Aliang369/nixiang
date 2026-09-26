using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarTaskAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarTaskAwardReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkxlZ2lvbldhclRhc2tBd2FyZFJlcS5wcm90bxIFdHlwZXMaHWVu" + "dW0uRUxlZ2lvbldhclRhc2tUeXBlLnByb3RvIlEKFUxlZ2lvbldhclRhc2tB" + "d2FyZFJlcRI4CghUYXNrVHlwZRgBIAEoDjImLkVMZWdpb25XYXJUYXNrVHlw" + "ZS5FTGVnaW9uV2FyVGFza1R5cGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumELegionWarTaskTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarTaskAwardReq), (MessageParser)(object)LegionWarTaskAwardReq.Parser, new string[1] { "TaskType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
