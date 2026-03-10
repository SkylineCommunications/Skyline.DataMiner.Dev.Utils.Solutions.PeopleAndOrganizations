# ApiObjectValidator Generic ID Type - Usage Guide

## Overview
The `ApiObjectValidator` classes have been adapted to support generic ID types (string, Guid, or any other type) while maintaining full backwards compatibility with existing code.

## Class Hierarchy

```
ApiObjectValidatorBase<TId>                          // Base class with generic ID support
├── ApiObjectValidator                                // Non-generic, defaults to Guid (backwards compatible)
├── ApiObjectValidator<T, TId>                        // Generic with type T and ID type TId
│   └── ApiObjectValidator<T>                         // Defaults to Guid for TId (backwards compatible)
│       └── DomInstanceApiObjectValidator<T>          // For DOM instances (backwards compatible)
│       └── StringObjectValidator<T>                  // For string-based IDs (new!)
└── DomInstanceApiObjectValidator<T, TId>             // Generic DOM validator (new!)
    └── DomInstanceApiObjectValidator<T>              // Defaults to Guid (backwards compatible)
```

## Usage Examples

### 1. Existing Code (Backwards Compatible)
All existing code continues to work without changes:

```csharp
// Example with DomInstanceApiObjectValidator (uses Guid by default)
internal class DomRoleHandler : DomInstanceApiObjectValidator<DomRole>
{
    // Works exactly as before
    private void CreateOrUpdate(ICollection<Role> apiRoles)
    {
        var validRoles = apiRoles.Where(IsValid).ToList();
        // ...
        ReportSuccess(domRole);
    }
}
```

### 2. String-Based IDs (New Capability)
Use `StringObjectValidator<T>` for objects identified by strings:

```csharp
internal class SkillHandler : StringObjectValidator<Skill>
{
    private SkillHandler(PeopleAndOrganizationsApi api) 
        : base(skill => skill.Name)  // Extract string ID from skill
    {
        this.api = api ?? throw new ArgumentNullException(nameof(api));
    }

    internal static bool TryCreateOrUpdate(
        PeopleAndOrganizationsApi api, 
        ICollection<Skill> apiSkills, 
        out StringBulkOperationResult<Skill> result)
    {
        var handler = new SkillHandler(api);
        handler.CreateOrUpdate(apiSkills);

        // Access string IDs and successful items
        result = new StringBulkOperationResult<Skill>(
            handler.SuccessfulItems,      // IReadOnlyCollection<Skill>
            handler.SuccessfulIds,         // IReadOnlyCollection<string>
            handler.UnsuccessfulItems,     // IReadOnlyCollection<string>
            handler.TraceDataPerItem);     // IReadOnlyDictionary<string, ...>

        return !result.HasFailures;
    }

    private void CreateOrUpdate(ICollection<Skill> apiSkills)
    {
        // ... your logic here ...

        // Report success for processed skills
        ReportSuccess(apiSkills);
    }
}
```

### 3. Custom ID Types
You can create validators for any custom ID type:

```csharp
// Example: Int-based IDs
internal class CustomObjectValidator<T> : ApiObjectValidator<T, int>
{
    private readonly List<int> successfulIds = new List<int>();
    private readonly Func<T, int> idExtractor;

    internal override IReadOnlyCollection<int> SuccessfulIds => successfulIds;

    public CustomObjectValidator(Func<T, int> idExtractor)
    {
        this.idExtractor = idExtractor ?? throw new ArgumentNullException(nameof(idExtractor));
    }

    protected override void ReportSuccess(T item)
    {
        var id = idExtractor(item);
        if (unsuccessfulItems.Contains(id))
        {
            throw new InvalidOperationException(
                $"An item cannot be marked as both successful and unsuccessful");
        }

        successfulIds.Add(id);
        successfulItems.Add(item);
    }
}
```

### 4. Generic DOM Validator with Custom ID
If you need DOM instance validation with a custom ID type:

```csharp
internal class CustomDomValidator<T> : DomInstanceApiObjectValidator<T, string> 
    where T : DomInstanceBase
{
    internal CustomDomValidator() 
        : base(item => item.SomeStringProperty)  // Extract string ID
    {
    }
}
```

## Key Methods

### Available Methods in All Validators:

| Method | Parameters | Returns | Description |
|--------|-----------|---------|-------------|
| `IsValid(TId id)` | ID of type TId | bool | Checks if an item with given ID is valid |
| `ReportSuccess(T item)` | Single item | void | Mark item as successfully processed |
| `ReportSuccess(IEnumerable<T> items)` | Multiple items | void | Mark multiple items as successful |
| `ReportError(TId key)` | ID and optional error data | void | Mark item as failed |
| `PassTraceData(...)` | Internal validator | void | Merge error/success data from another validator |

### Properties:

| Property | Type | Description |
|----------|------|-------------|
| `SuccessfulIds` | `IReadOnlyCollection<TId>` | IDs of successful items |
| `SuccessfulItems` | `IReadOnlyCollection<T>` | Successfully processed items |
| `UnsuccessfulItems` | `IReadOnlyCollection<TId>` | IDs of failed items |
| `TraceDataPerItem` | `IReadOnlyDictionary<TId, ...>` | Error data per ID |

## Migration Guide

### Converting Existing Handler to String-Based IDs:

**Before:**
```csharp
internal class MyHandler
{
    private readonly List<string> successfulIds = new List<string>();
    private readonly List<string> unsuccessfulIds = new List<string>();
    private readonly Dictionary<string, ...> traceDataPerItem = new Dictionary<string, ...>();
    
    internal static bool TryProcess(...)
    {
        result = new StringBulkOperationResult(
            handler.successfulIds, 
            handler.unsuccessfulIds, 
            handler.traceDataPerItem);
    }
}
```

**After:**
```csharp
internal class MyHandler : StringObjectValidator<MyType>
{
    internal MyHandler() : base(item => item.StringId) { }
    
    internal static bool TryProcess(...)
    {
        result = new StringBulkOperationResult<MyType>(
            handler.SuccessfulItems,    // Now includes the actual items!
            handler.SuccessfulIds, 
            handler.UnsuccessfulItems, 
            handler.TraceDataPerItem);
    }
}
```

## Benefits

1. **Type Safety**: Compile-time checking of ID types
2. **Backwards Compatible**: All existing code continues to work
3. **Flexible**: Support for Guid, string, int, or any custom ID type
4. **Consistent**: Same pattern across all validators
5. **Testable**: Easier to mock and test with strongly-typed IDs

## Notes

- The default ID type for backwards compatibility is `Guid`
- `StringObjectValidator<T>` requires an ID extractor function in the constructor
- All validators inherit from `ApiObjectValidatorBase<TId>` which provides core functionality
- The `IsValid` method is overloaded in backwards-compatible classes to accept `IIdentifiable` objects
