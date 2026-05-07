using System.IO;
using DNA.Net;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Net
{
	public class RemoveBlocksMessage : CastleMinerZMessage
	{
		public bool DoDigEffects;

		public int NumBlocks;

		public IntVector3[] BlocksToRemove;

		public override MessageTypes MessageType
		{
			get
			{
				return MessageTypes.System;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.ReliableInOrder;
			}
		}

		private RemoveBlocksMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, int numblocks, IntVector3[] blocks, bool doEffects)
		{
			RemoveBlocksMessage sendInstance = Message.GetSendInstance<RemoveBlocksMessage>();
			sendInstance.NumBlocks = numblocks;
			sendInstance.BlocksToRemove = blocks;
			sendInstance.DoDigEffects = doEffects;
			sendInstance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(NumBlocks);
			for (int i = 0; i < NumBlocks; i++)
			{
				BlocksToRemove[i].Write(writer);
			}
			writer.Write(DoDigEffects);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			NumBlocks = reader.ReadInt32();
			BlocksToRemove = new IntVector3[NumBlocks];
			for (int i = 0; i < NumBlocks; i++)
			{
				BlocksToRemove[i] = IntVector3.Read(reader);
			}
			DoDigEffects = reader.ReadBoolean();
		}
	}
}
