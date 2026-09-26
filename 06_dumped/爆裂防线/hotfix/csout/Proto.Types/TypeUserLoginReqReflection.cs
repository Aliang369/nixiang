using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeUserLoginReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeUserLoginReqReflection()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLlVzZXJMb2dpblJlcS5wcm90bxIFdHlwZXMaF2VudW0uRUNoYW5u" + "ZWxUeXBlLnByb3RvGhhlbnVtLkVMYW5ndWFnZVR5cGUucHJvdG8aGGVudW0u" + "RVBsYXRmb3JtVHlwZS5wcm90byKCAgoMVXNlckxvZ2luUmVxEi4KCFBsYXRm" + "b3JtGAEgASgOMhwuRVBsYXRmb3JtVHlwZS5FUGxhdGZvcm1UeXBlEi4KCkFw" + "cENoYW5uZWwYAiABKA4yGi5FQ2hhbm5lbFR5cGUuRUNoYW5uZWxUeXBlEjAK" + "DExvZ2luQ2hhbm5lbBgDIAEoDjIaLkVDaGFubmVsVHlwZS5FQ2hhbm5lbFR5" + "cGUSEgoKTG9naW5Ub2tlbhgEIAEoCRIQCghEZXZpY2VJZBgFIAEoCRIqCgRM" + "YW5nGAYgASgOMhwuRUxhbmd1YWdlVHlwZS5FTGFuZ3VhZ2VUeXBlEg4KBlJl" + "Z2lvbhgHIAEoCUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEChannelTypeReflection.Descriptor,
			EnumELanguageTypeReflection.Descriptor,
			EnumEPlatformTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(UserLoginReq), (MessageParser)(object)UserLoginReq.Parser, new string[7] { "Platform", "AppChannel", "LoginChannel", "LoginToken", "DeviceId", "Lang", "Region" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
