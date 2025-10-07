namespace EpsilonScript.Intermediate
{
  internal interface ITokenReader
  {
    void Push(Token token);
    void End();
  }
}