# Quick Reference

Common snippets for the public API in `Skyline.DataMiner.Solutions.PeopleAndOrganizations`.

## Instantiate `PeopleAndOrganizationsApi`

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
using Skyline.DataMiner.Net;

IConnection connection = /* create or retrieve connection */;
var api = connection.GetPeopleAndOrganizationsApi();

// Or use the extension method:
var api = engine.GetPeopleAndOrganizationsApi(); // For automation scripts
var api = protocol.GetPeopleAndOrganizationsApi(); // For protocols
var api = gqiDms.GetPeopleAndOrganizationsApi(); // For GQI data sources
```

## Access Repositories

Repositories are the primary way to interact with stored objects.

- CRUD operations (create, read, update, delete)
- Paged reading
- Batch operations
- State transitions

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

var organizationsRepo = api.Organizations;
var peopleRepo = api.People;
var teamsRepo = api.Teams;
var experienceRepo = api.Experience;
var categoriesRepo = api.Categories;
var rolesRepo = api.Roles;
var skillsRepo = api.Skills;
```

## Reading Objects

### Basic Reading

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// By ID
var person = api.People.Read(id);

// Multiple by IDs
var people = api.People.Read(new[] { id1, id2, id3 });

// All
var allPeople = api.People.Read();
```

### Paged Reading

For large datasets, use paged reading to process data in batches:

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Read all people in pages (default page size)
foreach (var page in api.People.ReadPaged())
{
    foreach (var person in page)
    {
        // Logic for processing person
    }
}

// Read with custom page size
foreach (var page in api.People.ReadPaged(pageSize: 50))
{
    ProcessBatch(page);
}
```

### Counting

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Count all people
var totalCount = api.People.Count();
```

## Create and Update Objects

### People

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Create a person
var person = api.People.Create(new Person
{
    Name = "John Doe",
    Email = "john.doe@example.com",
    Phone = "+1 555 0100",
    StreetAddress = "123 Main St",
    City = "Ghent",
    Country = Country.Belgium,
    ZipCode = "9000",
    OrganizationId = organization.Id,
    ExperienceId = experience.Id,
});

// Update person
person.Email = "john.updated@example.com";
person = api.People.Update(person);

// Delete person
api.People.Delete(person.Id);

// Delete multiple people
api.People.Delete(new[] { id1, id2, id3 });
```

### Teams

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Create a team
var team = api.Teams.Create(new Team
{
    Name = "Broadcast Crew",
    Email = "broadcast@example.com",
    Description = "Main broadcast production team",
});

// Add a skill to the team
team.AddSkill(skill);
team = api.Teams.Update(team);

// Update team
team.Description = "Updated description";
team = api.Teams.Update(team);

// Delete team
api.Teams.Delete(team.Id);
```

### Organizations

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Create an organization
var organization = api.Organizations.Create(new Organization
{
    Name = "Skyline Communications",
    CategoryId = category.Id,
});

// Update organization
organization.Name = "Updated Name";
organization = api.Organizations.Update(organization);

// Delete organization
api.Organizations.Delete(organization.Id);
```

### Skills

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Create a skill
var skill = api.Skills.Create(new Skill { Name = "Camera Operation" });

// Create or update multiple skills in one operation
var skills = api.Skills.CreateOrUpdate(new[]
{
    new Skill { Name = "Camera Operation" },
    new Skill { Name = "Lighting" },
    new Skill { Name = "Audio Mixing" },
});

// Update a skill
skill.Name = "Camera Operation (Advanced)";
skill = api.Skills.Update(skill);

// Delete a skill
api.Skills.Delete(skill.Id);
```

### Roles

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Create a role
var role = api.Roles.Create(new Role { Name = "Camera Operator" });

// Update a role
role.Name = "Senior Camera Operator";
role = api.Roles.Update(role);

// Delete a role
api.Roles.Delete(role.Id);
```

### Experience

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Create an experience level
var experience = api.Experience.Create(new Experience { Name = "Senior" });

// Update an experience level
experience.Name = "Lead";
experience = api.Experience.Update(experience);

// Delete an experience level
api.Experience.Delete(experience.Id);
```

### Categories

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Create a category
var category = api.Categories.Create(new Category { Name = "Broadcaster" });

// Update a category
category.Name = "Media Company";
category = api.Categories.Update(category);

// Delete a category
api.Categories.Delete(category.Id);
```

## Person Skills

### Assign Skills to a Person

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

var person = api.People.Read(personId);

// Add a single skill
person.AddSkill(skill);

// Replace all skills
person.SetSkills(new[] { skill1, skill2 });

// Remove a skill
person.RemoveSkill(skill);

person = api.People.Update(person);
```

## Team Skills

### Assign Skills to a Team

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

var team = api.Teams.Read(teamId);

// Add a single skill
team.AddSkill(skill);

// Replace all skills
team.SetSkills(new[] { skill1, skill2 });

// Remove a skill
team.RemoveSkill(skill);

team = api.Teams.Update(team);
```

## Team Memberships

### Manage Team Memberships on a Person

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

var person = api.People.Read(personId);

// Add person to a team (optionally with a role)
person.AddTeamMembership(new TeamMembership(team));
person.AddTeamMembership(new TeamMembership(team) { RoleId = role.Id });

// Remove person from a team
var membership = person.TeamMemberships.First(tm => tm.TeamId == team.Id);
person.RemoveTeamMembership(membership);

person = api.People.Update(person);
```
