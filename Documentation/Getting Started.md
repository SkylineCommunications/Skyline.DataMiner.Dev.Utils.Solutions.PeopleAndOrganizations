# Getting Started

This documentation describes how to use the public API exposed by `Skyline.DataMiner.Solutions.PeopleAndOrganizations`.

## Installation

Add the NuGet package to your solution:

```bash
dotnet add package Skyline.DataMiner.Dev.Utils.Solutions.PeopleAndOrganizations
```

Depending on your project type, one of the following additional packages is also required:

- Automation scripts: `Skyline.DataMiner.Dev.Utils.Solutions.PeopleAndOrganizations.Automation`
- Protocols: `Skyline.DataMiner.Dev.Utils.Solutions.PeopleAndOrganizations.Protocol`
- GQI Ad-hoc data sources and custom operators: `Skyline.DataMiner.Dev.Utils.Solutions.PeopleAndOrganizations.GQI`

> [!NOTE]
> This library targets `.NET Framework 4.8`.

## Entry Point

The `PeopleAndOrganizationsApi` class is the main entry point to the People and Organizations API.

It exposes:

- **Repositories** for reading/writing DOM-backed objects (Organizations, People, Teams, Experience, Categories, Roles, Skills)
- **State management** for transitioning people, teams, and organizations through lifecycle states
- **Logging** for custom logging integration

### Obtaining an API Instance

To obtain an instance of the `PeopleAndOrganizationsApi` class, use the `GetPeopleAndOrganizationsApi` extension method.
This extension method is available for automation scripts, connectors, and GQI ad-hoc data sources.

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Automation;
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Protocol;
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.GQI;

// Automation scripts
var api = engine.GetPeopleAndOrganizationsApi();

// Protocols
var api = protocol.GetPeopleAndOrganizationsApi();

// GQI ad-hoc data sources and custom operators
var api = dms.GetPeopleAndOrganizationsApi();
```

On other places the instance can also be created starting from an `IConnection` object:

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

IConnection connection;
var api = connection.GetPeopleAndOrganizationsApi();
```

## Core Concepts

### People

People represent individuals that can be managed and scheduled within the People and Organizations solution.

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

var person = api.People.Create(new Person
{
    Name = "John Doe",
    Email = "john.doe@example.com",
    Phone = "+1 555 0100",
});
```

### Teams

Teams group people together and can be made bookable so that they can be reserved or scheduled.

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

var team = api.Teams.Create(new Team
{
    Name = "Broadcast Crew",
    Description = "Main broadcast production team",
});
```

### Organizations

Organizations represent companies or departments that people belong to.

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

var organization = api.Organizations.Create(new Organization
{
    Name = "Skyline Communications",
});
```

### Skills

Skills define capabilities that can be assigned to people and teams.

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

var skill = api.Skills.Create(new Skill { Name = "Camera Operation" });
```

### Roles

Roles define the function a person has within a team.

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

var role = api.Roles.Create(new Role { Name = "Camera Operator" });
```

### Experience

Experience levels describe the seniority or expertise of a person.

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

var experience = api.Experience.Create(new Experience { Name = "Senior" });
```

### Categories

Categories classify organizations.

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

var category = api.Categories.Create(new Category { Name = "Broadcaster" });
```

## Basic Usage

Once you have an instance of the `PeopleAndOrganizationsApi` class, you can start using its features.

### Creating Objects

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Create a category
var category = api.Categories.Create(new Category { Name = "Broadcaster" });

// Create an organization assigned to the category
var organization = api.Organizations.Create(new Organization
{
    Name = "Skyline Communications",
    CategoryId = category.Id,
});

// Create skill and experience definitions
var skill = api.Skills.Create(new Skill { Name = "Camera Operation" });
var experience = api.Experience.Create(new Experience { Name = "Senior" });
var role = api.Roles.Create(new Role { Name = "Camera Operator" });

// Create a person
var person = api.People.Create(new Person
{
    Name = "John Doe",
    Email = "john.doe@example.com",
    OrganizationId = organization.Id,
    ExperienceId = experience.Id,
});

// Assign a skill to the person
person.AddSkill(skill);
api.People.Update(person);

// Create a team
var team = api.Teams.Create(new Team
{
    Name = "Broadcast Crew",
    Description = "Main broadcast production team",
});

// Add the person to the team with a role
person.AddTeamMembership(new TeamMembership(team) { RoleId = role.Id });
api.People.Update(person);
```

### Reading Objects

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Read all people
var people = api.People.Read();

// Read by ID
var person = api.People.Read(personId);

// Read all teams
var teams = api.Teams.Read();

// Read all organizations
var organizations = api.Organizations.Read();
```

### Updating Objects

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Update a person's email
person.Email = "new.email@example.com";
api.People.Update(person);

// Add a skill to a person
person.AddSkill(skill);
api.People.Update(person);
```

### Deleting Objects

```csharp
using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

// Delete a person
api.People.Delete(person.Id);

// Delete multiple people
api.People.Delete(new[] { id1, id2, id3 });
```

## Next Steps

- **[Quick Reference](Quick%20Reference.md)** – Common snippets for repositories, querying, and object management
- **[Advanced Topics](Advanced%20Topics.md)** – State management, logging, and installation checks
