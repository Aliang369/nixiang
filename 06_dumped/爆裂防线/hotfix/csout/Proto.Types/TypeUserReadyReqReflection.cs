using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeUserReadyReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeUserReadyReqReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLlVzZXJSZWFkeVJlcS5wcm90bxIFdHlwZXMaF2VudW0uRUNoYW5u" + "ZWxUeXBlLnByb3RvGhhlbnVtLkVMYW5ndWFnZVR5cGUucHJvdG8aGGVudW0u" + "RVBsYXRmb3JtVHlwZS5wcm90byK4AQoMVXNlclJlYWR5UmVxEi4KCFBsYXRm" + "b3JtGAEgASgOMhwuRVBsYXRmb3JtVHlwZS5FUGxhdGZvcm1UeXBlEi4KCkFw" + "cENoYW5uZWwYAiABKA4yGi5FQ2hhbm5lbFR5cGUuRUNoYW5uZWxUeXBlEgwK" + "BEd1aWQYAyABKAMSKgoETGFuZxgEIAEoDjIcLkVMYW5ndWFnZVR5cGUuRUxh" + "bmd1YWdlVHlwZRIOCgZUaWNrZXQYBSABKAlCLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEChannelTypeReflection.Descriptor,
			EnumELanguageTypeReflection.Descriptor,
			EnumEPlatformTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(UserReadyReq), (MessageParser)(object)UserReadyReq.Parser, new string[5] { "Platform", "AppChannel", "Guid", "Lang", "Ticket" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
