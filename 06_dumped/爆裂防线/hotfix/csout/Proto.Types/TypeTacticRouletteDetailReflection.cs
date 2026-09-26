using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTacticRouletteDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTacticRouletteDetailReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlRhY3RpY1JvdWxldHRlRGV0YWlsLnByb3RvEgV0eXBlcxoadHlw" + "ZS5PcGVyYXRlQmFzZUluZm8ucHJvdG8aJXR5cGUuT3BlcmF0ZVN1YmplY3RS" + "b3VsZXR0ZUluZm8ucHJvdG8aHnR5cGUuVWx0aW1hdGVBd2FyZERldGFpbC5w" + "cm90byLDAQoUVGFjdGljUm91bGV0dGVEZXRhaWwSKAoIQmFzZUluZm8YASAB" + "KAsyFi50eXBlcy5PcGVyYXRlQmFzZUluZm8SEQoJU3ViamVjdElkGAIgASgF" + "EjcKDFJvdWxldHRlSW5mbxgDIAEoCzIhLnR5cGVzLk9wZXJhdGVTdWJqZWN0" + "Um91bGV0dGVJbmZvEjUKEVVsdGltYXRlQXdhcmRMaXN0GAQgAygLMhoudHlw" + "ZXMuVWx0aW1hdGVBd2FyZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateSubjectRouletteInfoReflection.Descriptor,
			TypeUltimateAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TacticRouletteDetail), (MessageParser)(object)TacticRouletteDetail.Parser, new string[4] { "BaseInfo", "SubjectId", "RouletteInfo", "UltimateAwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
