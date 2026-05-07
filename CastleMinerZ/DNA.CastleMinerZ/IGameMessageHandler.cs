namespace DNA.CastleMinerZ
{
	public interface IGameMessageHandler
	{
		void HandleMessage(GameMessageType type, object data, object sender);
	}
}
