using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ChatActionParams : IMessage<ChatActionParams>, IMessage, IEquatable<ChatActionParams>, IDeepCloneable<ChatActionParams>, IBufferMessage
{
	private static readonly MessageParser<ChatActionParams> _parser = new MessageParser<ChatActionParams>((Func<ChatActionParams>)(() => new ChatActionParams()));

	private UnknownFieldSet _unknownFields;

	public const int BarShareFieldNumber = 1;

	private CommonAwardInfo barShare_;

	public const int HeroShareFieldNumber = 2;

	private int heroShare_;

	public const int EquipShareFieldNumber = 3;

	private EEquipType equipShare_;

	public const int BattleResultFieldNumber = 4;

	private string battleResult_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ChatActionParams> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeChatActionParamsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CommonAwardInfo BarShare
	{
		get
		{
			return barShare_;
		}
		set
		{
			barShare_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HeroShare
	{
		get
		{
			return heroShare_;
		}
		set
		{
			heroShare_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EEquipType EquipShare
	{
		get
		{
			return equipShare_;
		}
		set
		{
			equipShare_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string BattleResult
	{
		get
		{
			return battleResult_;
		}
		set
		{
			battleResult_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionParams()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionParams(ChatActionParams other)
		: this()
	{
		barShare_ = ((other.barShare_ != null) ? other.barShare_.Clone() : null);
		heroShare_ = other.heroShare_;
		equipShare_ = other.equipShare_;
		battleResult_ = other.battleResult_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionParams Clone()
	{
		return new ChatActionParams(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ChatActionParams);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ChatActionParams other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BarShare, other.BarShare))
		{
			return false;
		}
		if (HeroShare != other.HeroShare)
		{
			return false;
		}
		if (EquipShare != other.EquipShare)
		{
			return false;
		}
		if (BattleResult != other.BattleResult)
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
		if (barShare_ != null)
		{
			num ^= BarShare.GetHashCode();
		}
		if (HeroShare != 0)
		{
			num ^= HeroShare.GetHashCode();
		}
		if (EquipShare != EEquipType.EmptyValue)
		{
			num ^= EquipShare.GetHashCode();
		}
		if (BattleResult.Length != 0)
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
		if (barShare_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BarShare);
		}
		if (HeroShare != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(HeroShare);
		}
		if (EquipShare != EEquipType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)EquipShare);
		}
		if (BattleResult.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteString(BattleResult);
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
		if (barShare_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BarShare);
		}
		if (HeroShare != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HeroShare);
		}
		if (EquipShare != EEquipType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)EquipShare);
		}
		if (BattleResult.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(BattleResult);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ChatActionParams other)
	{
		if (other == null)
		{
			return;
		}
		if (other.barShare_ != null)
		{
			if (barShare_ == null)
			{
				BarShare = new CommonAwardInfo();
			}
			BarShare.MergeFrom(other.BarShare);
		}
		if (other.HeroShare != 0)
		{
			HeroShare = other.HeroShare;
		}
		if (other.EquipShare != EEquipType.EmptyValue)
		{
			EquipShare = other.EquipShare;
		}
		if (other.BattleResult.Length != 0)
		{
			BattleResult = other.BattleResult;
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
				if (barShare_ == null)
				{
					BarShare = new CommonAwardInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BarShare);
				break;
			case 16u:
				HeroShare = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				EquipShare = (EEquipType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 34u:
				BattleResult = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
