using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilEventHighScoreAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilEventHighScoreAckReflection()
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cit0eXBlLk9wZXJhdGVTbm93VmVpbEV2ZW50SGlnaFNjb3JlQWNrLnByb3Rv" + "EgV0eXBlcxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGiZ0eXBlLk9wZXJhdGVT" + "bm93VmVpbFRhcmdldERldGFpbC5wcm90bxondHlwZS5PcGVyYXRlVGFyZ2V0" + "TmV4dEdyYW5kRGV0YWlsLnByb3RvGh10eXBlLlNub3dWZWlsTGFuZERldGFp" + "bC5wcm90bxocdHlwZS5Tbm93VmVpbFJvbGVTdGF0ZS5wcm90byK9AgogT3Bl" + "cmF0ZVNub3dWZWlsRXZlbnRIaWdoU2NvcmVBY2sSDAoEVVVJRBgBIAEoCRIS" + "CgpUb3RhbFNjb3JlGAIgASgFEjgKDFRhcmdldERldGFpbBgDIAEoCzIiLnR5" + "cGVzLk9wZXJhdGVTbm93VmVpbFRhcmdldERldGFpbBI8Cg9UYXJnZXROZXh0" + "R3JhbmQYBCABKAsyIy50eXBlcy5PcGVyYXRlVGFyZ2V0TmV4dEdyYW5kRGV0" + "YWlsEisKCExhbmRMaXN0GAUgAygLMhkudHlwZXMuU25vd1ZlaWxMYW5kRGV0" + "YWlsEisKCVJvbGVTdGF0ZRgGIAEoCzIYLnR5cGVzLlNub3dWZWlsUm9sZVN0" + "YXRlEiUKCkRhdGFDaGFuZ2UYByABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeDataChangeReflection.Descriptor,
			TypeOperateSnowVeilTargetDetailReflection.Descriptor,
			TypeOperateTargetNextGrandDetailReflection.Descriptor,
			TypeSnowVeilLandDetailReflection.Descriptor,
			TypeSnowVeilRoleStateReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilEventHighScoreAck), (MessageParser)(object)OperateSnowVeilEventHighScoreAck.Parser, new string[7] { "UUID", "TotalScore", "TargetDetail", "TargetNextGrand", "LandList", "RoleState", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
