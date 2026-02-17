namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;

	/// <summary>
	/// Thrown when a People and Organizations operation failed.
	/// </summary>
	public class PeopleAndOrganizationsException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PeopleAndOrganizationsException"/> class with a specified error data.
		/// </summary>
		/// <param name="data">The <see cref="PeopleAndOrganizationsErrorData"/> that describes the error.</param>
		public PeopleAndOrganizationsException(PeopleAndOrganizationsErrorData data)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			TraceData = new PeopleAndOrganizationsTraceData();
			TraceData.ErrorData.Add(data);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PeopleAndOrganizationsException"/> class with specified trace data.
		/// </summary>
		/// <param name="data">The <see cref="PeopleAndOrganizationsTraceData"/> that contains the trace information of the error.</param>
		public PeopleAndOrganizationsException(PeopleAndOrganizationsTraceData data)
		{
			TraceData = data ?? new PeopleAndOrganizationsTraceData();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PeopleAndOrganizationsException"/> class with the specified error message.
		/// </summary>
		/// <param name="message">The error message that describes the reason for the exception.</param>
		public PeopleAndOrganizationsException(string message)
			: this(new PeopleAndOrganizationsErrorData { ErrorMessage = message })
		{
		}

		/// <summary>
		/// Gets the trace data associated with this exception.
		/// </summary>
		public PeopleAndOrganizationsTraceData TraceData { get; private set; }

		/// <summary>
		/// Gets the error message that explains the reason for this <see cref="PeopleAndOrganizationsException" />.
		/// </summary>
		public override string Message
		{
			get
			{
				if (TraceData.ErrorData.Count == 1)
				{
					return TraceData.ErrorData[0].ErrorMessage;
				}

				return TraceData.ToString();
			}
		}

		/// <summary>
		/// Returns a string that represents the current exception.
		/// </summary>
		/// <returns>A string that represents the current exception, including the trace data.</returns>
		public override string ToString()
		{
			return $"{base.ToString()}{Environment.NewLine}Containing TraceData:{Environment.NewLine}{TraceData}";
		}
	}
}
