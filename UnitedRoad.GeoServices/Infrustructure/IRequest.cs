namespace UnitedRoad.GeoServices.Infrustructure
{
	using System.IO;
	using System.Net;
	using Newtonsoft.Json;

	public interface IMakeRequest
	{
		dynamic GetResponse(string url);
	}

	public class MakeRequest : IMakeRequest
	{
		public dynamic GetResponse(string url)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			var response = request.GetResponse();
			var dataStream = response.GetResponseStream();
			var sreader = new StreamReader(dataStream);
			var responsereader = sreader.ReadToEnd();
			response.Close();

			return JsonConvert.DeserializeObject(responsereader);
		}
	}
}