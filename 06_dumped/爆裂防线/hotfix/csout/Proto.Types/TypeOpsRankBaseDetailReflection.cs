using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOpsRankBaseDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOpsRankBaseDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLk9wc1JhbmtCYXNlRGV0YWlsLnByb3RvEgV0eXBlcxoUZW51bS5F" + "UmFua1R5cGUucHJvdG8iTQoRT3BzUmFua0Jhc2VEZXRhaWwSJgoIUmFua1R5" + "cGUYASABKA4yFC5FUmFua1R5cGUuRVJhbmtUeXBlEhAKCFNldHRsZVRtGAIg" + "ASgDQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumERankTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OpsRankBaseDetail), (MessageParser)(object)OpsRankBaseDetail.Parser, new string[2] { "RankType", "SettleTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
