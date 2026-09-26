using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementChapterThreeBarAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementChapterThreeBarAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLkFkdmVydGlzZW1lbnRDaGFwdGVyVGhyZWVCYXJBY2sucHJvdG8S" + "BXR5cGVzGhx0eXBlLkFkdmVydGlzZW1lbnRJbmZvLnByb3RvGhZ0eXBlLkF3" + "YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aFnR5cGUu" + "Sm91cm5leUluZm8ucHJvdG8izQEKH0FkdmVydGlzZW1lbnRDaGFwdGVyVGhy" + "ZWVCYXJBY2sSMwoRQWR2ZXJ0aXNlbWVudEluZm8YASABKAsyGC50eXBlcy5B" + "ZHZlcnRpc2VtZW50SW5mbxInCgtKb3VybmV5SW5mbxgCIAEoCzISLnR5cGVz" + "LkpvdXJuZXlJbmZvEiUKCUF3YXJkTGlzdBgDIAMoCzISLnR5cGVzLkF3YXJk" + "RGV0YWlsEiUKCkRhdGFDaGFuZ2UYBCABKAsyES50eXBlcy5EYXRhQ2hhbmdl" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAdvertisementInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeJourneyInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementChapterThreeBarAck), (MessageParser)(object)AdvertisementChapterThreeBarAck.Parser, new string[4] { "AdvertisementInfo", "JourneyInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
