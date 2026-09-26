using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCipherBoxDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCipherBoxDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkNpcGhlckJveERldGFpbC5wcm90bxIFdHlwZXMaIHR5cGUuQ2lw" + "aGVyQm94VGFyZ2V0RGV0YWlsLnByb3RvGhp0eXBlLk9wZXJhdGVCYXNlSW5m" + "by5wcm90byKOAQoPQ2lwaGVyQm94RGV0YWlsEigKCEJhc2VJbmZvGAEgASgL" + "MhYudHlwZXMuT3BlcmF0ZUJhc2VJbmZvEhAKCExpbWl0TnVtGAIgASgFEg0K" + "BUNvdW50GAMgASgFEjAKClRhcmdldExpc3QYBCADKAsyHC50eXBlcy5DaXBo" + "ZXJCb3hUYXJnZXREZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeCipherBoxTargetDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CipherBoxDetail), (MessageParser)(object)CipherBoxDetail.Parser, new string[4] { "BaseInfo", "LimitNum", "Count", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
