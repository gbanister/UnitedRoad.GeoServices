namespace UnitedRoad.GeoServices.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Web.Http;
	using System.Web.Http.Controllers;
	using System.Web.Http.ModelBinding;
	using Newtonsoft.Json;

	public class DistanceMatrixController : ApiController
	{

		public IEnumerable<dynamic> Get([ModelBinder(typeof (DelimitedArrayModelBinder))] 
			                                string[] origins,
		                                [ModelBinder(typeof(DelimitedArrayModelBinder))] 
			                                string[] destinations)
		{
			var originsString = string.Join("|", origins);
			var destinationsString = string.Join("|", destinations);

			var url = @"http://maps.googleapis.com/maps/api/distancematrix/json?origins=" +
			          originsString + "&destinations=" + destinationsString +
			          "&mode=driving&sensor=false&language=en-EN&units=imperial";

			var request = (HttpWebRequest) WebRequest.Create(url);
			var response = request.GetResponse();
			var dataStream = response.GetResponseStream();
			var sreader = new StreamReader(dataStream);
			var responsereader = sreader.ReadToEnd();
			response.Close();

			dynamic result = JsonConvert.DeserializeObject(responsereader);

			if (result.status != "OK") yield break;
			
			var originIndex = 0;
			foreach (var origin in result.origin_addresses)
			{
				var destinationIndex = 0;
				foreach (var destination in result.destination_addresses)
				{
					string distanceString = result.rows[originIndex].elements[destinationIndex].distance.text;
					var distance = Convert.ToDecimal(distanceString.Replace(" mi", "").Replace(" ft", "").Replace(",", ""));
					yield return new
						{
							Origin = origin,
							Destination = destination,
							Distance = distance
						};
					destinationIndex++;
				}
				originIndex++;
			}
		}

		public class DelimitedArrayModelBinder : IModelBinder
		{
			public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
			{
				var key = bindingContext.ModelName;
				var val = bindingContext.ValueProvider.GetValue(key);
				if (val != null)
				{
					var s = val.AttemptedValue;
					if (s != null)
					{
						var elementType = bindingContext.ModelType.GetElementType();
						var converter = TypeDescriptor.GetConverter(elementType);
						var values =
							s.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries)
							 .Select(converter.ConvertFromString).ToArray();

						var typedValues = Array.CreateInstance(elementType, values.Length);

						values.CopyTo(typedValues, 0);

						bindingContext.Model = typedValues;
					}
					else
					{
						// change this line to null if you prefer nulls to empty arrays 
						bindingContext.Model = Array.CreateInstance(bindingContext.ModelType.GetElementType(), 0);
					}
					return true;
				}
				return false;
			}
		}
	}
}