namespace EpsilonScript
{
  public interface IVariableContainer
  {
    bool TryGetValue(uint variableKey, out VariableValue variableValue);
  }
}