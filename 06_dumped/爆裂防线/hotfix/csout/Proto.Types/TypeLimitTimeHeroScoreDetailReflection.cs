using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLimitTimeHeroScoreDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitTimeHeroScoreDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkxpbWl0VGltZUhlcm9TY29yZURldGFpbC5wcm90bxIFdHlwZXMa" + "FnR5cGUuQXdhcmREZXRhaWwucHJvdG8igAEKGExpbWl0VGltZUhlcm9TY29y" + "ZURldGFpbBINCgVJbmRleBgBIAEoBRINCgVTY29yZRgCIAEoBRIQCghMaW1p" + "dE51bRgDIAEoBRINCgVDb3VudBgEIAEoBRIlCglBd2FyZExpc3QYBSADKAsy" + "Ei50eXBlcy5Bd2FyZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitTimeHeroScoreDetail), (MessageParser)(object)LimitTimeHeroScoreDetail.Parser, new string[5] { "Index", "Score", "LimitNum", "Count", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
