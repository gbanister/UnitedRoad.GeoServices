namespace UnitedRoad.GeoServices.Controllers
{
	using System;
	using System.IO;
	using System.Net;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web.Http;
	using Newtonsoft.Json;

	public class DistanceController : ApiController
	{
		public int Get(string origin, string destination)
		{
			var url = @"http://maps.googleapis.com/maps/api/distancematrix/json?origins=" +
			          origin + "&destinations=" + destination +
			          "&mode=driving&sensor=false&language=en-EN&units=imperial";

			url += "&client=";

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

		public static string Sign(string url, string keyString)
		{
			var encoding = new ASCIIEncoding();

			// converting key to bytes will throw an exception, need to replace '-' and '_' characters first.
			var usablePrivateKey = keyString.Replace("-", "+").Replace("_", "/");
			var privateKeyBytes = Convert.FromBase64String(usablePrivateKey);

			var uri = new Uri(url);
			var encodedPathAndQueryBytes = encoding.GetBytes(uri.LocalPath + uri.Query);

			// compute the hash
			var algorithm = new HMACSHA1(privateKeyBytes);
			var hash = algorithm.ComputeHash(encodedPathAndQueryBytes);

			// convert the bytes to string and make url-safe by replacing '+' and '/' characters
			var signature = Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_");

			// Add the signature to the existing URI.
			return uri.Scheme + "://" + uri.Host + uri.LocalPath + uri.Query + "&signature=" + signature;
		}
	}
}