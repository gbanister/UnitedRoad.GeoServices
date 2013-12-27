namespace UnitedRoad.GeoServices.Infrustructure
{
	using System.Configuration;

	public interface IAppSettings
	{
		string GoogleApiClientId();
		string GoogleApiClientKey();
	}

	public class AppSettings : IAppSettings
	{
		public string GoogleApiClientId()
		{
			return ConfigurationManager.AppSettings["GoogleApiClientId"];
		}

		public string GoogleApiClientKey()
		{
			return ConfigurationManager.AppSettings["GoogleApiClientKey"];
		}
	}
}