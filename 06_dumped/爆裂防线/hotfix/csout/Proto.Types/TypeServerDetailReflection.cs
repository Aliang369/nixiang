using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeServerDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServerDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLlNlcnZlckRldGFpbC5wcm90bxIFdHlwZXMaFWVudW0uRUJvb2xT" + "dGF0ZS5wcm90bxoXZW51bS5FU2VydmVyU3RhdGUucHJvdG8inwEKDFNlcnZl" + "ckRldGFpbBIOCgZTb3J0SWQYASABKAUSEAoIU2VydmVySWQYAiABKAUSEgoK" + "U2VydmVyTmFtZRgDIAEoCRIqCgZTdGF0dXMYBCABKA4yGi5FU2VydmVyU3Rh" + "dGUuRVNlcnZlclN0YXRlEi0KDVJlZ2lzdGVyU3RhdGUYBSABKA4yFi5FQm9v" + "bFN0YXRlLkVCb29sU3RhdGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumEServerStateReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServerDetail), (MessageParser)(object)ServerDetail.Parser, new string[5] { "SortId", "ServerId", "ServerName", "Status", "RegisterState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
