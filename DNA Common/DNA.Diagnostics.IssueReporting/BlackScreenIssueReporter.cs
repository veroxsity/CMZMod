using System;

namespace DNA.Diagnostics.IssueReporting
{
	public class BlackScreenIssueReporter : IssueReporter
	{
		private string _errorURL;

		private string _name;

		private Version _version;

		private DateTime _gameStartTime;

		public BlackScreenIssueReporter(string errorURL, string name, Version version, DateTime gameStartTime)
		{
			_errorURL = errorURL;
			_name = name;
			_version = version;
			_gameStartTime = gameStartTime;
		}

		public override void ReportBug()
		{
		}

		public override void ReportCrash(Exception e)
		{
			using (ExceptionGame exceptionGame = new ExceptionGame(e, _errorURL, _name, _version, _gameStartTime))
			{
				exceptionGame.Run();
			}
		}

		public override void ReportStat(string stat, string value)
		{
		}
	}
}
