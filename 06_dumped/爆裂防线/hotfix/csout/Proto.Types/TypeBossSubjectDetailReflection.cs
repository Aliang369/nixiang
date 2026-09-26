using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBossSubjectDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBossSubjectDetailReflection()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkJvc3NTdWJqZWN0RGV0YWlsLnByb3RvEgV0eXBlcxoadHlwZS5C" + "b3NzU3ViamVjdEluZm8ucHJvdG8aInR5cGUuQm9zc1N1YmplY3RSb3VsZXR0" + "ZUluZm8ucHJvdG8aGnR5cGUuT3BlcmF0ZUJhc2VJbmZvLnByb3RvGhV0eXBl" + "LlRhc2tEZXRhaWwucHJvdG8aHXR5cGUuWmhhbkxpbmdDb21tb25JbmZvLnBy" + "b3RvIokCChFCb3NzU3ViamVjdERldGFpbBIoCghCYXNlSW5mbxgBIAEoCzIW" + "LnR5cGVzLk9wZXJhdGVCYXNlSW5mbxIrCgtCb3NzU3ViamVjdBgCIAEoCzIW" + "LnR5cGVzLkJvc3NTdWJqZWN0SW5mbxIvCgxaaGFuTGluZ0luZm8YAyABKAsy" + "GS50eXBlcy5aaGFuTGluZ0NvbW1vbkluZm8SNAoMUm91bGV0dGVJbmZvGAQg" + "ASgLMh4udHlwZXMuQm9zc1N1YmplY3RSb3VsZXR0ZUluZm8SIwoIVGFza0xp" + "c3QYBSADKAsyES50eXBlcy5UYXNrRGV0YWlsEhEKCVRhc2tDb3VudBgGIAEo" + "BUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeBossSubjectInfoReflection.Descriptor,
			TypeBossSubjectRouletteInfoReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor,
			TypeTaskDetailReflection.Descriptor,
			TypeZhanLingCommonInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BossSubjectDetail), (MessageParser)(object)BossSubjectDetail.Parser, new string[6] { "BaseInfo", "BossSubject", "ZhanLingInfo", "RouletteInfo", "TaskList", "TaskCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
