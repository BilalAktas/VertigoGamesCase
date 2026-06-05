using UnityEngine;

namespace Core
{
    public class Helpers
    {
        public static int GetWeightedIndex(RewardData[] datas)
        {
            var total = 0f;

            for (var i = 0; i < datas.Length; i++)
                total += datas[i].Weight;

            var rand = Random.value * total;

            var current = 0f;

            for (var i = 0; i < datas.Length; i++)
            {
                current += datas[i].Weight;

                if (rand <= current)
                    return i;
            }

            return datas.Length - 1;
        }
    }
}