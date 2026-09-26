using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRankFirstReachDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRankFirstReachDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlJhbmtGaXJzdFJlYWNoRGV0YWlsLnByb3RvEgV0eXBlcxoXdHlw" + "ZS5Sb2xlQmFzZUluZm8ucHJvdG8iYwoUUmFua0ZpcnN0UmVhY2hEZXRhaWwS" + "DwoHUmVhY2hJZBgBIAEoBRIPCgdSZWFjaFRtGAIgASgDEikKDFJvbGVCYXNl" + "SW5mbxgDIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mb0IuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RankFirstReachDetail), (MessageParser)(object)RankFirstReachDetail.Parser, new string[3] { "ReachId", "ReachTm", "RoleBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
