using System.IO;
using DNA.Net;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Net
{
	public class EnemyGiveUpMessage : CastleMinerZMessage
	{
		public int EnemyID;

		public int TargetID;

		public override MessageTypes MessageType
		{
			get
			{
				return MessageTypes.EnemyMessage;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.Reliable;
			}
		}

		private EnemyGiveUpMessage()
		{
		}

		public static void Send(int enemyid, int targetid)
		{
			EnemyGiveUpMessage sendInstance = Message.GetSendInstance<EnemyGiveUpMessage>();
			sendInstance.EnemyID = enemyid;
			sendInstance.TargetID = targetid;
			sendInstance.DoSend(CastleMinerZGame.Instance.LocalPlayer.Gamer as LocalNetworkGamer);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			EnemyID = reader.ReadInt32();
			TargetID = reader.ReadInt32();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(EnemyID);
			writer.Write(TargetID);
		}
	}
}
