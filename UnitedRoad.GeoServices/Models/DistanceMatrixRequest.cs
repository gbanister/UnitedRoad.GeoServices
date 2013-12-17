namespace UnitedRoad.GeoServices.Models
{
	using System.Collections.Generic;

	public class DistanceMatrixRequest
	{
		public IEnumerable<string> Origins { get; set; }
		public IEnumerable<string> Destinations { get; set; } 
	}
}