namespace EpsilonScript
{
  public interface IVariableContainer
  {
    bool TryGetValue(VariableId variableKey, out VariableValue variableValue);
  }
}