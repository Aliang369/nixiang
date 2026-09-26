using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsZhanLingAdAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsZhanLingAdAwardReqReflection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk9wZXJhdGVBZHNaaGFuTGluZ0FkQXdhcmRSZXEucHJvdG8SBXR5" + "cGVzIkoKHE9wZXJhdGVBZHNaaGFuTGluZ0FkQXdhcmRSZXESDAoEVVVJRBgB" + "IAEoCRINCgVBZHZJZBgCIAEoBRINCgVMZXZlbBgDIAEoBUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsZhanLingAdAwardReq), (MessageParser)(object)OperateAdsZhanLingAdAwardReq.Parser, new string[3] { "UUID", "AdvId", "Level" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
