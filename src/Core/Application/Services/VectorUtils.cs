using System.Text.Json;

namespace Application.Services;

public sealed class VectorUtils
{
        public static float CosineSimilarity(IReadOnlyList<float> a, IReadOnlyList<float> b)
    {
        if (a.Count != b.Count) throw new InvalidOperationException("Vector length mismatch");
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Count; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }
        return (float)(dot / (Math.Sqrt(na) * Math.Sqrt(nb) + 1e-12));
    }

    public static IReadOnlyList<float> ParseJsonVector(string json)
    {
        var arr = JsonSerializer.Deserialize<List<float>>(json) ?? new List<float>();
        return arr;
    }
}
