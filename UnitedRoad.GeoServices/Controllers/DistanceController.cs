namespace UnitedRoad.GeoServices.Controllers
{
	using System;
	using System.IO;
	using System.Net;
	using System.Web.Http;
	using Newtonsoft.Json;

	public class DistanceController : ApiController
	{
		public int Get(string origin, string destination)
		{
			var url = @"http://maps.googleapis.com/maps/api/distancematrix/json?origins=" +
			          origin + "&destinations=" + destination +
			          "&mode=driving&sensor=false&language=en-EN&units=imperial";

			var request = (HttpWebRequest) WebRequest.Create(url);
			var response = request.GetResponse();
			var dataStream = response.GetResponseStream();
			var sreader = new StreamReader(dataStream);
			var responsereader = sreader.ReadToEnd();
			response.Close();

			dynamic result = JsonConvert.DeserializeObject(responsereader);

			if (result.status == "OK")
			{
				string distanceString = result.rows[0].elements[0].distance.text;
				var distance = Convert.ToInt32(distanceString.Replace(" mi", "").Replace(",", ""));
				return distance;
			}

			return 0;

		}

	}
}