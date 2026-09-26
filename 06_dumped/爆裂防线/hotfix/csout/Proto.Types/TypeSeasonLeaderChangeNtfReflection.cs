using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonLeaderChangeNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonLeaderChangeNtfReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlNlYXNvbkxlYWRlckNoYW5nZU50Zi5wcm90bxIFdHlwZXMaF2Vu" + "dW0uRVNlYXNvbkZvcmNlLnByb3RvImoKFVNlYXNvbkxlYWRlckNoYW5nZU50" + "ZhIrCgdGb3JjZUlkGAEgASgOMhouRVNlYXNvbkZvcmNlLkVTZWFzb25Gb3Jj" + "ZRIRCglPbGRMZWFkZXIYAiABKAMSEQoJTmV3TGVhZGVyGAMgASgDQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESeasonForceReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonLeaderChangeNtf), (MessageParser)(object)SeasonLeaderChangeNtf.Parser, new string[3] { "ForceId", "OldLeader", "NewLeader" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
