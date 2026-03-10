namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM;

	internal class DomInstanceApiObjectValidator<T> : ApiObjectValidator<T, Guid> where T : DomInstanceBase
	{
		private readonly List<Guid> successfulIds = new List<Guid>();

		internal override IReadOnlyCollection<Guid> SuccessfulIds => successfulIds;

		internal bool IsValid(IIdentifiable identifiable)
		{
			return IsValid(identifiable.Id);
		}

		protected override void ReportSuccess(T item)
		{
			if (unsuccessfulItems.Contains(item.ID.Id))
			{
				throw new InvalidOperationException($"An item cannot be marked as both successful and unsuccessful");
			}

			successfulIds.Add(item.ID.Id);
			successfulItems.Add(item);
		}
	}
}
