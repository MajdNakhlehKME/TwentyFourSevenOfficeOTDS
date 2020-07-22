using System;
using System.Linq;
using System.Net;
using MFiles.Server.Extensions;
using TwentyFourSevenOfficeOTDS.ExtensionMethods;
using TwentyFourSevenOfficeOTDS.TwentyFourSevenOffice.Services.Authenticate;
using TwentyFourSevenOfficeOTDS.TwentyFourSevenOffice.Services.ClientService;
using TwentyFourSevenOfficeOTDS.TwentyFourSevenOffice.Services.Invoices;
using TwentyFourSevenOfficeOTDS.TwentyFourSevenOffice.Services.PersonService;

namespace TwentyFourSevenOfficeOTDS
{
	/// <summary>
	/// The underlying data provider, wrapping the implementation of the underlying web service calls.
	/// </summary>
	public class TwentyFourSevenOfficeDataProvider
	{

		#region Defining cookies
		/// <summary>
		/// The cookie container used to hold the ASP.NET session ID used for authentication.
		/// </summary>
		public CookieContainer CookieContainer { get; set; } = new CookieContainer();
		#endregion

		#region Authenticating the service
		/// <summary>
		/// Authenticates to 24sevenoffice web services using the data from <paramref name="dataSource"/>.
		/// </summary>
		/// <param name="dataSource">The configured data source containing authentication details.</param>
		public void Authenticate(DataSource dataSource)
		{
			this.Authenticate(dataSource.Username, dataSource.Password, dataSource.ApplicationId);
		}

		/// <summary>
		/// Authenticates to 24sevenoffice web services, populating <see cref="CookieContainer"/> with
		/// the current access token.
		/// Does not re-authenticate if <see cref="CookieContainer"/> already contains a valid token.
		/// </summary>
		/// <param name="username">The username to connect with.</param>
		/// <param name="password">The password to connect with.</param>
		/// <param name="applicationId">The application Id for this integration.</param>
		public void Authenticate(string username, string password, Guid applicationId)
		{

            
            using (var authService = new Authenticate { CookieContainer = this.CookieContainer })
			{
				// If we still have a valid auth token then die.
				if (authService.HasSession())
					return;

				// Configure the credentials to use.
				var credential = new Credential
				{
					Username = username, // Always email address
					Password = password,
					ApplicationId = applicationId,
					IdentityId = Guid.Empty // Always empty
				};
				// Do the actual log in
				var sessionId = authService.Login(credential);
				if (string.IsNullOrEmpty(sessionId))
				{
					//Login returned empty string, login has failed.
					throw new Exception
					(
					"Error logging into 24sevenoffice web services.  Credentials (username, password, application Id) may be incorrect."
					);
				}

				// Login was successful.
				// Persist session cookie into the container.
				this.CookieContainer.Add
				(
					new Cookie
					(
						"ASP.NET_SessionId",
						sessionId
					)
					{
						Domain = "webservices.24sevenoffice.com"
					});

			}
		}
		#endregion

		#region Invoices
		/// <summary>
		/// Retrieves all invoices in 24sevenoffice that were altered after <paramref name="changedAfter"/>.
		/// </summary>
		/// <param name="dataSource">The data source containing the 24sevenoffice configuration parameters.</param>
		/// <param name="changedAfter">The date to retrieve data after.  Should be a VERY EARLY date (i.e. to return everything) unless <see cref="DataSourceConnection"/> also implements <see cref="IDataSourceConnection2"/>.</param>
		/// <returns>The invoices changed since <paramref name="changedAfter"/>.</returns>
		public InvoiceOrder[] GetInvoices(DataSource dataSource, DateTime changedAfter)
		{
			// Authenticate.
			this.Authenticate(dataSource);

			// Connect to the invoice service.
			using (var invoiceService = new InvoiceService()
			{
				CookieContainer = this.CookieContainer
			})
			{
				// Retrieve the invoices.
				return invoiceService.GetInvoices
				(
					new InvoiceSearchParameters()
					{
						ChangedAfter = changedAfter
					},
					typeof(InvoiceOrder)
						.GetColumnDefinitions()
						.Select(c => c.Name)
						.ToArray(),
					new string[0]
				);

			}
		}
		#endregion

		#region Persons
		public PersonItem[] GetPersons(DataSource dataSource, DateTime changedAfter)
        {
			//Authenticate
			this.Authenticate(dataSource);

			//Connect to the person service
			using (var personService = new PersonService()
			{
				CookieContainer = this.CookieContainer
            })
            {
				//Retrive the persons
				return personService.GetPersonsDetailed
				(
					new PersonSearchParameters()
					{
						IsEmployee = TriState.True,
						Id = 3005346
					}

				);
            }
        }
		#endregion

		#region Departments
		public Department[] GetDepartments(DataSource dataSource, DateTime changedAfter)
        {
			//Authenticate
			this.Authenticate(dataSource);

			//Connect to the person service
			using (var clientService = new ClientService()
			{
				CookieContainer = this.CookieContainer
			})
			{
				//Retrive the persons
				return clientService.GetDepartmentList();

			}
		}

        #endregion

    }
}