using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementBossSubRouletteAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementBossSubRouletteAckReflection()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLkFkdmVydGlzZW1lbnRCb3NzU3ViUm91bGV0dGVBY2sucHJvdG8S" + "BXR5cGVzGhx0eXBlLkFkdmVydGlzZW1lbnRJbmZvLnByb3RvGhZ0eXBlLkF3" + "YXJkRGV0YWlsLnByb3RvGhx0eXBlLkJvc3NTdWJqZWN0RGV0YWlsLnByb3Rv" + "GhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8i6AEKH0FkdmVydGlzZW1lbnRCb3Nz" + "U3ViUm91bGV0dGVBY2sSMwoRQWR2ZXJ0aXNlbWVudEluZm8YASABKAsyGC50" + "eXBlcy5BZHZlcnRpc2VtZW50SW5mbxIxCg9Cb3NzU3ViamVjdExpc3QYAiAD" + "KAsyGC50eXBlcy5Cb3NzU3ViamVjdERldGFpbBIPCgdIaXRMaXN0GAMgAygF" + "EiUKCUF3YXJkTGlzdBgEIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRh" + "dGFDaGFuZ2UYBSABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAdvertisementInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeBossSubjectDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementBossSubRouletteAck), (MessageParser)(object)AdvertisementBossSubRouletteAck.Parser, new string[5] { "AdvertisementInfo", "BossSubjectList", "HitList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
