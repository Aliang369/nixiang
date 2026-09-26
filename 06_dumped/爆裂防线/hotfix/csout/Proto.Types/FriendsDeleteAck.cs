using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FriendsDeleteAck : IMessage<FriendsDeleteAck>, IMessage, IEquatable<FriendsDeleteAck>, IDeepCloneable<FriendsDeleteAck>, IBufferMessage
{
	private static readonly MessageParser<FriendsDeleteAck> _parser = new MessageParser<FriendsDeleteAck>((Func<FriendsDeleteAck>)(() => new FriendsDeleteAck()));

	private UnknownFieldSet _unknownFields;

	public const int PlayerIdFieldNumber = 1;

	private int playerId_;

	public const int FriendsInfoFieldNumber = 2;

	private FriendsInfo friendsInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FriendsDeleteAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFriendsDeleteAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PlayerId
	{
		get
		{
			return playerId_;
		}
		set
		{
			playerId_ = value;
		}
	}

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
	public FriendsDeleteAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsDeleteAck(FriendsDeleteAck other)
		: this()
	{
		playerId_ = other.playerId_;
		friendsInfo_ = ((other.friendsInfo_ != null) ? other.friendsInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsDeleteAck Clone()
	{
		return new FriendsDeleteAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FriendsDeleteAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FriendsDeleteAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PlayerId != other.PlayerId)
		{
			return false;
		}
		if (!object.Equals(FriendsInfo, other.FriendsInfo))
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
		if (PlayerId != 0)
		{
			num ^= PlayerId.GetHashCode();
		}
		if (friendsInfo_ != null)
		{
			num ^= FriendsInfo.GetHashCode();
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
		if (PlayerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(PlayerId);
		}
		if (friendsInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FriendsInfo);
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
		if (PlayerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PlayerId);
		}
		if (friendsInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FriendsInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FriendsDeleteAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.PlayerId != 0)
		{
			PlayerId = other.PlayerId;
		}
		if (other.friendsInfo_ != null)
		{
			if (friendsInfo_ == null)
			{
				FriendsInfo = new FriendsInfo();
			}
			FriendsInfo.MergeFrom(other.FriendsInfo);
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
				PlayerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				if (friendsInfo_ == null)
				{
					FriendsInfo = new FriendsInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FriendsInfo);
				break;
			}
		}
	}
}
