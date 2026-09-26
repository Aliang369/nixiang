using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTrainCarriageDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTrainCarriageDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlRyYWluQ2FycmlhZ2VEZXRhaWwucHJvdG8SBXR5cGVzGhZ0eXBl" + "LkF3YXJkRGV0YWlsLnByb3RvGhd0eXBlLlJvbGVCYXNlSW5mby5wcm90byKP" + "AQoTVHJhaW5DYXJyaWFnZURldGFpbBIVCg1DYXJyaWFnZUluZGV4GAEgASgF" + "EiUKCUF3YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEikKDFJv" + "bGVCYXNlSW5mbxgDIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mbxIPCgdSb2JM" + "aXN0GAQgAygFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TrainCarriageDetail), (MessageParser)(object)TrainCarriageDetail.Parser, new string[4] { "CarriageIndex", "AwardList", "RoleBaseInfo", "RobList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
