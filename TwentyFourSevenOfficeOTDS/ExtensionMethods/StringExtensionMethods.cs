using System;
using System.Collections.Generic;
using System.Linq;

namespace TwentyFourSevenOfficeOTDS.ExtensionMethods
{
	/// <summary>
	/// Extension methods for interacting with strings that represent configuration data formatted as:
	/// "key1=value1;key2=value2;key3=value3".
	/// </summary>
	public static class StringExtensionMethods
	{
		/// <summary>
		/// Parses the configuration string into its component parts.
		/// </summary>
		/// <param name="configurationString">The connection string to parse.</param>
		/// <param name="pairDelimiter">The string used to separate key-value pairs from one another (e.g. in "key1=value1;key2=value2", the delimiter is ";", in "key1=value1&amp;key2=value2", the delimiter is "&amp;").</param>
		/// <param name="keyValueDelimiter">The string used to separate keys from values within a pair (e.g. "in key1=value1;key2=value2", the delimiter is "=").</param>
		/// <returns>
		/// If the input is "a=b;c=d", returns two <see cref="KeyValuePair{TKey, TValue}"/> items,
		/// one with a key of "a" and value of "b", one with a key of "c" and a value of "d".
		/// </returns>
		public static IEnumerable<KeyValuePair<string, string>> SplitConfigurationString
		(
			this string configurationString,
			string pairDelimiter = ";",
			string keyValueDelimiter = "="
		)
		{
			// Sanity.
			if(null == configurationString)
				yield break;
			pairDelimiter = string.IsNullOrWhiteSpace(pairDelimiter)
				? ";"
				: pairDelimiter;
			keyValueDelimiter = string.IsNullOrWhiteSpace(keyValueDelimiter)
				? "="
				: keyValueDelimiter;

			// Split out each item.
			foreach (var item in configurationString.Split(pairDelimiter.ToCharArray()))
			{
				// Sanity.
				if(string.IsNullOrWhiteSpace(item))
					continue;
				
				// Split by the equals.
				var data = item.Trim().Split(keyValueDelimiter.ToCharArray());

				// How much data do we have?
				switch (data.Length)
				{
					// Is it just a flag (e.g. "encrypted" in "key1=value1;encrypted")?
					case 1:
						yield return new KeyValuePair<string, string>(data[0], string.Empty);
						break;

					// Is it a key and value pair?
					case 2:
						// Return the data.
						yield return new KeyValuePair<string, string>(data[0], data[1]);
						break;

					// Assume that the value contains kvp delimiters and combine them together.
					default: // "x=y=z=d" should be "x" equals "y=z=d".
						yield return new KeyValuePair<string, string>
						(
							data[0],
							string.Join("=", data.Skip(1))
						);
						break;
				}

			}
		}
	}
}
