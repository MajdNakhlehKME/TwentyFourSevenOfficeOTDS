using System;
using TwentyFourSevenOfficeOTDS;
using TwentyFourSevenOfficeOTDS.ExtensionMethods;

namespace CLI
{
	class Program
	{
		static void Main(string[] args)
		{
            #region Data source and credentials
            var dataSource = new DataSource();

			var dataConnection = dataSource.OpenConnection
			(
				"Username=majd.nakhleh@konicaminolta.no;Password=Mavnjdgymfzie;ApplicationId=41dbae94-2d95-47ec-bab7-d8943c734e3a",
				Guid.Empty
			);

			Console.WriteLine("Authentication successful.");
			#endregion

			#region Printing results
			//Type of results we want to get:
			dataConnection.PrepareForDataRetrieval("type=department");

			//// Retrieve data.
			Console.WriteLine("Showing data: ");


			var count = 0;
			
			foreach (var i in dataConnection.GetItems())
			{ 
				
				Console.WriteLine("Item number " + (++count));

				foreach (var c in dataConnection.GetAvailableColumns())
				{
					Console.WriteLine("\tColumn: " + c.Name + "		Value: " + i.GetValue(c.Ordinal));
				}
			}

			#endregion
		 }
    }
}
