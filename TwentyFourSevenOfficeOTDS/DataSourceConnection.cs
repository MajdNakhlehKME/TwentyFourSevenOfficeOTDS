using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MFiles.Server.Extensions;
using TwentyFourSevenOfficeOTDS.ExtensionMethods;
using TwentyFourSevenOfficeOTDS.TwentyFourSevenOffice.Services.ClientService;
using TwentyFourSevenOfficeOTDS.TwentyFourSevenOffice.Services.Invoices;
using TwentyFourSevenOfficeOTDS.TwentyFourSevenOffice.Services.PersonService;

namespace TwentyFourSevenOfficeOTDS
{
	/// <summary>
	/// Provides a connection to the 24sevenoffice web service.
	/// </summary>
	public class DataSourceConnection
		: ReadOnlyDataSourceConnection
	{
        #region Defining the data source
        /// <summary>
        /// The <see cref="DataSource"/> that created this connection.
        /// </summary>
        protected DataSource DataSource { get; private set; }

		/// <summary>
		/// The underlying provider that is used to retrieve data from 24sevenoffice.
		/// </summary>
		protected TwentyFourSevenOfficeDataProvider DataProvider { get; private set; }

		/// <summary>
		/// The type of entity to retrieve.  Populated from the "SELECT statement" in the object type details.
		/// </summary>
		protected TwentyFourSevenOfficeEntityTypes EntityTypeToRetrieve { get; private set; }
			= TwentyFourSevenOfficeEntityTypes.Unknown;

		/// <summary>
		/// Creates a data connection for the data source details provided.
		/// </summary>
		/// <param name="dataSource"></param>
		public DataSourceConnection(DataSource dataSource)
		{
			// Sanity.
			this.DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
			this.DataProvider = new TwentyFourSevenOfficeDataProvider();
		}
		#endregion

		#region Select statement in M-Files (In progress)
		//This gets the select statement from M-Files and tells the program what type of data the user wants to get.
		//Changes done:
		//
		/// <inheritdoc />
		public override void PrepareForDataRetrieval(string selectStatement)
		{
			// Parse the type out of the select statement.
			// "type=invoice"
			foreach (var kvp in selectStatement.SplitConfigurationString())
			{
				switch (kvp.Key?.ToLower().Trim())
				{
					// What was the type?
					case "type":
						switch (kvp.Value?.ToLower().Trim())
						{
							// Allow "invoice" and "invoices"
							case "invoice":
							case "invoices":
								this.EntityTypeToRetrieve = TwentyFourSevenOfficeEntityTypes.Invoices;
								break;

							// Allow "empoyee" and "employees"
							case "employee":
							case "employees":
								this.EntityTypeToRetrieve = TwentyFourSevenOfficeEntityTypes.Persons;
								break;

							// Allow "department" and "departments"
							case "department":
							case "departments":
								this.EntityTypeToRetrieve = TwentyFourSevenOfficeEntityTypes.Departments;
								break;

							default:
								// It was a type but not one we recognise.
								throw new InvalidOperationException
								(
									$"The type element of the select statement ({kvp.Value}) was invalid.  Can be: {string.Join(",", Enum.GetNames(typeof(TwentyFourSevenOfficeEntityTypes)))}."
								);
						}
						break;
				}
			}
		}
		#endregion

		#region Gets the columns based on the entity type (In progress)
		/// <inheritdoc />
		public override IEnumerable<ColumnDefinition> GetAvailableColumns()
		{
			Type outputType = null;

			// Populate the (.NET) type based on the entity type we're configured for.
			switch (this.EntityTypeToRetrieve)
			{
				// Invoice entity
				case TwentyFourSevenOfficeEntityTypes.Invoices:
					outputType = typeof(InvoiceOrder);
					break;

				// Person entity
				case TwentyFourSevenOfficeEntityTypes.Persons:
					outputType = typeof(PersonItem);
					break;

				// Department entity
				case TwentyFourSevenOfficeEntityTypes.Departments:
					outputType = typeof(Department);
					break;

				default:
					throw new InvalidOperationException
					(
						$"Type not configured in select statement or not supported ({this.EntityTypeToRetrieve})."
					);
			}

			return outputType.GetColumnDefinitions();
		}
		#endregion

		#region Gets the data based of the entity type (In progress)
		/// <inheritdoc />
		public override IEnumerable<DataItem> GetItems()
		{
			// The data to be returned to M-Files
			IEnumerable data = null;

			// Get the data based on the configured type
			switch (this.EntityTypeToRetrieve)
			{
				// Invoices
				case TwentyFourSevenOfficeEntityTypes.Invoices:
					data = this.DataProvider.GetInvoices
					(
						this.DataSource,
						new DateTime(2000, 01, 01)
					);
					break;

				// Persons
				case TwentyFourSevenOfficeEntityTypes.Persons:
					data = this.DataProvider.GetPersons
					(
						this.DataSource,
						new DateTime(2000, 01, 01)

					);
					break;

				// Departments
				case TwentyFourSevenOfficeEntityTypes.Departments:
					data = this.DataProvider.GetDepartments
					(
						this.DataSource,
						new DateTime(2000, 01, 01)

					);
					break;

				default:
					throw new InvalidOperationException
					(
						$"Type not configured in select statement or not supported ({this.EntityTypeToRetrieve})."
					);
			}

			// Return data if we have it.
			return null == data
				? new DataItem[0]
				: data.Cast<object>().Select(i => i.ToDataItem());
		}
        #endregion

    }
}