using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeAbnormalClientReportReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAbnormalClientReportReqReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkFibm9ybWFsQ2xpZW50UmVwb3J0UmVxLnByb3RvEgV0eXBlcxoU" + "ZW51bS5FRnVuY1R5cGUucHJvdG8ilgEKF0Fibm9ybWFsQ2xpZW50UmVwb3J0" + "UmVxEiYKCEZ1bmNUeXBlGAEgASgOMhQuRUZ1bmNUeXBlLkVGdW5jVHlwZRIT" + "CgtPcmlnaW5WYWx1ZRgCIAEoAhITCgtNb2RpZnlWYWx1ZRgDIAEoAhIVCg1D" + "bGllbnRWZXJzaW9uGAQgASgJEhIKClJlcG9ydERlc2MYBSABKAlCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEFuncTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AbnormalClientReportReq), (MessageParser)(object)AbnormalClientReportReq.Parser, new string[5] { "FuncType", "OriginValue", "ModifyValue", "ClientVersion", "ReportDesc" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
