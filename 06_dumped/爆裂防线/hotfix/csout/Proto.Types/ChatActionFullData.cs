using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ChatActionFullData : IMessage<ChatActionFullData>, IMessage, IEquatable<ChatActionFullData>, IDeepCloneable<ChatActionFullData>, IBufferMessage
{
	private static readonly MessageParser<ChatActionFullData> _parser = new MessageParser<ChatActionFullData>((Func<ChatActionFullData>)(() => new ChatActionFullData()));

	private UnknownFieldSet _unknownFields;

	public const int LegionInviteFieldNumber = 1;

	private LegionTipsInfo legionInvite_;

	public const int EquipShareFieldNumber = 2;

	private EquipSlotDetail equipShare_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ChatActionFullData> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeChatActionFullDataReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionTipsInfo LegionInvite
	{
		get
		{
			return legionInvite_;
		}
		set
		{
			legionInvite_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotDetail EquipShare
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
	public ChatActionFullData()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionFullData(ChatActionFullData other)
		: this()
	{
		legionInvite_ = ((other.legionInvite_ != null) ? other.legionInvite_.Clone() : null);
		equipShare_ = ((other.equipShare_ != null) ? other.equipShare_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionFullData Clone()
	{
		return new ChatActionFullData(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ChatActionFullData);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ChatActionFullData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(LegionInvite, other.LegionInvite))
		{
			return false;
		}
		if (!object.Equals(EquipShare, other.EquipShare))
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
		if (legionInvite_ != null)
		{
			num ^= LegionInvite.GetHashCode();
		}
		if (equipShare_ != null)
		{
			num ^= EquipShare.GetHashCode();
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
		if (legionInvite_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionInvite);
		}
		if (equipShare_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EquipShare);
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
		if (legionInvite_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionInvite);
		}
		if (equipShare_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EquipShare);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ChatActionFullData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.legionInvite_ != null)
		{
			if (legionInvite_ == null)
			{
				LegionInvite = new LegionTipsInfo();
			}
			LegionInvite.MergeFrom(other.LegionInvite);
		}
		if (other.equipShare_ != null)
		{
			if (equipShare_ == null)
			{
				EquipShare = new EquipSlotDetail();
			}
			EquipShare.MergeFrom(other.EquipShare);
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
				if (legionInvite_ == null)
				{
					LegionInvite = new LegionTipsInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionInvite);
				break;
			case 18u:
				if (equipShare_ == null)
				{
					EquipShare = new EquipSlotDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EquipShare);
				break;
			}
		}
	}
}
