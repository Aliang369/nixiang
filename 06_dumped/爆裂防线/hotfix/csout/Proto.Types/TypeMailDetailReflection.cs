using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeMailDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMailDetailReflection()
	{
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLk1haWxEZXRhaWwucHJvdG8SBXR5cGVzGhVlbnVtLkVCb29sU3Rh" + "dGUucHJvdG8aFWVudW0uRU1haWxMZXZlbC5wcm90bxoWdHlwZS5Bd2FyZERl" + "dGFpbC5wcm90byKuAgoKTWFpbERldGFpbBIOCgZNYWlsSWQYASABKAMSKQoJ" + "TWFpbExldmVsGAIgASgOMhYuRU1haWxMZXZlbC5FTWFpbExldmVsEgwKBFVV" + "SUQYAyABKAkSEAoIU2VuZGVySWQYBCABKAUSEgoKU2VuZGVyTmFtZRgFIAEo" + "CRINCgVUaXRsZRgGIAEoCRIpCglSZWFkU3RhdGUYByABKA4yFi5FQm9vbFN0" + "YXRlLkVCb29sU3RhdGUSKgoKQXdhcmRTdGF0ZRgIIAEoDjIWLkVCb29sU3Rh" + "dGUuRUJvb2xTdGF0ZRIlCglBd2FyZExpc3QYCSADKAsyEi50eXBlcy5Bd2Fy" + "ZERldGFpbBIQCghTZW5kVGltZRgKIAEoAxISCgpFeHBpcmVUaW1lGAsgASgD" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumEMailLevelReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MailDetail), (MessageParser)(object)MailDetail.Parser, new string[11]
			{
				"MailId", "MailLevel", "UUID", "SenderId", "SenderName", "Title", "ReadState", "AwardState", "AwardList", "SendTime",
				"ExpireTime"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
