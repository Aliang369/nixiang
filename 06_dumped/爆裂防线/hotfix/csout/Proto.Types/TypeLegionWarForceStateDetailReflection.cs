using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarForceStateDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarForceStateDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkxlZ2lvbldhckZvcmNlU3RhdGVEZXRhaWwucHJvdG8SBXR5cGVz" + "GhplbnVtLkVMZWdpb25XYXJGb3JjZS5wcm90byJ1ChlMZWdpb25XYXJGb3Jj" + "ZVN0YXRlRGV0YWlsEjEKB0ZvcmNlSWQYASABKA4yIC5FTGVnaW9uV2FyRm9y" + "Y2UuRUxlZ2lvbldhckZvcmNlEhIKClNjb3JlQ291bnQYAiABKAUSEQoJU2Nv" + "cmVSYXRlGAMgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJn" + "YW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumELegionWarForceReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarForceStateDetail), (MessageParser)(object)LegionWarForceStateDetail.Parser, new string[3] { "ForceId", "ScoreCount", "ScoreRate" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
