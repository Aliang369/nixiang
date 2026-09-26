using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeDailyDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeDailyDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlJlY2hhcmdlRGFpbHlEZXRhaWwucHJvdG8SBXR5cGVzGhp0eXBl" + "Lk9wZXJhdGVCYXNlSW5mby5wcm90bxofdHlwZS5SZWNoYXJnZVRhcmdldERl" + "dGFpbC5wcm90byJ/ChNSZWNoYXJnZURhaWx5RGV0YWlsEigKCEJhc2VJbmZv" + "GAEgASgLMhYudHlwZXMuT3BlcmF0ZUJhc2VJbmZvEi8KClRhcmdldExpc3QY" + "AiADKAsyGy50eXBlcy5SZWNoYXJnZVRhcmdldERldGFpbBINCgVDb3VudBgD" + "IAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeRechargeTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeDailyDetail), (MessageParser)(object)RechargeDailyDetail.Parser, new string[3] { "BaseInfo", "TargetList", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
