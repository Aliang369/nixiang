using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldLairFightRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldLairFightRecordDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLldvcmxkTGFpckZpZ2h0UmVjb3JkRGV0YWlsLnByb3RvEgV0eXBl" + "cxoXdHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8ibAoaV29ybGRMYWlyRmlnaHRS" + "ZWNvcmREZXRhaWwSKQoMUm9sZUJhc2VJbmZvGAEgASgLMhMudHlwZXMuUm9s" + "ZUJhc2VJbmZvEhEKCUtpbGxDb3VudBgCIAEoBRIQCghSZWNvcmRUbRgDIAEo" + "A0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldLairFightRecordDetail), (MessageParser)(object)WorldLairFightRecordDetail.Parser, new string[3] { "RoleBaseInfo", "KillCount", "RecordTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
