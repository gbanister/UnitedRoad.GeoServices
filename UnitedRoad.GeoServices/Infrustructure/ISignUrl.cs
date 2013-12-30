namespace UnitedRoad.GeoServices.Infrustructure
{
	using System;
	using System.Security.Cryptography;
	using System.Text;

	public interface ISignUrl
	{
		string Sign(string url, string keyString);
	}

	public class SignUrl : ISignUrl
	{
		public string Sign(string url, string keyString)
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