using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePlayerSuitInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePlayerSuitInfoReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlBsYXllclN1aXRJbmZvLnByb3RvEgV0eXBlcxobdHlwZS5QbGF5" + "ZXJTdWl0RGV0YWlsLnByb3RvIncKDlBsYXllclN1aXRJbmZvEikKCFN1aXRM" + "aXN0GAEgAygLMhcudHlwZXMuUGxheWVyU3VpdERldGFpbBITCgtUb3RhbEF0" + "dGFjaxgCIAEoBRIPCgdVc2VMaXN0GAMgAygFEhQKDFN1cGVyVXNlTGlzdBgE" + "IAMoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypePlayerSuitDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PlayerSuitInfo), (MessageParser)(object)PlayerSuitInfo.Parser, new string[4] { "SuitList", "TotalAttack", "UseList", "SuperUseList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
