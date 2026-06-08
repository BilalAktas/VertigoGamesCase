using UnityEngine;

namespace Core
{
    public class Helpers
    {
        public static int GetWeightedIndex(RewardData[] datas, int state)
        {
            var total = 0f;

            foreach (var data in datas)
                total += state == 0 ? data.PlacementWeight : data.SelectionWeight;

            var rand = Random.value * total;
            var current = 0f;

            var i = 0;
            foreach (var data in datas)
            {
                current += state == 0 ? data.PlacementWeight : data.SelectionWeight;

                if (rand <= current)
                    return i;

                i++;
            }

            return datas.Length - 1;
        }

        private static string[] _suffix = { "", "K", "M", "B", "T", "Q", "QU", "S" };

        public static string ConvertToKBM(float value)
        {
            if (value < 1000)
                return value.ToString();

            var count = 0;
            while (value >= 1000f)
            {
                count++;
                value /= 1000f;
            }

            return value < .01f && value != 0 ? $"{value:0.000}{_suffix[count]}" : $"{value:F}{_suffix[count]}";
        }
    }
}