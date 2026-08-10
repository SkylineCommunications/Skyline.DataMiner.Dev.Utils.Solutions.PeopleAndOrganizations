namespace RT_PeopleAndOrganizations.Querying
{
	using System;
	using System.Linq;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API.Querying;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	using SLDataGateway.API.Querying;
	using SLDataGateway.API.Types.Querying;

	/// <summary>
	/// Validates that the order by of a query is translated into an order by on the underlying storage.
	/// </summary>
	[TestClass]
	public class QueryTranslationTests
	{
		[TestMethod]
		public void TranslateFullOrderBy_Team()
		{
			var translator = new TeamFilterTranslator();

			AssertTranslatedField(translator, TeamExposers.Id, DomInstanceExposers.Id);
			AssertTranslatedField(translator, TeamExposers.Name, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamName));
			AssertTranslatedField(translator, TeamExposers.Email, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamEmail));
			AssertTranslatedField(translator, TeamExposers.Description, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamDescription));
			AssertTranslatedField(translator, TeamExposers.IsBookable, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.Bookable));
			AssertTranslatedField(translator, TeamExposers.State, DomInstanceExposers.StatusId);
		}

		[TestMethod]
		public void TranslateFullOrderBy_Person()
		{
			var translator = new PersonFilterTranslator();

			AssertTranslatedField(translator, PersonExposers.Id, DomInstanceExposers.Id);
			AssertTranslatedField(translator, PersonExposers.Name, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.PeopleInformation.FullName));
			AssertTranslatedField(translator, PersonExposers.Email, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.Email));
			AssertTranslatedField(translator, PersonExposers.Phone, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.Phone));
			AssertTranslatedField(translator, PersonExposers.StreetAddress, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.StreetAddress));
			AssertTranslatedField(translator, PersonExposers.City, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.City));
			AssertTranslatedField(translator, PersonExposers.Country, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.Country));
			AssertTranslatedField(translator, PersonExposers.ZipCode, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.ZIP));
			AssertTranslatedField(translator, PersonExposers.ExperienceId, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.PeopleInformation.ExperienceLevel));
			AssertTranslatedField(translator, PersonExposers.OrganizationId, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Organization.Organization_57695f03));
			AssertTranslatedField(translator, PersonExposers.State, DomInstanceExposers.StatusId);
			AssertTranslatedField(translator, PersonExposers.ResourceId, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Resource.LinkedResource));
			AssertTranslatedField(translator, PersonExposers.TeamMemberships.TeamId, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Team.Team_144d3379));
			AssertTranslatedField(translator, PersonExposers.TeamMemberships.RoleId, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Team.TeamRole));
		}

		[TestMethod]
		public void TranslateFullOrderBy_Organization()
		{
			var translator = new OrganizationFilterTranslator();

			AssertTranslatedField(translator, OrganizationExposers.Id, DomInstanceExposers.Id);
			AssertTranslatedField(translator, OrganizationExposers.Name, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.OrganizationInformation.OrganizationName));
			AssertTranslatedField(translator, OrganizationExposers.CategoryId, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.OrganizationInformation.Category));
			AssertTranslatedField(translator, OrganizationExposers.State, DomInstanceExposers.StatusId);
		}

		[TestMethod]
		public void TranslateFullOrderBy_Role()
		{
			var translator = new RoleFilterTranslator();

			AssertTranslatedField(translator, RoleExposers.Id, DomInstanceExposers.Id);
			AssertTranslatedField(translator, RoleExposers.Name, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.RoleInformation.Role));
		}

		[TestMethod]
		public void TranslateFullOrderBy_Category()
		{
			var translator = new CategoryFilterTranslator();

			AssertTranslatedField(translator, CategoryExposers.Id, DomInstanceExposers.Id);
			AssertTranslatedField(translator, CategoryExposers.Name, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.CategoryInformation.Category));
		}

		[TestMethod]
		public void TranslateFullOrderBy_Experience()
		{
			var translator = new ExperienceFilterTranslator();

			AssertTranslatedField(translator, ExperienceExposers.Id, DomInstanceExposers.Id);
			AssertTranslatedField(translator, ExperienceExposers.Name, DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ExperienceInformation.Experience));
		}

		[TestMethod]
		public void TranslateFullOrderBy_KeepsSortOrderAndNaturalSort()
		{
			var translator = new TeamFilterTranslator();

			var ascending = Translate(translator, TeamExposers.Name, SortOrder.Ascending, false);
			Assert.AreEqual(SortOrder.Ascending, ascending.SortOrder);
			Assert.IsFalse(ascending.Options.NaturalSort);

			var descending = Translate(translator, TeamExposers.Name, SortOrder.Descending, true);
			Assert.AreEqual(SortOrder.Descending, descending.SortOrder);
			Assert.IsTrue(descending.Options.NaturalSort);
		}

		[TestMethod]
		public void TranslateFullOrderBy_KeepsMultipleElementsInOrder()
		{
			var translator = new TeamFilterTranslator();
			var order = new OrderBy(new[]
			{
				CreateOrderByElement(TeamExposers.Name, SortOrder.Descending, false),
				CreateOrderByElement(TeamExposers.Email, SortOrder.Ascending, false),
			});

			var translated = translator.TranslateFullOrderBy(order).Elements.ToList();

			Assert.AreEqual(2, translated.Count);
			Assert.AreEqual(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamName).fieldName, translated[0].Exposer.fieldName);
			Assert.AreEqual(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamEmail).fieldName, translated[1].Exposer.fieldName);
		}

		[TestMethod]
		public void TranslateFullOrderBy_WithoutElements()
		{
			var translator = new TeamFilterTranslator();

			var translated = translator.TranslateFullOrderBy(OrderBy.Default);

			Assert.IsFalse(translated.Elements.Any());
		}

		[TestMethod]
		public void TranslateFullOrderBy_WithNullThrows()
		{
			var translator = new TeamFilterTranslator();

			Assert.ThrowsException<ArgumentNullException>(() => translator.TranslateFullOrderBy(null));
		}

		[TestMethod]
		public void TranslateFullOrderBy_WithUnsupportedFieldThrows()
		{
			var translator = new TeamFilterTranslator();
			var order = new OrderBy(new[] { CreateOrderByElement(PersonExposers.Phone, SortOrder.Ascending, false) });

			Assert.ThrowsException<NotSupportedException>(() => translator.TranslateFullOrderBy(order));
		}

		[TestMethod]
		public void TranslateFilter_WithNotFilter()
		{
			var translator = new TeamFilterTranslator();
			var filter = new NOTFilterElement<Team>(TeamExposers.Name.Equal("abc"));

			var translated = translator.TranslateFilter(filter);

			Assert.IsNotNull(translated);
		}

		private static IOrderByElement CreateOrderByElement(FieldExposer exposer, SortOrder sortOrder, bool naturalSort)
		{
			return OrderByElement.Default
				.WithFieldExposer(exposer)
				.WithSortOrder(sortOrder)
				.WithNaturalSort(naturalSort);
		}

		private static IOrderByElement Translate<T>(FilterTranslator<T, DomInstance> translator, FieldExposer exposer, SortOrder sortOrder, bool naturalSort)
			where T : ApiObject
		{
			var order = new OrderBy(new[] { CreateOrderByElement(exposer, sortOrder, naturalSort) });

			return translator.TranslateFullOrderBy(order).Elements.Single();
		}

		private static void AssertTranslatedField<T>(FilterTranslator<T, DomInstance> translator, FieldExposer exposer, FieldExposer expectedExposer)
			where T : ApiObject
		{
			var translated = Translate(translator, exposer, SortOrder.Ascending, false);

			Assert.AreEqual(expectedExposer.fieldName, translated.Exposer.fieldName, $"Unexpected translation for field '{exposer.fieldName}'.");
		}
	}
}
