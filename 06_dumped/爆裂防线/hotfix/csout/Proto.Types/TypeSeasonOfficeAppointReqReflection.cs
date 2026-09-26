using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonOfficeAppointReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonOfficeAppointReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlNlYXNvbk9mZmljZUFwcG9pbnRSZXEucHJvdG8SBXR5cGVzGhhl" + "bnVtLkVTZWFzb25PZmZpY2UucHJvdG8iXAoWU2Vhc29uT2ZmaWNlQXBwb2lu" + "dFJlcRIUCgxUYXJnZXRSb2xlSWQYASABKAMSLAoGT2ZmaWNlGAIgASgOMhwu" + "RVNlYXNvbk9mZmljZS5FU2Vhc29uT2ZmaWNlQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESeasonOfficeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonOfficeAppointReq), (MessageParser)(object)SeasonOfficeAppointReq.Parser, new string[2] { "TargetRoleId", "Office" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
