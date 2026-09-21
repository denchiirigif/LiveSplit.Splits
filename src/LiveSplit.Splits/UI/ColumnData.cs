using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace LiveSplit.UI;

public class ColumnData
{
    public string Name { get; set; }
    public ColumnType Type { get; set; }
    public string Comparison { get; set; }
    public string TimingMethod { get; set; }

    // 段階色オプション
    public bool UseTierColors { get; set; }

    // 上限秒数（新記録 < 0 は固定）。例: 10,20,30
    public double[] TierThresholds { get; set; } = [10, 20, 30];

    // 色は5つ: [0]新記録, [1]<=T1, [2]<=T2, [3]<=T3, [4]それ以上
    public Color[] TierColors { get; set; } =
    [
        Color.FromArgb(0xFF, 0xD7, 0x00), // 黄
        Color.FromArgb(0x3A, 0x8F, 0xFF), // 青
        Color.FromArgb(0x40, 0xE0, 0xF0), // 水色
        Color.FromArgb(0xFF, 0x8A, 0xC8), // ピンク
        Color.FromArgb(0xFF, 0x40, 0x40), // 赤
    ];

    public ColumnData(string name, ColumnType type, string comparison, string method)
    {
        Name = name;
        Type = type;
        Comparison = comparison;
        TimingMethod = method;
    }

    /// <summary>差分から段階色を返す。無効・差分なしのときはnull（通常の色を使う）</summary>
    public Color? GetTierColor(TimeSpan? delta)
    {
        if (!UseTierColors || delta == null)
        {
            return null;
        }

        double seconds = delta.Value.TotalSeconds;
        if (seconds < 0)
        {
            return TierColors[0];
        }

        for (int i = 0; i < TierThresholds.Length; i++)
        {
            if (seconds <= TierThresholds[i])
            {
                return TierColors[i + 1];
            }
        }

        return TierColors[TierColors.Length - 1];
    }

    public static ColumnData FromXml(XmlNode node)
    {
        var element = (XmlElement)node;
        var data = new ColumnData(element["Name"].InnerText,
            (ColumnType)Enum.Parse(typeof(ColumnType), element["Type"].InnerText),
            element["Comparison"].InnerText,
            element["TimingMethod"].InnerText);

        // 古いレイアウト（項目なし）でも壊れないよう、あれば読む
        try
        {
            if (element["UseTierColors"] != null)
            {
                data.UseTierColors = bool.Parse(element["UseTierColors"].InnerText);
            }

            if (element["TierThresholds"] != null)
            {
                double[] thresholds = element["TierThresholds"].InnerText
                    .Split(',')
                    .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                    .ToArray();
                if (thresholds.Length == 3)
                {
                    data.TierThresholds = thresholds;
                }
            }

            if (element["TierColors"] != null)
            {
                Color[] colors = element["TierColors"].InnerText
                    .Split(',')
                    .Select(s => Color.FromArgb(int.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture)))
                    .ToArray();
                if (colors.Length == 5)
                {
                    data.TierColors = colors;
                }
            }
        }
        catch (FormatException)
        {
            // 壊れた値は初期値のまま使う
        }

        return data;
    }

    public int CreateElement(XmlDocument document, XmlElement element)
    {
        string thresholds = string.Join(",", TierThresholds.Select(t => t.ToString(CultureInfo.InvariantCulture)));
        string colors = string.Join(",", TierColors.Select(c => c.ToArgb().ToString("X8", CultureInfo.InvariantCulture)));

        return SettingsHelper.CreateSetting(document, element, "Version", "1.5") ^
        SettingsHelper.CreateSetting(document, element, "Name", Name) ^
        SettingsHelper.CreateSetting(document, element, "Type", Type) ^
        SettingsHelper.CreateSetting(document, element, "Comparison", Comparison) ^
        SettingsHelper.CreateSetting(document, element, "TimingMethod", TimingMethod) ^
        SettingsHelper.CreateSetting(document, element, "UseTierColors", UseTierColors.ToString()) ^
        SettingsHelper.CreateSetting(document, element, "TierThresholds", thresholds) ^
        SettingsHelper.CreateSetting(document, element, "TierColors", colors);
    }
}
