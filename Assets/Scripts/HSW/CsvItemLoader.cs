using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class CsvItemLoader
{
    private const string CsvResourcePath = "Tb_ItemTable";      // Assets/Resources/Tb_ItemTable.csv
    private const string SpriteRoot = "Images/Items/";          // ✅ 변경됨: Assets/Resources/Images/Items/...

    static readonly Regex CsvSplitRegex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

    public static List<ItemCsvRow> LoadDefault(char intArraySeparator = '|')
    {
        var ta = Resources.Load<TextAsset>(CsvResourcePath);
        if (ta == null)
        {
            Debug.LogError($"[CsvItemLoader] Resources에서 '{CsvResourcePath}.csv' 를 찾을 수 없습니다. (경로: Assets/Resources/{CsvResourcePath}.csv)");
            return new List<ItemCsvRow>();
        }
        return Parse(ta.text, intArraySeparator);
    }

    private static List<ItemCsvRow> Parse(string csvText, char intArraySeparator)
    {
        var result = new List<ItemCsvRow>();
        if (string.IsNullOrWhiteSpace(csvText)) return result;

        var lines = csvText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
        if (lines.Length == 0) return result;

        int lineIndex = 0;
        var headers = SafeSplit(lines[lineIndex++]).Select(Normalize).ToList();

        int hIndex       = headers.IndexOf("index");
        int hName        = headers.IndexOf("itemname");
        int hSprite      = headers.IndexOf("itemsprite");
        int hDes         = headers.IndexOf("itemdes");
        int hCure        = headers.IndexOf("curefloor");
        int hAppear      = headers.IndexOf("appearfloor");

        var spriteCache = new Dictionary<string, Sprite>();

        for (; lineIndex < lines.Length; lineIndex++)
        {
            var raw = lines[lineIndex];
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var cols = SafeSplit(raw);
            while (cols.Count < headers.Count) cols.Add(string.Empty);

            try
            {
                var row = new ItemCsvRow
                {
                    index       = ParseInt(cols[hIndex]),
                    itemName    = Unquote(cols[hName]),
                    itemDes     = Unquote(cols[hDes]),
                    cureFloor   = ParseIntArray(Unquote(cols[hCure]), intArraySeparator),
                    appearFloor = ParseIntArray(Unquote(cols[hAppear]), intArraySeparator),
                };

                string spriteCol = Unquote(cols[hSprite]);    // ex: "Potion" 또는 "Weapons/Sword.png"
                string trimmed   = TrimExtension(spriteCol);   // 확장자 제거
                string resPath   = SpriteRoot + trimmed;       // ✅ "Images/Items/Potion" 형태

                if (!spriteCache.TryGetValue(resPath, out var sp))
                {
                    sp = Resources.Load<Sprite>(resPath);
                    if (sp == null)
                        Debug.LogWarning($"[CsvItemLoader] Sprite 로드 실패: '{resPath}' (Assets/Resources/{resPath}.png 확인)");
                    spriteCache[resPath] = sp;
                }
                row.itemSprite = sp;

                result.Add(row);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CsvItemLoader] {lineIndex + 1}행 파싱 실패: {e.Message}\n원본: {raw}");
            }
        }

        return result;
    }

    // --- 헬퍼 ---
    private static List<string> SafeSplit(string line)
        => CsvSplitRegex.Split(line).Select(Unquote).ToList();

    private static string Unquote(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        s = s.Trim();
        if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"')
            s = s.Substring(1, s.Length - 2);
        return s.Replace("\"\"", "\"");
    }

    private static string Normalize(string h) => Unquote(h).Trim().ToLowerInvariant();

    private static int ParseInt(string s, int def = 0)
        => int.TryParse(s?.Trim(), out var v) ? v : def;

    private static int[] ParseIntArray(string s, char sep)
    {
        if (string.IsNullOrWhiteSpace(s)) return Array.Empty<int>();
        return s.Split(sep)
                .Select(t => int.TryParse(t.Trim(), out var v) ? v : (int?)null)
                .Where(v => v.HasValue)
                .Select(v => v.Value)
                .ToArray();
    }

    private static string TrimExtension(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        int dot = path.LastIndexOf('.');
        return (dot >= 0) ? path.Substring(0, dot) : path;
    }
}
