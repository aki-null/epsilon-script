namespace EpsilonScript
{
  public interface IVariableContainer
  {
    bool TryGetValue(int variableKey, out VariableValue variableValue);
  }
}