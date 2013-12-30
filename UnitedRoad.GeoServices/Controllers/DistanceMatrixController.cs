namespace UnitedRoad.GeoServices.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Web;
	using System.Web.Http;
	using System.Web.Http.Controllers;
	using System.Web.Http.ModelBinding;
	using UnitedRoad.GeoServices.Infrustructure;

	public class DistanceMatrixController : ApiController
	{
		private readonly IAppSettings _appSettings;
		private readonly ISignUrl _signUrl;
		private readonly IMakeRequest _makeRequest;

		public DistanceMatrixController(IAppSettings appSettings, ISignUrl signUrl, IMakeRequest makeRequest)
		{
			_appSettings = appSettings;
			_signUrl = signUrl;
			_makeRequest = makeRequest;
		}


		public IEnumerable<dynamic> Get([ModelBinder(typeof (DelimitedArrayModelBinder))] 
			                                string[] origins,
		                                [ModelBinder(typeof(DelimitedArrayModelBinder))] 
			                                string[] destinations)
		{
			var originsString = string.Join("|", origins);
			var destinationsString = string.Join("|", destinations);

			var urlParams = @"/json?origins=" +
			          originsString + "&destinations=" + destinationsString +
			          "&mode=driving&sensor=false&language=en-EN&units=imperial";

			urlParams += "&client=" + _appSettings.GoogleApiClientId();

			var fullUrl = Constants.DISTANCE_MATRIX_BASE_URL + urlParams;

			var signedUrl = _signUrl.Sign(fullUrl, _appSettings.GoogleApiClientKey());

			var result = _makeRequest.GetResponse(signedUrl);

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