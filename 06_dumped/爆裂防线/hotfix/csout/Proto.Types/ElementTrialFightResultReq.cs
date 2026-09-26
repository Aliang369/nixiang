using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ElementTrialFightResultReq : IMessage<ElementTrialFightResultReq>, IMessage, IEquatable<ElementTrialFightResultReq>, IDeepCloneable<ElementTrialFightResultReq>, IBufferMessage
{
	private static readonly MessageParser<ElementTrialFightResultReq> _parser = new MessageParser<ElementTrialFightResultReq>((Func<ElementTrialFightResultReq>)(() => new ElementTrialFightResultReq()));

	private UnknownFieldSet _unknownFields;

	public const int TrialTypeFieldNumber = 1;

	private ESkillAttackType trialType_;

	public const int LevelIdFieldNumber = 2;

	private int levelId_;

	public const int BattleResultFieldNumber = 3;

	private BattleResultInfo battleResult_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ElementTrialFightResultReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeElementTrialFightResultReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESkillAttackType TrialType
	{
		get
		{
			return trialType_;
		}
		set
		{
			trialType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LevelId
	{
		get
		{
			return levelId_;
		}
		set
		{
			levelId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultInfo BattleResult
	{
		get
		{
			return battleResult_;
		}
		set
		{
			battleResult_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ElementTrialFightResultReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ElementTrialFightResultReq(ElementTrialFightResultReq other)
		: this()
	{
		trialType_ = other.trialType_;
		levelId_ = other.levelId_;
		battleResult_ = ((other.battleResult_ != null) ? other.battleResult_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ElementTrialFightResultReq Clone()
	{
		return new ElementTrialFightResultReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ElementTrialFightResultReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ElementTrialFightResultReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TrialType != other.TrialType)
		{
			return false;
		}
		if (LevelId != other.LevelId)
		{
			return false;
		}
		if (!object.Equals(BattleResult, other.BattleResult))
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
		if (TrialType != ESkillAttackType.EmptyValue)
		{
			num ^= TrialType.GetHashCode();
		}
		if (LevelId != 0)
		{
			num ^= LevelId.GetHashCode();
		}
		if (battleResult_ != null)
		{
			num ^= BattleResult.GetHashCode();
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
		if (TrialType != ESkillAttackType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)TrialType);
		}
		if (LevelId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(LevelId);
		}
		if (battleResult_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleResult);
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
		if (TrialType != ESkillAttackType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)TrialType);
		}
		if (LevelId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LevelId);
		}
		if (battleResult_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleResult);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ElementTrialFightResultReq other)
	{
		if (other == null)
		{
			return;
		}
		if (other.TrialType != ESkillAttackType.EmptyValue)
		{
			TrialType = other.TrialType;
		}
		if (other.LevelId != 0)
		{
			LevelId = other.LevelId;
		}
		if (other.battleResult_ != null)
		{
			if (battleResult_ == null)
			{
				BattleResult = new BattleResultInfo();
			}
			BattleResult.MergeFrom(other.BattleResult);
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
			case 8u:
				TrialType = (ESkillAttackType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				LevelId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				if (battleResult_ == null)
				{
					BattleResult = new BattleResultInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleResult);
				break;
			}
		}
	}
}
