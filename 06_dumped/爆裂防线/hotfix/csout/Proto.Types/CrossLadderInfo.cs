using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossLadderInfo : IMessage<CrossLadderInfo>, IMessage, IEquatable<CrossLadderInfo>, IDeepCloneable<CrossLadderInfo>, IBufferMessage
{
	private static readonly MessageParser<CrossLadderInfo> _parser = new MessageParser<CrossLadderInfo>((Func<CrossLadderInfo>)(() => new CrossLadderInfo()));

	private UnknownFieldSet _unknownFields;

	public const int OpenInfoFieldNumber = 1;

	private CrossLadderOpenInfo openInfo_;

	public const int SelfInfoFieldNumber = 2;

	private CrossLadderSelfInfo selfInfo_;

	public const int AchieveInfoFieldNumber = 3;

	private CrossLadderAchieveInfo achieveInfo_;

	public const int CombatInfoFieldNumber = 4;

	private CrossLadderCombatInfo combatInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossLadderInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossLadderInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderOpenInfo OpenInfo
	{
		get
		{
			return openInfo_;
		}
		set
		{
			openInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderSelfInfo SelfInfo
	{
		get
		{
			return selfInfo_;
		}
		set
		{
			selfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderAchieveInfo AchieveInfo
	{
		get
		{
			return achieveInfo_;
		}
		set
		{
			achieveInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderCombatInfo CombatInfo
	{
		get
		{
			return combatInfo_;
		}
		set
		{
			combatInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderInfo(CrossLadderInfo other)
		: this()
	{
		openInfo_ = ((other.openInfo_ != null) ? other.openInfo_.Clone() : null);
		selfInfo_ = ((other.selfInfo_ != null) ? other.selfInfo_.Clone() : null);
		achieveInfo_ = ((other.achieveInfo_ != null) ? other.achieveInfo_.Clone() : null);
		combatInfo_ = ((other.combatInfo_ != null) ? other.combatInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderInfo Clone()
	{
		return new CrossLadderInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossLadderInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossLadderInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(OpenInfo, other.OpenInfo))
		{
			return false;
		}
		if (!object.Equals(SelfInfo, other.SelfInfo))
		{
			return false;
		}
		if (!object.Equals(AchieveInfo, other.AchieveInfo))
		{
			return false;
		}
		if (!object.Equals(CombatInfo, other.CombatInfo))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (openInfo_ != null)
		{
			num ^= OpenInfo.GetHashCode();
		}
		if (selfInfo_ != null)
		{
			num ^= SelfInfo.GetHashCode();
		}
		if (achieveInfo_ != null)
		{
			num ^= AchieveInfo.GetHashCode();
		}
		if (combatInfo_ != null)
		{
			num ^= CombatInfo.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= ((object)_unknownFields).GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (openInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)OpenInfo);
		}
		if (selfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SelfInfo);
		}
		if (achieveInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)AchieveInfo);
		}
		if (combatInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)CombatInfo);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (openInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)OpenInfo);
		}
		if (selfInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SelfInfo);
		}
		if (achieveInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)AchieveInfo);
		}
		if (combatInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)CombatInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CrossLadderInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.openInfo_ != null)
		{
			if (openInfo_ == null)
			{
				OpenInfo = new CrossLadderOpenInfo();
			}
			OpenInfo.MergeFrom(other.OpenInfo);
		}
		if (other.selfInfo_ != null)
		{
			if (selfInfo_ == null)
			{
				SelfInfo = new CrossLadderSelfInfo();
			}
			SelfInfo.MergeFrom(other.SelfInfo);
		}
		if (other.achieveInfo_ != null)
		{
			if (achieveInfo_ == null)
			{
				AchieveInfo = new CrossLadderAchieveInfo();
			}
			AchieveInfo.MergeFrom(other.AchieveInfo);
		}
		if (other.combatInfo_ != null)
		{
			if (combatInfo_ == null)
			{
				CombatInfo = new CrossLadderCombatInfo();
			}
			CombatInfo.MergeFrom(other.CombatInfo);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = ((ParseContext)(ref input)).ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 10u:
				if (openInfo_ == null)
				{
					OpenInfo = new CrossLadderOpenInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)OpenInfo);
				break;
			case 18u:
				if (selfInfo_ == null)
				{
					SelfInfo = new CrossLadderSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SelfInfo);
				break;
			case 26u:
				if (achieveInfo_ == null)
				{
					AchieveInfo = new CrossLadderAchieveInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)AchieveInfo);
				break;
			case 34u:
				if (combatInfo_ == null)
				{
					CombatInfo = new CrossLadderCombatInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)CombatInfo);
				break;
			}
		}
	}
}
