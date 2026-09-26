using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsGiftAdAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsGiftAdAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLk9wZXJhdGVBZHNHaWZ0QWRBd2FyZEFjay5wcm90bxIFdHlwZXMa" + "GHR5cGUuQWRzR2lmdERldGFpbC5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5w" + "cm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvIpMBChhPcGVyYXRlQWRzR2lm" + "dEFkQXdhcmRBY2sSKQoLQWRzR2lmdExpc3QYASADKAsyFC50eXBlcy5BZHNH" + "aWZ0RGV0YWlsEiUKCUF3YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0" + "YWlsEiUKCkRhdGFDaGFuZ2UYAyABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAdsGiftDetailReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsGiftAdAwardAck), (MessageParser)(object)OperateAdsGiftAdAwardAck.Parser, new string[3] { "AdsGiftList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
