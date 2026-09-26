using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMineWarIdleAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarIdleAwardAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLk1pbmVXYXJJZGxlQXdhcmRBY2sucHJvdG8SBXR5cGVzGhZ0eXBl" + "LkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aGnR5" + "cGUuTWluZVdhckJhc2VJbmZvLnByb3RvGhl0eXBlLk1pbmVXYXJNYXBJbmZv" + "LnByb3RvIsMBChNNaW5lV2FySWRsZUF3YXJkQWNrEi8KD01pbmVXYXJCYXNl" + "SW5mbxgBIAEoCzIWLnR5cGVzLk1pbmVXYXJCYXNlSW5mbxItCg5NaW5lV2Fy" + "TWFwSW5mbxgCIAEoCzIVLnR5cGVzLk1pbmVXYXJNYXBJbmZvEiUKCUF3YXJk" + "TGlzdBgDIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UY" + "BCABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor,
			TypeMineWarMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarIdleAwardAck), (MessageParser)(object)MineWarIdleAwardAck.Parser, new string[4] { "MineWarBaseInfo", "MineWarMapInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
