namespace EpsilonScript.Intermediate
{
  public interface ITokenReader
  {
    void Push(Token token);
    void End();
  }
}