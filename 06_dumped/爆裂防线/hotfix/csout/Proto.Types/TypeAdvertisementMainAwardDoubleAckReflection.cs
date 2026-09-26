using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementMainAwardDoubleAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementMainAwardDoubleAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLkFkdmVydGlzZW1lbnRNYWluQXdhcmREb3VibGVBY2sucHJvdG8S" + "BXR5cGVzGhx0eXBlLkFkdmVydGlzZW1lbnRJbmZvLnByb3RvGhZ0eXBlLkF3" + "YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aGHR5cGUu" + "TWFpblF1ZXN0SW5mby5wcm90byLRAQofQWR2ZXJ0aXNlbWVudE1haW5Bd2Fy" + "ZERvdWJsZUFjaxIzChFBZHZlcnRpc2VtZW50SW5mbxgBIAEoCzIYLnR5cGVz" + "LkFkdmVydGlzZW1lbnRJbmZvEisKDU1haW5RdWVzdEluZm8YAiABKAsyFC50" + "eXBlcy5NYWluUXVlc3RJbmZvEiUKCUF3YXJkTGlzdBgDIAMoCzISLnR5cGVz" + "LkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYBCABKAsyES50eXBlcy5EYXRh" + "Q2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAdvertisementInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMainQuestInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementMainAwardDoubleAck), (MessageParser)(object)AdvertisementMainAwardDoubleAck.Parser, new string[4] { "AdvertisementInfo", "MainQuestInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
