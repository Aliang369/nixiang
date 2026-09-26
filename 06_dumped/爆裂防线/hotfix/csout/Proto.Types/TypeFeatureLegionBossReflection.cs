using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFeatureLegionBossReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFeatureLegionBossReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkZlYXR1cmVMZWdpb25Cb3NzLnByb3RvEgV0eXBlcxoZdHlwZS5M" + "ZWdpb25Cb3NzSW5mby5wcm90bxoZdHlwZS5MZWdpb25TZWxmSW5mby5wcm90" + "byKDAQoRRmVhdHVyZUxlZ2lvbkJvc3MSLQoOTGVnaW9uU2VsZkluZm8YASAB" + "KAsyFS50eXBlcy5MZWdpb25TZWxmSW5mbxItCg5MZWdpb25Cb3NzSW5mbxgC" + "IAEoCzIVLnR5cGVzLkxlZ2lvbkJvc3NJbmZvEhAKCFNlbGZSYW5rGAMgASgF" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLegionBossInfoReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FeatureLegionBoss), (MessageParser)(object)FeatureLegionBoss.Parser, new string[3] { "LegionSelfInfo", "LegionBossInfo", "SelfRank" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
