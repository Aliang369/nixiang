using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeScratchCardDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeScratchCardDetailReflection()
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNjcmF0Y2hDYXJkRGV0YWlsLnByb3RvEgV0eXBlcxoadHlwZS5P" + "cGVyYXRlQmFzZUluZm8ucHJvdG8aI3R5cGUuT3BlcmF0ZVNjcmF0Y2hDYXJk" + "RGV0YWlsLnByb3RvItMBChFTY3JhdGNoQ2FyZERldGFpbBIoCghCYXNlSW5m" + "bxgBIAEoCzIWLnR5cGVzLk9wZXJhdGVCYXNlSW5mbxISCgpSZXNldENvdW50" + "GAIgASgFEhAKCFJlc2V0TWF4GAMgASgFEhQKDFJlc2V0RGlhbW9uZBgEIAEo" + "BRIOCgZQcm9wSWQYBSABKAUSEwoLU2NyYXRjaExpc3QYBiADKAUSMwoKVGFy" + "Z2V0TGlzdBgHIAMoCzIfLnR5cGVzLk9wZXJhdGVTY3JhdGNoQ2FyZERldGFp" + "bEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateScratchCardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ScratchCardDetail), (MessageParser)(object)ScratchCardDetail.Parser, new string[7] { "BaseInfo", "ResetCount", "ResetMax", "ResetDiamond", "PropId", "ScratchList", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
