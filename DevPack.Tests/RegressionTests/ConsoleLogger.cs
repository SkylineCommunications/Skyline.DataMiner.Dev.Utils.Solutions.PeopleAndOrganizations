namespace RT_PeopleAndOrganizations.RegressionTests
{
	using System;

	internal class ConsoleLogger : Skyline.DataMiner.Solutions.PeopleAndOrganizations.Logging.ILogger
	{
		public void Debug(object callerInstance, string message, object[]? args = null, string methodName = "")
		{
			var formattedMessage = args != null ? string.Format(message, args) : message;
			var prefix = string.IsNullOrEmpty(methodName) ? string.Empty : $"[{methodName}] ";
			Console.WriteLine($"DEBUG: {prefix}{formattedMessage}");
		}

		public void Debug(string message)
		{
			Console.WriteLine($"DEBUG: {message}");
		}

		public void Error(object callerInstance, string message, object[]? args = null, string methodName = "")
		{
			var formattedMessage = args != null ? string.Format(message, args) : message;
			var prefix = string.IsNullOrEmpty(methodName) ? string.Empty : $"[{methodName}] ";
			Console.WriteLine($"ERROR: {prefix}{formattedMessage}");
		}

		public void Error(string message)
		{
			Console.WriteLine($"ERROR: {message}");
		}

		public void Information(object callerInstance, string message, object[]? args = null, string methodName = "")
		{
			var formattedMessage = args != null ? string.Format(message, args) : message;
			var prefix = string.IsNullOrEmpty(methodName) ? string.Empty : $"[{methodName}] ";
			Console.WriteLine($"INFO: {prefix}{formattedMessage}");
		}

		public void Information(string message)
		{
			Console.WriteLine($"INFO: {message}");
		}

		public void Warning(object callerInstance, string message, object[]? args = null, string methodName = "")
		{
			var formattedMessage = args != null ? string.Format(message, args) : message;
			var prefix = string.IsNullOrEmpty(methodName) ? string.Empty : $"[{methodName}] ";
			Console.WriteLine($"WARNING: {prefix}{formattedMessage}");
		}

		public void Warning(string message)
		{
			Console.WriteLine($"WARNING: {message}");
		}
	}
}
