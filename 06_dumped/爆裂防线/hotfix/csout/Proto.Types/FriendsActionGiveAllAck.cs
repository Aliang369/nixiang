using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FriendsActionGiveAllAck : IMessage<FriendsActionGiveAllAck>, IMessage, IEquatable<FriendsActionGiveAllAck>, IDeepCloneable<FriendsActionGiveAllAck>, IBufferMessage
{
	private static readonly MessageParser<FriendsActionGiveAllAck> _parser = new MessageParser<FriendsActionGiveAllAck>((Func<FriendsActionGiveAllAck>)(() => new FriendsActionGiveAllAck()));

	private UnknownFieldSet _unknownFields;

	public const int FriendsInfoFieldNumber = 1;

	private FriendsInfo friendsInfo_;

	public const int FriendsListFieldNumber = 2;

	private static readonly FieldCodec<FriendsDetail> _repeated_friendsList_codec = FieldCodec.ForMessage<FriendsDetail>(18u, FriendsDetail.Parser);

	private readonly RepeatedField<FriendsDetail> friendsList_ = new RepeatedField<FriendsDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FriendsActionGiveAllAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFriendsActionGiveAllAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsInfo FriendsInfo
	{
		get
		{
			return friendsInfo_;
		}
		set
		{
			friendsInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<FriendsDetail> FriendsList => friendsList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsActionGiveAllAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsActionGiveAllAck(FriendsActionGiveAllAck other)
		: this()
	{
		friendsInfo_ = ((other.friendsInfo_ != null) ? other.friendsInfo_.Clone() : null);
		friendsList_ = other.friendsList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsActionGiveAllAck Clone()
	{
		return new FriendsActionGiveAllAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FriendsActionGiveAllAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FriendsActionGiveAllAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(FriendsInfo, other.FriendsInfo))
		{
			return false;
		}
		if (!friendsList_.Equals(other.friendsList_))
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
		if (friendsInfo_ != null)
		{
			num ^= FriendsInfo.GetHashCode();
		}
		num ^= ((object)friendsList_).GetHashCode();
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
		if (friendsInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FriendsInfo);
		}
		friendsList_.WriteTo(ref output, _repeated_friendsList_codec);
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
		if (friendsInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FriendsInfo);
		}
		num += friendsList_.CalculateSize(_repeated_friendsList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FriendsActionGiveAllAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.friendsInfo_ != null)
		{
			if (friendsInfo_ == null)
			{
				FriendsInfo = new FriendsInfo();
			}
			FriendsInfo.MergeFrom(other.FriendsInfo);
		}
		friendsList_.Add((IEnumerable<FriendsDetail>)other.friendsList_);
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
				if (friendsInfo_ == null)
				{
					FriendsInfo = new FriendsInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FriendsInfo);
				break;
			case 18u:
				friendsList_.AddEntriesFrom(ref input, _repeated_friendsList_codec);
				break;
			}
		}
	}
}
