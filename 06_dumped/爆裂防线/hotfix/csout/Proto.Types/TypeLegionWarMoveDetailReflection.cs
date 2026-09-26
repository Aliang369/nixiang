using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarMoveDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarMoveDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkxlZ2lvbldhck1vdmVEZXRhaWwucHJvdG8SBXR5cGVzGhd0eXBl" + "LlJvbGVCYXNlSW5mby5wcm90byJ7ChNMZWdpb25XYXJNb3ZlRGV0YWlsEikK" + "DFJvbGVCYXNlSW5mbxgBIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mbxITCgtG" + "cm9tUG9pbnRJZBgCIAEoBRIRCglUb1BvaW50SWQYAyABKAUSEQoJQXJyaXZh" + "bFRtGAQgASgDQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarMoveDetail), (MessageParser)(object)LegionWarMoveDetail.Parser, new string[4] { "RoleBaseInfo", "FromPointId", "ToPointId", "ArrivalTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
