using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarPointPlayerDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarPointPlayerDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkxlZ2lvbldhclBvaW50UGxheWVyRGV0YWlsLnByb3RvEgV0eXBl" + "cxohdHlwZS5MZWdpb25XYXJSb2xlU3RhdGVJbmZvLnByb3RvGhd0eXBlLlJv" + "bGVCYXNlSW5mby5wcm90byJ9ChpMZWdpb25XYXJQb2ludFBsYXllckRldGFp" + "bBIpCgxSb2xlQmFzZUluZm8YASABKAsyEy50eXBlcy5Sb2xlQmFzZUluZm8S" + "NAoNUm9sZVN0YXRlSW5mbxgCIAEoCzIdLnR5cGVzLkxlZ2lvbldhclJvbGVT" + "dGF0ZUluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLegionWarRoleStateInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarPointPlayerDetail), (MessageParser)(object)LegionWarPointPlayerDetail.Parser, new string[2] { "RoleBaseInfo", "RoleStateInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
