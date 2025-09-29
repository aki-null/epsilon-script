using EpsilonScript.AST;

namespace EpsilonScript
{
  public class CompiledScript
  {
    private readonly Node _rootNode;
    private bool _isResultCached = false;
    public Type ValueType { get; private set; } = Type.Undefined;
    public bool IsConstant { get; }

    public int IntegerValue => _rootNode.IntegerValue;
    public float FloatValue => _rootNode.FloatValue;
    public bool BooleanValue => _rootNode.BooleanValue;
    public string StringValue => _rootNode.StringValue;

    public CompiledScript(Node rootNode)
    {
      _rootNode = rootNode;
      // Constness is cached, because this information is gathered from each AST node O(n)
      IsConstant = rootNode.IsConstant;
    }

    public void Execute(IVariableContainer variablesOverride = null)
    {
      // Do not execute this again if the script is completely constant, and it has been executed at least once.
      if (IsConstant && _isResultCached)
      {
        return;
      }

      _rootNode.Execute(variablesOverride);
      switch (_rootNode.ValueType)
      {
        case AST.ValueType.Integer:
          ValueType = Type.Integer;
          break;
        case AST.ValueType.Float:
          ValueType = Type.Float;
          break;
        case AST.ValueType.Boolean:
          ValueType = Type.Boolean;
          break;
        case AST.ValueType.String:
          ValueType = Type.String;
          break;
        default:
          throw new RuntimeException("AST root node returned invalid value type");
      }

      _isResultCached = true;
    }
  }
}