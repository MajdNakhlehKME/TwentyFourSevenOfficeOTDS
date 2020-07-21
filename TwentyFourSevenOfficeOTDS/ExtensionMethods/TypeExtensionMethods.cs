using System;
using System.Collections.Generic;
using System.Reflection;
using MFiles.Server.Extensions;
using TwentyFourSevenOfficeOTDS.TwentyFourSevenOffice.Services.PersonService;

namespace TwentyFourSevenOfficeOTDS.ExtensionMethods
{
	/// <summary>
	/// Extension methods for interacting with .NET types.
	/// </summary>
	public static class TypeExtensionMethods
	{

		/// <summary>
		/// Gets the M-Files-supported column definitions from a <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type to reflect.</param>
		/// <returns>Information about the properties and fields exposed by the type.</returns>
		public static IEnumerable<ColumnDefinition> GetColumnDefinitions
		(
			this Type type,
			int ordinalStart = 0
		)
		{
			// Counter for the ordinal/index.
			var index = ordinalStart;

			// Grab the fields.
			foreach (var fi in type.GetFields(BindingFlags.Instance | BindingFlags.Public ))
			{
				// Is it a data type that we support?
				if(fi.FieldType != typeof(string)
					&& fi.FieldType != typeof(int)
					&& fi.FieldType != typeof(decimal)
					&& fi.FieldType != typeof(double)
					&& fi.FieldType != typeof(DateTime)
					&& fi.FieldType != typeof(bool)
					&& fi.FieldType != typeof(long)
					&& fi.FieldType != typeof(EmailAddress[])
					&& fi.FieldType != typeof(Address)
				)
					continue;
				
				// Deal with addresses.
				if(fi.FieldType == typeof(Address))
				{
					// Return the properties and fields from the address.
					var maxOrdinal = 0;
					foreach(var columnDefinition in fi.FieldType.GetColumnDefinitions(index))
					{
						yield return new ColumnDefinition()
						{
							Ordinal = columnDefinition.Ordinal,
							Name = fi.Name + "." + columnDefinition.Name,
							Type = columnDefinition.Type
						};
						maxOrdinal = (columnDefinition.Ordinal > maxOrdinal)
							? columnDefinition.Ordinal
							: maxOrdinal;
					}

					// Increment index.
					index = maxOrdinal + 1;

					// Stop.
					continue;
				}
				
				// Deal with email addresses.
				if(fi.FieldType == typeof(EmailAddress[]))
				{
					// For email addresses we need to return the actual email address.
					yield return new ColumnDefinition()
					{
						Ordinal = index++,
						Name = fi.Name,
						Type = typeof(string)
					};
					continue;
				}

				// Return the column definition.
				yield return new ColumnDefinition()
				{
					Ordinal = index++,
					Name = fi.Name,
					Type = fi.FieldType == typeof(decimal)
						? typeof(double) // Decimals must be doubles.
						: fi.FieldType == typeof(long)
						? typeof(string) // Longs should be strings
						: fi.FieldType
				};
			}

			// Grab the properties.
			foreach (var pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				// Is it a data type that we support?
				if(pi.PropertyType != typeof(string)
					&& pi.PropertyType != typeof(int)
					&& pi.PropertyType != typeof(decimal)
					&& pi.PropertyType != typeof(double)
					&& pi.PropertyType != typeof(DateTime)
					&& pi.PropertyType != typeof(bool)
					&& pi.PropertyType != typeof(long)
					&& pi.PropertyType != typeof(EmailAddress[])
					&& pi.PropertyType != typeof(Address)
				)
					continue;
				
				// Deal with addresses.
				if(pi.PropertyType == typeof(Address))
				{
					// Return the properties and fields from the address.
					var maxOrdinal = 0;
					foreach(var columnDefinition in pi.PropertyType.GetColumnDefinitions(index))
					{
						yield return new ColumnDefinition()
						{
							Ordinal = columnDefinition.Ordinal,
							Name = pi.Name + "." + columnDefinition.Name,
							Type = columnDefinition.Type
						};
						maxOrdinal = (columnDefinition.Ordinal > maxOrdinal)
							? columnDefinition.Ordinal
							: maxOrdinal;
					}

					// Increment index.
					index = maxOrdinal + 1;

					// Stop.
					continue;
				}

				// Deal with email addresses.
				if(pi.PropertyType == typeof(EmailAddress[]))
				{
					// For email addresses we need to return the actual email address.
					yield return new ColumnDefinition()
					{
						Ordinal = index++,
						Name = pi.Name,
						Type = typeof(string)
					};
				}

				// If the property doesn't have a getter then don't add it.
				if(false == pi.CanRead)
					continue;

				// Return the column definition.
				yield return new ColumnDefinition()
				{
					Ordinal = index++,
					Name = pi.Name,
					Type = pi.PropertyType == typeof(decimal)
						? typeof(double) // Decimals must be doubles.
						: pi.PropertyType == typeof(long)
						? typeof(string) // Longs should be strings
						: pi.PropertyType
				};
			}
		}
	}
}
