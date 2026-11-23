namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Tools;

	/// <summary>
	/// Defines extension methods on the <see cref="IPeopleAndOrganizationsApi"/> interface.
	/// </summary>
	public static class IPeopleAndOrganizationsApiExtensions
	{
		/// <summary>
		/// Installs or reconciles the DOM schema owned by People and Organizations.
		/// </summary>
		/// <param name="api">A People and Organizations API client created by the DevPack.</param>
		/// <remarks>
		/// Requires permission to manage DOM module settings. Differing module settings and declared
		/// behaviors are replaced in full, not merged. Undeclared fields and definition section links
		/// are retained as soft-deleted metadata. This does not create or migrate DOM instances or
		/// register the solution. If the DOM Manager cannot be reinitialized, the schema writes remain
		/// in place. Fix the cause and retry with the same package.
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="api"/> is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException"><paramref name="api"/> is not a DevPack-created client.</exception>
		/// <exception cref="InvalidOperationException">The DOM Manager could not be reinitialized after writing the schema.</exception>
		public static void InstallDomModules(this IPeopleAndOrganizationsApi api)
		{
			if (api == null)
			{
				throw new ArgumentNullException(nameof(api));
			}

			if (!(api is PeopleAndOrganizationsApi client))
			{
				throw new NotSupportedException("DOM schema installation requires a People and Organizations API client created by the DevPack.");
			}

			DomModuleInstaller.Install(client.Connection.HandleMessages, message => client.Logger.Information(message));
		}
	}
}
