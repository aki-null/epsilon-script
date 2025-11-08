# Unit Test Guidelines

## The Golden Rule: Test What You Can See

Don't write tests that claim to verify things you can't actually measure. If you're testing a cache, you need to see cache invalidation happen (like getting a different result after adding a function overload), not just run the same thing 1000 times and assume it's hitting the cache.

Bad example:
```csharp
[Fact]
public void Cache_RepeatedExecutions_AlwaysHit()
{
    for (int i = 0; i < 1000; i++)
    {
        functionNode.Execute(null);
        Assert.Equal(10, functionNode.IntegerValue);
    }
}
```

This just proves the function returns 10. We have no idea if caching is involved.

Good example:
```csharp
[Fact]
public void Invalidation_AfterOverloadAdd_NewOverloadSelected()
{
    compiler.AddCustomFunction(CustomFunction.Create("test", (int x) => x * 2));
    compiler.AddCustomFunction(CustomFunction.Create("test", (float x) => x * 10.0f));

    var script = compiler.Compile("test(5.5)");
    script.Execute();

    Assert.Equal(55.0f, script.FloatValue); // Different result = cache was invalidated
}
```

Now we can see something changed - the function picked the float overload instead of the int one.

## Unit Tests Are Not Benchmarks

We're testing correctness, not speed. If you want to know if something is fast, write a benchmark. Unit tests should verify:

- Invalidation prevents stale data
- Correct behavior when state changes
- Type system works right
- Edge cases don't break things

Skip testing:
- Cache hit rates
- Execution speed
- Memory allocations (unless you're specifically testing NoAlloc mode)

## Name Your Tests Honestly

Test names should say what they actually verify. Use `Component_Scenario_ExpectedBehavior` format.

Good:
- `VersionTracking_AfterOverloadAdd_VersionIncrements`
- `TypeFallback_IntegerToFloat_CorrectOverloadSelected`

Bad:
- `Cache_AlwaysHit` (you can't verify this without instrumentation)
- `TestFunction` (what does this even test?)

## Don't Test Unsafe Patterns

If the README says something is unsafe, don't write tests for it. We don't want tests that validate unsafe usage.

Wrong:
```csharp
public void Threading_SharedScript_NoCorruption()
{
    var script = compiler.Compile("test(5)");
    Parallel.For(0, 50, i => script.Execute()); // UNSAFE per README
}
```

Right:
```csharp
public void Threading_PerThreadInstances_NoCorruption()
{
    Parallel.For(0, 50, i =>
    {
        var compiler = new Compiler(); // Each thread gets its own instance
        var script = compiler.Compile("test(5)");
        script.Execute();
    });
}
```

Threading model: each thread needs its own Compiler, CompiledScript, and DictionaryVariableContainer.

## Avoid Duplication

Before writing a new test, ask yourself:
1. Is this already tested somewhere else?
2. Does my test actually add new coverage?
3. Am I testing the right component?

Where tests should live:
- Basic function execution → `ScriptFunctionTests`
- Function overload selection → `ScriptFunctionTests`
- Contextual functions → `ScriptContextualFunctionTests`
- Cache invalidation → `FunctionOverloadCacheTests`
- Variable resolution → `ScriptVariableTests`

## Keep Tests Focused

Unit tests should verify one thing. If your test needs a ton of setup for unrelated components, it probably belongs in integration tests.

Too broad:
```csharp
public void EntireSystem_WithCaching_WorksCorrectly()
{
    // This is testing everything at once
    var compiler = new Compiler();
    compiler.AddCustomFunction(...);
    var script = compiler.Compile("complex expression");
    script.Execute(variables);
    Assert.Equal(expectedResult, script.IntegerValue);
}
```

Better:
```csharp
public void VersionTracking_AfterOverloadAdd_VersionIncrements()
{
    var overload = new CustomFunctionOverload(...);
    var initialVersion = overload.Version;

    overload.Add(newFunction);

    Assert.Equal(initialVersion + 1, overload.Version);
}
```

## Comment Your Tests

Use Setup/Action/Verify comments so it's obvious what you're testing:

```csharp
[Fact]
public void VersionTracking_AfterOverloadAdd_VersionIncrements()
{
    // Setup: Create overload with initial version
    var overload = new CustomFunctionOverload(...);
    var initialVersion = overload.Version;

    // Action: Add new overload
    overload.Add(newFunction);

    // Verify: Version incremented
    Assert.Equal(initialVersion + 1, overload.Version);
}
```

Use `#region` blocks to group related tests:

```csharp
public class FunctionOverloadCacheTests
{
    #region Version Tracking & Invalidation
    // ...
    #endregion

    #region Dynamic Type Resolution
    // ...
    #endregion

    #region Edge Cases
    // ...
    #endregion
}
```

## Red Flags

Stop and think if your test:
- Claims something "always" happens without actually measuring it
- Mentions caching but doesn't test invalidation
- Needs tons of setup for unrelated stuff
- Repeats the same assertion 1000+ times
- Uses patterns marked UNSAFE in the docs
- Has the same name as a test in another file
- Can't be explained in one sentence