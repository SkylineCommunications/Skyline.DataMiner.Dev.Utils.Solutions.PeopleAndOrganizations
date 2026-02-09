namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	internal abstract class Repository
	{
		private readonly PeopleAndOrganizationsApi api;

		/// <summary>
		/// Initializes a new instance of the <see cref="Repository"/> class.
		/// </summary>
		/// <param name="api">The People and Organizations API instance.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="api"/> is <c>null</c>.</exception>
		protected Repository(PeopleAndOrganizationsApi api)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		/// <summary>
		/// Gets the People and Organizations API instance associated with this repository.
		/// </summary>
		public PeopleAndOrganizationsApi Api => api;
	}
}
