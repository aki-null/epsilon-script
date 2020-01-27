using EpsilonScript.AST;
using EpsilonScript.Parser;

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

    public CompiledScript(Node rootNode)
    {
      _rootNode = rootNode;
      // Constness is cached, because this information is gathered from each AST node O(n)
      IsConstant = rootNode.IsConstant;
    }

    public void Execute()
    {
      // Do not execute this again if the script is completely constant, and it has been executed at least once.
      if (IsConstant && _isResultCached)
      {
        return;
      }

      _rootNode.Execute();
      ValueType = _rootNode.ValueType switch
      {
        AST.ValueType.Integer => Type.Integer,
        AST.ValueType.Float => Type.Float,
        AST.ValueType.Boolean => Type.Boolean,
        _ => throw new RuntimeException("AST root node returned invalid value type")
      };

      _isResultCached = true;
    }
  }
}