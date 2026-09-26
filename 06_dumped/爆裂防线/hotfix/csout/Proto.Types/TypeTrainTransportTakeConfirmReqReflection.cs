using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTrainTransportTakeConfirmReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTrainTransportTakeConfirmReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLlRyYWluVHJhbnNwb3J0VGFrZUNvbmZpcm1SZXEucHJvdG8SBXR5" + "cGVzIkoKHFRyYWluVHJhbnNwb3J0VGFrZUNvbmZpcm1SZXESEwoLVHJhbnNw" + "b3J0SWQYASABKAUSFQoNQ2FycmlhZ2VJbmRleBgCIAEoBUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TrainTransportTakeConfirmReq), (MessageParser)(object)TrainTransportTakeConfirmReq.Parser, new string[2] { "TransportId", "CarriageIndex" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
