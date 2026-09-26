using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeClientDumpReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeClientDumpReqReflection()
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkNsaWVudER1bXBSZXEucHJvdG8SBXR5cGVzGhdlbnVtLkVDaGFu" + "bmVsVHlwZS5wcm90byLLAQoNQ2xpZW50RHVtcFJlcRIuCgpBcHBDaGFubmVs" + "GAEgASgOMhouRUNoYW5uZWxUeXBlLkVDaGFubmVsVHlwZRIKCgJPcxgCIAEo" + "CRIOCgZSb2xlSWQYAyABKAMSEAoIU2VydmVySWQYBCABKAUSEgoKQXBwVmVy" + "c2lvbhgFIAEoCRIVCg1BcHBSZXNWZXJzaW9uGAYgASgJEhEKCVN0YXJ0VGlt" + "ZRgHIAEoAxINCgVDb3VudBgIIAEoBRIPCgdDb250ZW50GAkgASgJQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEChannelTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ClientDumpReq), (MessageParser)(object)ClientDumpReq.Parser, new string[9] { "AppChannel", "Os", "RoleId", "ServerId", "AppVersion", "AppResVersion", "StartTime", "Count", "Content" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
