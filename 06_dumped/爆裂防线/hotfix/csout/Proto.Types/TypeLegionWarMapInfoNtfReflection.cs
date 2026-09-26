using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarMapInfoNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarMapInfoNtfReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkxlZ2lvbldhck1hcEluZm9OdGYucHJvdG8SBXR5cGVzGht0eXBl" + "LkxlZ2lvbldhck1hcEluZm8ucHJvdG8iSAoTTGVnaW9uV2FyTWFwSW5mb050" + "ZhIxChBMZWdpb25XYXJNYXBJbmZvGAEgASgLMhcudHlwZXMuTGVnaW9uV2Fy" + "TWFwSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeLegionWarMapInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarMapInfoNtf), (MessageParser)(object)LegionWarMapInfoNtf.Parser, new string[1] { "LegionWarMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
