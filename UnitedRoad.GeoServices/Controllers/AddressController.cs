using System;
using System.Collections;
using System.Net;
using System.Web.Http;
using System.Xml;
using UnitedRoad.GeoServices.Infrustructure;

namespace UnitedRoad.GeoServices.Controllers
{
    public class AddressController : ApiController
    {
        private readonly IAppSettings _appSettings;
        private readonly ISignUrl _signUrl;
        private readonly IMakeRequest _makeRequest;

        public AddressController(ISignUrl signUrl, IMakeRequest makeRequest, IAppSettings appSettings)
        {
            _signUrl = signUrl;
            _makeRequest = makeRequest;
            _appSettings = appSettings;
        }

        //Correct address using Google Geocoding API  
        public Hashtable Get(string address1, string city, string state)
        {
	        var m_hashTable = new Hashtable();
	        m_hashTable.Clear();

            try {
		        address1 = address1.Replace(" CR ", " County Road ");
		        address1 = address1.Replace(" SR ", " State Road ");

		        //Construct the URL concating the address values with it  
		        const string zipcodeurl = "?address={0},+{1},+{2}&sensor=false";

                var urlParams = @"/json" + zipcodeurl;

                urlParams = String.Format(urlParams, address1.Replace(" ", "+"), city.Replace(" ", "+"), state.Replace(" ", "+"));

                urlParams += "&client=" + _appSettings.GoogleApiClientId();

                var fullUrl = Constants.GEO_CODE_BASE_URL + urlParams;

                var signedUrl = _signUrl.Sign(fullUrl, _appSettings.GoogleApiClientKey());

                var result = _makeRequest.GetResponse(signedUrl);

                if (result.status != "OK") return null;

                var containsPostalCode = false;
                foreach (var addressComponent in result.results[0].address_components)
                {
                    foreach (var type in addressComponent.types )
                    {
                        if (type != "postal_code") continue;
                        containsPostalCode = true;
                        break;
                    }
                    if (containsPostalCode) break;
                }

                if (containsPostalCode == false) return null;

                var count = 0;
                foreach (var addressComponent in result.results[0].address_components)
                {
                    //Get the zipLongName Element Value  
                    var longName = addressComponent.long_name;
                    //Get the zipShortName Element Value  
                    var shortName = addressComponent.short_name;
                    //Get the zipType Element Value  
                    var type = addressComponent.types[0];
                    //If the type of the component is postal_code then get the postal code as zipLongName  
                    if (type == "street_number")
                    {
                        m_hashTable.Add("street_number", longName);
                    }
                    if (type == "route")
                    {
                        m_hashTable.Add("street", longName);
                    }
                    if (type == "locality")
                    {
                        m_hashTable.Add("city", longName);
                    }
                    if (type == "administrative_area_level_1")
                    {
                        m_hashTable.Add("state", shortName);
                    }
                    if (type == "postal_code")
                    {                
                        m_hashTable.Add("zip", longName);
                    }
                    count = count + 1;
                }

            } 
            catch (WebException ex)
            {
                throw;
            }
	return m_hashTable;
}


    }
}