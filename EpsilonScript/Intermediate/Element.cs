namespace EpsilonScript.Intermediate
{
  public readonly struct Element
  {
    public Token Token { get; }
    public ElementType Type { get; }

    public Element(Token token, ElementType type)
    {
      Token = token;
      Type = type;
    }

    public override string ToString()
    {
      return $"{Token.ToString()}: {Type}";
    }
  }
}