using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeServerListReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServerListReqReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlNlcnZlckxpc3RSZXEucHJvdG8SBXR5cGVzGhdlbnVtLkVDaGFu" + "bmVsVHlwZS5wcm90bxoYZW51bS5FUGxhdGZvcm1UeXBlLnByb3RvIn0KDVNl" + "cnZlckxpc3RSZXESLgoIUGxhdGZvcm0YASABKA4yHC5FUGxhdGZvcm1UeXBl" + "LkVQbGF0Zm9ybVR5cGUSLgoKQXBwQ2hhbm5lbBgCIAEoDjIaLkVDaGFubmVs" + "VHlwZS5FQ2hhbm5lbFR5cGUSDAoER3VpZBgDIAEoA0IuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEChannelTypeReflection.Descriptor,
			EnumEPlatformTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServerListReq), (MessageParser)(object)ServerListReq.Parser, new string[3] { "Platform", "AppChannel", "Guid" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
