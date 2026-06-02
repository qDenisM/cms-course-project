using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Таблица: N, КПД (%), скорость маховика (°/с), интенсивность пара (%).
/// Имена ячеек в Hierarchy: KPD1..3, SPD1..3, INT1..3, AVG или KPDavg.
/// </summary>
public class ResultsTableVal : MonoBehaviour
{
    public InputField textInput;
    public SteamEngineSimulator engine;
    public SteamIntensityController intensity;

    [SerializeField] Text kpd1;
    [SerializeField] Text kpd2;
    [SerializeField] Text kpd3;
    [SerializeField] Text spd1;
    [SerializeField] Text spd2;
    [SerializeField] Text spd3;
    [SerializeField] Text int1;
    [SerializeField] Text int2;
    [SerializeField] Text int3;
    [SerializeField] Text kpdAverage;

    const string Empty = "-";

    /// <summary>Запись: если в поле ввода число — в КПД; иначе снимок с симулятора во все три колонки строки.</summary>
    public void WriteValue()
    {
        Text kpd = NextEmpty(kpd1, kpd2, kpd3);
        Text spd = NextEmpty(spd1, spd2, spd3);
        Text intr = NextEmpty(int1, int2, int3);
        if (kpd == null)
            return;

        float manualKpd;
        if (!string.IsNullOrEmpty(textInput != null ? textInput.text : null) &&
            float.TryParse(textInput.text.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out manualKpd))
        {
            kpd.text = manualKpd.ToString("0.##");
        }
        else if (intensity != null)
            kpd.text = intensity.GetEstimatedEfficiency().ToString("0.#");

        if (spd != null && engine != null)
            spd.text = Mathf.Abs(engine.AngularVelocityDeg).ToString("0.#");

        if (intr != null && intensity != null)
            intr.text = intensity.IntensityPercent > 0 ? intensity.IntensityPercent.ToString() : Empty;

        if (textInput != null)
            textInput.text = "";

        UpdateAverage();
    }

    public void Clean()
    {
        SetAll(Empty);
        if (textInput != null)
            textInput.text = "";
        UpdateAverage();
    }

    void SetAll(string v)
    {
        if (kpd1) kpd1.text = v;
        if (kpd2) kpd2.text = v;
        if (kpd3) kpd3.text = v;
        if (spd1) spd1.text = v;
        if (spd2) spd2.text = v;
        if (spd3) spd3.text = v;
        if (int1) int1.text = v;
        if (int2) int2.text = v;
        if (int3) int3.text = v;
        if (kpdAverage) kpdAverage.text = v;
    }

    static Text NextEmpty(Text a, Text b, Text c)
    {
        if (a != null && a.text == Empty) return a;
        if (b != null && b.text == Empty) return b;
        if (c != null && c.text == Empty) return c;
        return null;
    }

    void UpdateAverage()
    {
        if (kpdAverage == null)
            return;
        float sum = 0f;
        int n = 0;
        TryAdd(kpd1, ref sum, ref n);
        TryAdd(kpd2, ref sum, ref n);
        TryAdd(kpd3, ref sum, ref n);
        kpdAverage.text = n > 0 ? (sum / n).ToString("0.#") : Empty;
    }

    static void TryAdd(Text cell, ref float sum, ref int n)
    {
        if (cell == null || cell.text == Empty)
            return;
        float v;
        if (float.TryParse(cell.text.Replace(',', '.'), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out v))
        {
            sum += v;
            n++;
        }
    }
}
