namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Example validator for objects identified by string IDs instead of Guids.
	/// </summary>
	/// <typeparam name="T">The type of object being validated.</typeparam>
	internal class StringApiObjectValidator<T> : ApiObjectValidator<T, string>
	{
		private readonly List<string> successfulIds = new List<string>();
		private readonly Func<T, string> idExtractor;

		internal override IReadOnlyCollection<string> SuccessfulIds => successfulIds;

		/// <summary>
		/// Initializes a new instance of the <see cref="StringApiObjectValidator{T}"/> class.
		/// </summary>
		/// <param name="idExtractor">Function to extract the string ID from an object of type T.</param>
		public StringApiObjectValidator(Func<T, string> idExtractor)
		{
			this.idExtractor = idExtractor ?? throw new ArgumentNullException(nameof(idExtractor));
		}

		protected override void ReportSuccess(T item)
		{
			var id = idExtractor(item);
			if (string.IsNullOrEmpty(id))
			{
				throw new InvalidOperationException("Cannot report success for an item with a null or empty ID");
			}

			if (unsuccessfulItems.Contains(id))
			{
				throw new InvalidOperationException($"An item cannot be marked as both successful and unsuccessful");
			}

			successfulIds.Add(id);
			successfulItems.Add(item);
		}
	}
}
