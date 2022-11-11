namespace EpsilonScript.Intermediate
{
  internal interface IElementReader
  {
    void Push(Element element);
    void End();
  }
}