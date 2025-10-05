using System;
using System.Collections.Generic;
using System.IO;

namespace Underground_Psychosis.GameEngine
{
    public static class LevelFileLoader
    {
        // Loads rectangular CSV (any row length allowed; final width = widest row, missing cells = 0)
        public static int[,] LoadRectangular(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Level file not found", filePath);

            var lines = File.ReadAllLines(filePath);
            int rows = lines.Length;
            int width = 0;

            var parsed = new List<int[]>();
            foreach (var line in lines)
            {
                var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                width = Math.Max(width, parts.Length);
                var row = new int[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                    row[i] = int.TryParse(parts[i], out var v) ? v : 0;
                parsed.Add(row);
            }

            var result = new int[rows, width];
            for (int y = 0; y < rows; y++)
            {
                var row = parsed[y];
                for (int x = 0; x < row.Length; x++)
                    result[y, x] = row[x];
                // remaining cells default 0
            }
            return result;
        }

        // Loads dynamic map (jagged) as list<int[]>
        public static List<int[]> LoadJagged(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Level file not found", filePath);

            var lines = File.ReadAllLines(filePath);
            var list = new List<int[]>(lines.Length);

            foreach (var line in lines)
            {
                var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var row = new int[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                    row[i] = int.TryParse(parts[i], out var v) ? v : 0;
                list.Add(row);
            }
            return list;
        }
    }
}