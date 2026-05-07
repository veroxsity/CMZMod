using System;

namespace DNA.Distribution
{
	public abstract class OnlineServices
	{
		private static OnlineServices _instance;

		private Guid _productID;

		protected string _username;

		public static OnlineServices Instance
		{
			get
			{
				return _instance;
			}
		}

		public Guid ProductID
		{
			get
			{
				return _productID;
			}
		}

		public string Username
		{
			get
			{
				return _username;
			}
		}

		public OnlineServices(Guid productID)
		{
			if (_instance != null)
			{
				throw new Exception("Instance of online service already running");
			}
			_instance = this;
			_productID = productID;
		}

		public bool IsOnline()
		{
			try
			{
				GetServerTime();
			}
			catch
			{
				return false;
			}
			return true;
		}

		public abstract DateTime GetServerTime();

		public abstract bool ValidateLicense(string userName, string password, out string reason);

		public abstract bool ValidateLicenseFacebook(string facebookID, string accessToken, out string username, out string reason);

		public abstract bool RegisterFacebook(string facebookID, string accessToken, string email, string userName, string password, out string reason);

		public abstract void AcceptTerms(string userName, string password);

		public abstract void AcceptTermsFacebook(string facebookID);

		public abstract string GetLauncherPage();

		public abstract string GetProductTitle();

		public abstract int? GetAddOn(Guid guid);
	}
}
