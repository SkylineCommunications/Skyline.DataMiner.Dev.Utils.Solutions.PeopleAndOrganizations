namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// Thrown when a People and Organizations bulk operation failed.
	/// </summary>
	/// <typeparam name="K">The type of the identifier used in the bulk operation.</typeparam>
	public class PeopleAndOrganizationsBulkException<K> : Exception
		where K : IEquatable<K>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PeopleAndOrganizationsBulkException{K}"/> class with the specified bulk operation result.
		/// </summary>
		/// <param name="result">The result of the bulk operation that caused the exception.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is <c>null</c>.</exception>
		public PeopleAndOrganizationsBulkException(IBulkOperationResult<K> result)
		{
			Result = result ?? throw new ArgumentNullException(nameof(result));
		}

		/// <summary>
		/// Gets the result of the bulk operation that caused the exception.
		/// </summary>
		public IBulkOperationResult<K> Result { get; }

		/// <summary>
		/// Returns a string that represents the current exception, including details about the bulk operation result.
		/// </summary>
		/// <returns>A string representation of the exception.</returns>
		public override string ToString()
		{
			const int maxSuccessfulIds = 10;
			var successfulIds = string.Join(", ", Result.SuccessfulIds.Take(maxSuccessfulIds));
			if (Result.SuccessfulIds.Count > maxSuccessfulIds)
			{
				successfulIds += $", ... ({Result.SuccessfulIds.Count - maxSuccessfulIds} more)";
			}

			var preLines = new List<string>(3 + Result.UnsuccessfulIds.Count)
									{
										$"Bulk CRUD operation: {Result.SuccessfulIds.Count} succeeded, {Result.UnsuccessfulIds.Count} failed",
										$" - IDs of the successful items: {successfulIds}",
										$" - Failures:",
									};

			var traces = Result.TraceDataPerItem;
			foreach (var id in Result.UnsuccessfulIds)
			{
				if (!traces.TryGetValue(id, out var traceData))
				{
					continue;
				}

				var traceDataLines = traceData?.ToString().Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None) ?? Array.Empty<string>();
				preLines.Add($"  - {id}:");
				preLines.AddRange(traceDataLines.Select(x => $"    {x}"));
			}

			var lines = preLines.Concat(new[] { base.ToString() });
			return string.Join(Environment.NewLine, lines);
		}
	}
}
