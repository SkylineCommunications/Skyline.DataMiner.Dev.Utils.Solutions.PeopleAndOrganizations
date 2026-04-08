namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;

	internal class ApiObjectValidator<T> : ObjectValidator<T, Guid> where T : ApiObject
	{
		private readonly List<Guid> successfulIds = new List<Guid>();

		internal override IReadOnlyCollection<Guid> SuccessfulIds => successfulIds;

		internal bool IsValid(IIdentifiable identifiable)
		{
			return IsValid(identifiable.Id);
		}

		protected override void ReportSuccess(T item)
		{
			if (unsuccessfulItems.Contains(item.Id))
			{
				throw new InvalidOperationException($"An item cannot be marked as both successful and unsuccessful");
			}

			successfulIds.Add(item.Id);
			successfulItems.Add(item);
		}
	}
}
