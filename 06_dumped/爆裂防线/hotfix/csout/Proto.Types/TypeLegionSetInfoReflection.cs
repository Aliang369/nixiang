using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionSetInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionSetInfoReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkxlZ2lvblNldEluZm8ucHJvdG8SBXR5cGVzIq4BCg1MZWdpb25T" + "ZXRJbmZvEhAKCExlYWRlcklkGAEgASgDEg8KB0FkdmlzZXIYAiADKAMSEQoJ" + "S2lja0NvdW50GAMgASgFEhYKDkxlYWRlckNoYW5nZUNEGAQgASgDEhIKCkxl" + "Z2lvblNpZ24YBSABKAkSEQoJTGVnaW9uRXhwGAYgASgFEhMKC0RvbmF0ZUNv" + "dW50GAcgASgFEhMKC1Jld2FyZENvdW50GAggASgFQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionSetInfo), (MessageParser)(object)LegionSetInfo.Parser, new string[8] { "LeaderId", "Adviser", "KickCount", "LeaderChangeCD", "LegionSign", "LegionExp", "DonateCount", "RewardCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
