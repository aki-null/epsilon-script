namespace EpsilonScript.Intermediate
{
  public interface IElementReader
  {
    void Push(Element element);
    void End();
  }
}