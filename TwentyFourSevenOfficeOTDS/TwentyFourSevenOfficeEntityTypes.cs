namespace TwentyFourSevenOfficeOTDS
{
	/// <summary>
	/// The entity types exposed by 24sevenoffice.
	/// </summary>
	public enum TwentyFourSevenOfficeEntityTypes
	{
        #region Entity types

        /// <summary>
        /// Not set or not configured; typically results in an exception if used.
        /// </summary>
        Unknown = 0,

		/// <summary>
		/// Invoices.
		/// </summary>
		Invoices = 1,

		/// <summary>
		/// Persons.
		/// </summary>
		Persons = 2,

		/// <summary>
		/// Departments.
		/// </summary>
		Departments = 3

		#endregion
	}
}