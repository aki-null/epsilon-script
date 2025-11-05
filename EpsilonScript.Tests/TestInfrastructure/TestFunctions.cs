using EpsilonScript.Function;

namespace EpsilonScript.Tests.TestInfrastructure
{
  public static class TestFunctions
  {
    public sealed class CallCounter
    {
      public int Count { get; private set; }

      public void Increment()
      {
        ++Count;
      }
    }

    public static (CustomFunction Function, CallCounter Counter) CreateBooleanProbe(string name,
      bool isConstant = false)
    {
      var counter = new CallCounter();
      var function = CustomFunction.Create(name, (bool value) =>
      {
        counter.Increment();
        return value;
      }, isConstant);
      return (function, counter);
    }

    public static (CustomFunction Function, CallCounter Counter) CreateIntegerProbe(string name,
      bool isConstant = false)
    {
      var counter = new CallCounter();
      var function = CustomFunction.Create(name, (int value) =>
      {
        counter.Increment();
        return value;
      }, isConstant);
      return (function, counter);
    }

    public static (CustomFunction Function, CallCounter Counter) CreateFloatProbe(string name,
      bool isConstant = false)
    {
      var counter = new CallCounter();
      var function = CustomFunction.Create(name, (float value) =>
      {
        counter.Increment();
        return value;
      }, isConstant);
      return (function, counter);
    }

    public static (CustomFunction Function, CallCounter Counter) CreateTupleProbe(string name,
      bool isConstant = false)
    {
      var counter = new CallCounter();
      var function = CustomFunction.Create(name, (float a, float b, float c) =>
      {
        counter.Increment();
        return a + b + c;
      }, isConstant);
      return (function, counter);
    }
  }
}