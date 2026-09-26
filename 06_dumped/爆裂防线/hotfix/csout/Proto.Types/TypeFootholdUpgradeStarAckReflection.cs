using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFootholdUpgradeStarAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFootholdUpgradeStarAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkZvb3Rob2xkVXBncmFkZVN0YXJBY2sucHJvdG8SBXR5cGVzGhV0" + "eXBlLkRhdGFDaGFuZ2UucHJvdG8aF3R5cGUuRm9vdGhvbGRJbmZvLnByb3Rv" + "ImoKFkZvb3Rob2xkVXBncmFkZVN0YXJBY2sSKQoMRm9vdGhvbGRJbmZvGAEg" + "ASgLMhMudHlwZXMuRm9vdGhvbGRJbmZvEiUKCkRhdGFDaGFuZ2UYAiABKAsy" + "ES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeFootholdInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FootholdUpgradeStarAck), (MessageParser)(object)FootholdUpgradeStarAck.Parser, new string[2] { "FootholdInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
