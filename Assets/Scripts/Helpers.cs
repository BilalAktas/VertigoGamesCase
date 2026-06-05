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
    }
}