namespace Application.Interfaces;

public interface ITextSplitter
{
    IReadOnlyList<string> Split(string text);
}
