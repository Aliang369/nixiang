using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLimitTimeHeroDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitTimeHeroDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkxpbWl0VGltZUhlcm9EZXRhaWwucHJvdG8SBXR5cGVzGiB0eXBl" + "LkxpbWl0VGltZUhlcm9CYXNlRGF0YS5wcm90bxojdHlwZS5MaW1pdFRpbWVI" + "ZXJvU2NvcmVEZXRhaWwucHJvdG8aGnR5cGUuT3BlcmF0ZUJhc2VJbmZvLnBy" + "b3RvIqMBChNMaW1pdFRpbWVIZXJvRGV0YWlsEigKCEJhc2VJbmZvGAEgASgL" + "MhYudHlwZXMuT3BlcmF0ZUJhc2VJbmZvEi4KCEJhc2VEYXRhGAIgASgLMhwu" + "dHlwZXMuTGltaXRUaW1lSGVyb0Jhc2VEYXRhEjIKCVNjb3JlTGlzdBgDIAMo" + "CzIfLnR5cGVzLkxpbWl0VGltZUhlcm9TY29yZURldGFpbEIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeLimitTimeHeroBaseDataReflection.Descriptor,
			TypeLimitTimeHeroScoreDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitTimeHeroDetail), (MessageParser)(object)LimitTimeHeroDetail.Parser, new string[3] { "BaseInfo", "BaseData", "ScoreList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
