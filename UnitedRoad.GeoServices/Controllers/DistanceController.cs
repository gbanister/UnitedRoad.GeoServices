namespace UnitedRoad.GeoServices.Controllers
{
	using System;
	using System.Diagnostics;
	using System.Text.RegularExpressions;
	using System.Web.Http;
	using Common.Logging;
	using Infrustructure;


    public class DistanceController : ApiController
	{
		private readonly IAppSettings _appSettings;
		private readonly ISignUrl _signUrl;
		private readonly IMakeRequest _makeRequest;
		private readonly ILog _log;

		public DistanceController(IAppSettings appSettings, ISignUrl signUrl, IMakeRequest makeRequest, ILog log)
		{
			_appSettings = appSettings;
			_signUrl = signUrl;
			_makeRequest = makeRequest;
			_log = log;
		}

		public dynamic Get(string origin, string destination)
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();

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
				// convert anything less than 1 mile to 0
				var regexp = new Regex("\\d+ ft");
				distanceString = regexp.Replace(distanceString, "0");
				var distance = Convert.ToDecimal(distanceString.Replace(" mi", "").Replace(",", ""));
				var duration = result.rows[0].elements[0].duration.text;

				stopWatch.Stop();

				_log.Info(m => m(@"Distance\Get elapsed milliseconds: {0}", stopWatch.ElapsedMilliseconds));

				return new {distance, duration};
			}

			stopWatch.Stop();
			return 0;

		}
	}
}