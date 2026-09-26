using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMechaUpgradeStarAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMechaUpgradeStarAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLk1lY2hhVXBncmFkZVN0YXJBY2sucHJvdG8SBXR5cGVzGhV0eXBl" + "LkRhdGFDaGFuZ2UucHJvdG8aFHR5cGUuTWVjaGFJbmZvLnByb3RvImEKE01l" + "Y2hhVXBncmFkZVN0YXJBY2sSIwoJTWVjaGFJbmZvGAEgASgLMhAudHlwZXMu" + "TWVjaGFJbmZvEiUKCkRhdGFDaGFuZ2UYAiABKAsyES50eXBlcy5EYXRhQ2hh" + "bmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeMechaInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MechaUpgradeStarAck), (MessageParser)(object)MechaUpgradeStarAck.Parser, new string[2] { "MechaInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
