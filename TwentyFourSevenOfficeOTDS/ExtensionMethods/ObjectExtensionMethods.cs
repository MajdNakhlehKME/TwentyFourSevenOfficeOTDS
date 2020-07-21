using System;
using System.Collections.Generic;
using System.Linq;
using MFiles.Server.Extensions;

namespace TwentyFourSevenOfficeOTDS.ExtensionMethods
{
	/// <summary>
	/// Extension methods for working with objects that should be returned to M-Files.
	/// </summary>
	public static class ObjectExtensionMethods
	{
		/// <summary>
		/// Converts a .NET object into a <see cref="DataItem"/> by extracting its
		/// <see cref="ColumnDefinition"/> information and retrieving associated values
		/// for each property/field.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <returns>The object converted to a <see cref="DataItem"/>.</returns>
		public static DataItem ToDataItem(this object obj)
		{
			// Sanity.
			if(null == obj)
				return null;

			// Get the properties for this specific item.
			var properties = obj
				.GetType()
				.GetColumnDefinitions()
				.ToDictionary
				(
					column => column.Ordinal,  // The index is the key.
					column => obj.GetValue(column.Name) // The value of this property/field on this object.
				);

			// Create a data item from the properties.
			return new DataItemSimple(properties);
		}

		/// <summary>
		/// Uses reflection to get the value of a property or field on an object.
		/// </summary>
		/// <param name="obj">The object to look up the value on.</param>
		/// <param name="propertyOrFieldName">The name of the property or field.</param>
		/// <returns>The value of the property or field.</returns>
		private static object GetValue(this object obj, string propertyOrFieldName)
		{

			//// Sanity.
			if (null == obj)
				throw new ArgumentNullException(nameof(obj));
			if(string.IsNullOrWhiteSpace(propertyOrFieldName))
				throw new ArgumentException("The property or field name cannot be null or whitespace.", nameof(propertyOrFieldName));

			// Iterate over the property definitions.
			foreach (var property in obj.GetType().GetProperties())
			{
				// Is this the right property?
				if (property.Name != propertyOrFieldName)
					continue;

				// Try and read the value.
				try
				{
					var retVal =  property.GetValue(obj);
					// If it's a long then convert to a string.
					if (property.PropertyType == typeof(long))
						retVal = retVal?.ToString();
					return retVal;

				}
				catch (Exception e)
				{
					// TODO: Exception logging!
					return null;
				}
			}

			// Iterate over the field definitions.
			foreach (var field in obj.GetType().GetFields())
			{
				// Is this the right field?
				if (field.Name != propertyOrFieldName)
					continue;

				// Try and read the value.
				try
				{
					var retVal = field.GetValue(obj);
					// If it's a long then convert to a string.
					if (field.FieldType == typeof(long))
						retVal = retVal?.ToString();
					return retVal;

				}
				catch (Exception e)
				{
					// TODO: Exception logging!
					return null;
				}
			}

			// Could not find the property or field.
			throw new ArgumentException("The property or field could not be found", nameof(propertyOrFieldName));

		}
	}
}