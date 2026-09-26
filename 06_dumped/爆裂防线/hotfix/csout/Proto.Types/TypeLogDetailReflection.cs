using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLogDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLogDetailReflection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChR0eXBlLkxvZ0RldGFpbC5wcm90bxIFdHlwZXMaGHR5cGUuQ29udGVudERl" + "dGFpbC5wcm90byJBCglMb2dEZXRhaWwSDQoFTG9nVG0YASABKAMSJQoHQ29u" + "dGVudBgCIAEoCzIULnR5cGVzLkNvbnRlbnREZXRhaWxCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeContentDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LogDetail), (MessageParser)(object)LogDetail.Parser, new string[2] { "LogTm", "Content" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
