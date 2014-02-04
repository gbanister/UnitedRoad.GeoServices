namespace UnitedRoad.GeoServices.Controllers
{
	using System;
	using System.Web;
	using System.Web.Http;
	using UnitedRoad.GeoServices.Infrustructure;

	public class DistanceController : ApiController
	{
		private readonly IAppSettings _appSettings;
		private readonly ISignUrl _signUrl;
		private readonly IMakeRequest _makeRequest;

		public DistanceController(IAppSettings appSettings, ISignUrl signUrl, IMakeRequest makeRequest)
		{
			_appSettings = appSettings;
			_signUrl = signUrl;
			_makeRequest = makeRequest;
		}

		public dynamic Get(string origin, string destination)
		{
			var urlParams = @"/json?origins=" +
			          origin + "&destinations=" + destination +
			          "&mode=driving&sensor=false&language=en-EN&units=imperial";

			urlParams += "&client=" + _appSettings.GoogleApiClientId();

			var fullUrl = Constants.DISTANCE_MATRIX_BASE_URL + urlParams;

			var signedUrl = _signUrl.Sign(fullUrl, _appSettings.GoogleApiClientKey());

			var result = _makeRequest.GetResponse(signedUrl);

			if (result.status == "OK" && result.rows[0].elements[0].status.Value == "OK")
			{
				string distanceString = result.rows[0].elements[0].distance.text;
				var distance = Convert.ToInt32(distanceString.Replace(" mi", "").Replace(",", ""));
				var duration = result.rows[0].elements[0].duration.text;
				return new {distance, duration};
			}

			return 0;

		}
	}
}