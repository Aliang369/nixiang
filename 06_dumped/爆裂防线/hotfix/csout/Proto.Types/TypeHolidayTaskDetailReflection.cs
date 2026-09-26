using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHolidayTaskDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHolidayTaskDetailReflection()
	{
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkhvbGlkYXlUYXNrRGV0YWlsLnByb3RvEgV0eXBlcxokdHlwZS5I" + "b2xpZGF5VGFza0V4Y2hhbmdlRGV0YWlsLnByb3RvGhp0eXBlLk9wZXJhdGVC" + "YXNlSW5mby5wcm90bxoVdHlwZS5UYXNrRGV0YWlsLnByb3RvIt0CChFIb2xp" + "ZGF5VGFza0RldGFpbBIoCghCYXNlSW5mbxgBIAEoCzIWLnR5cGVzLk9wZXJh" + "dGVCYXNlSW5mbxIPCgdHb29kc0lkGAIgASgFEg0KBUxldmVsGAMgASgFEgsK" + "A0V4cBgEIAEoBRIVCg1CdXlMZXZlbENvdW50GAUgASgFEhMKC0FjdGl2ZVN0" + "YXRlGAYgASgFEhUKDUxvd0F3YXJkTGV2ZWwYByABKAUSFgoOSGlnaEF3YXJk" + "TGV2ZWwYCCABKAUSDgoGSGVyb0lkGAkgASgFEiMKCFRhc2tMaXN0GAogAygL" + "MhEudHlwZXMuVGFza0RldGFpbBISCgpUb3RhbFNjb3JlGAsgASgFEhUKDUV4" + "Y2hhbmdlU2NvcmUYDCABKAUSNgoMRXhjaGFuZ2VMaXN0GA0gAygLMiAudHlw" + "ZXMuSG9saWRheVRhc2tFeGNoYW5nZURldGFpbEIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeHolidayTaskExchangeDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor,
			TypeTaskDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HolidayTaskDetail), (MessageParser)(object)HolidayTaskDetail.Parser, new string[13]
			{
				"BaseInfo", "GoodsId", "Level", "Exp", "BuyLevelCount", "ActiveState", "LowAwardLevel", "HighAwardLevel", "HeroId", "TaskList",
				"TotalScore", "ExchangeScore", "ExchangeList"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
