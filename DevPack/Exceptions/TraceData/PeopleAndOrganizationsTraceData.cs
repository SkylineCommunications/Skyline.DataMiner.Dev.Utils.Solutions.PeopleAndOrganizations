namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// Contains all kinds of data that MediaOps could generate while handling a request.
	/// </summary>
	public class PeopleAndOrganizationsTraceData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PeopleAndOrganizationsTraceData"/> class.
		/// Creates an empty trace data object.
		/// </summary>
		public PeopleAndOrganizationsTraceData()
		{
		}

		/// <summary>
		/// Gets the error data that was generated while handling the request.
		/// </summary>
		/// <returns>Never null.</returns>
		public List<PeopleAndOrganizationsErrorData> ErrorData { get; private set; } = new List<PeopleAndOrganizationsErrorData>();

		/// <summary>
		/// Returns all the data contained in the object in a readable format.
		/// Is also log-friendly.
		/// </summary>
		/// <returns>A string representation of the trace data.</returns>
		public override string ToString()
		{
			var info = new StringBuilder();
			info.Append($"TraceData: (amount = {ErrorData.Count})\n");

			if (ErrorData.Count != 0)
			{
				info.Append($"  - ErrorData: (amount = {ErrorData.Count})\n");
				info.Append($"      - {string.Join("\n      - ", ErrorData)}\n");
			}

			return info.ToString();
		}

		/// <summary>
		/// Returns true if the object does not contain any errors indicating failure of a operation.
		/// </summary>
		/// <returns>True if the operation succeeded; otherwise, false.</returns>
		public bool HasSucceeded()
		{
			return ErrorData.Count == 0;
		}

		/// <summary>
		/// Adds error data.
		/// </summary>
		/// <param name="errorData">The error data to add. Cannot be null.</param>
		public void Add(PeopleAndOrganizationsErrorData errorData)
		{
			if (errorData == null)
			{
				throw new ArgumentNullException(nameof(errorData));
			}

			ErrorData.Add(errorData);
		}
	}
}
