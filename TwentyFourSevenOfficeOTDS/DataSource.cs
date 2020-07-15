using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using MFiles.Server.Extensions;
using TwentyFourSevenOfficeOTDS.ExtensionMethods;

namespace TwentyFourSevenOfficeOTDS
{
	/// <summary>
	/// Entry point for the external object type data source.
	/// </summary>
	public class DataSource
		: MFiles.Server.Extensions.IDataSource

	{

		#region Getting the credentials
		/// <summary>
		/// The username to connect to 24sevenoffice.
		/// </summary>
		public string Username { get; private set; }

		/// <summary>
		/// The password to connect to 24sevenoffice.
		/// </summary>
		public string Password { get; private set; }

		/// <summary>
		/// The application id to connect to 24sevenoffice.
		/// </summary>
		public Guid ApplicationId { get; private set; } = Guid.Empty;
        #endregion
		//s
        #region Implementation of IDataSource
		//Gets the credentials from M-Files
        /// <inheritdoc />
        public IDataSourceConnection OpenConnection
		(
			string connectionString,
			Guid configurationId
		)
		{
			// Parse the connection string for username, password and application Id.
			foreach (var kvp in connectionString.SplitConfigurationString())
			{
				switch (kvp.Key?.ToLower().Trim())
				{
					case "username":
						this.Username = kvp.Value;
						break;
					case "password":
						this.Password = kvp.Value;
						break;
					case "applicationid":
						if (Guid.TryParse(kvp.Value, out Guid applicationId))
						{
							this.ApplicationId = applicationId;
						}
						else
						{
							throw new InvalidOperationException("The application Id was not a valid GUID.");
						}

						break;
				}
			}

			// Validate.
			if (string.IsNullOrWhiteSpace(this.Username))
				throw new InvalidOperationException("The username must be provided in the connection string.");
			if (string.IsNullOrWhiteSpace(this.Password))
				throw new InvalidOperationException("The password must be provided in the connection string.");
			if (this.ApplicationId == Guid.Empty)
				throw new InvalidOperationException("The application Id must be provided in the connection string.");

			// Return a connection.
			return new DataSourceConnection(this);
		}

		/// <inheritdoc />
		public bool CanAlterData()
		{
			// We only support reading.
			return false;
		}
		#endregion

	}
}
