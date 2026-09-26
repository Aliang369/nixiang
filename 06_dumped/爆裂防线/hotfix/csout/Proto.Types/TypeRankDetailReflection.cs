using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRankDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRankDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLlJhbmtEZXRhaWwucHJvdG8SBXR5cGVzGhN0eXBlLlJhbmtEYXRh" + "LnByb3RvGhd0eXBlLlJvbGVCYXNlSW5mby5wcm90byJaCgpSYW5rRGV0YWls" + "EikKDFJvbGVCYXNlSW5mbxgBIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mbxIh" + "CghSYW5rRGF0YRgCIAEoCzIPLnR5cGVzLlJhbmtEYXRhQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeRankDataReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RankDetail), (MessageParser)(object)RankDetail.Parser, new string[2] { "RoleBaseInfo", "RankData" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
