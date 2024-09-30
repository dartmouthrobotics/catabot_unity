using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RainProbTest : MonoBehaviour {
    static float dropSizeMin = 0.5f;
    static float dropSizeMax = 6;
    static float dropSizeDistributionRightHalf = -4.1f * ((dropSizeMax - dropSizeMin) / 2f);

    void Start() {
        //// Raindrop size and average num drops in 50 meters at various rates of rain test
        //for (int i = 0; i <= 50; i++) { // Rate of rain
        //    float dropDistribution = DropSizeDistribution(i);
        //    print(i + " - Drop Size Distribution: " + dropDistribution);
        //    // 0   - 0
        //    // 1   - 0.1014892
        //    // ... - Increasing by small amounts
        //    // 50  - 56.19884
        //    // Summary, no rain below 1, and the distribution in a cubic meter is fairly small even at a rate of 50.

        //    float avgNumDrops = dropDistribution * Mathf.PI * Mathf.Pow(0.02f, 2f) * 50f;
        //    print(i + " - Average Num Drops in 50 meters: " + avgNumDrops);
        //    // 0   - 0
        //    // 1   - 0.006376755
        //    // ... - Increasing by small amounts
        //    // 50  - 3.531078
        //    // Summary, no rain below 1, and the number of drops in the beam is reasonable.
        //}

        // Actual number of drops hitting the beam test
        float rateOfRain = 50;
        for (int i = 0; i <= 200; i++) { // Distance
            float dropDistribution = DropSizeDistribution(rateOfRain);
            float avgNumDrops = dropDistribution * Mathf.PI * Mathf.Pow(0.02f, 2f) * (float)i;
            float dropsFloor = ProbNumRaindrops(avgNumDrops, (int)Mathf.Floor(avgNumDrops));
            float dropsCeil = ProbNumRaindrops(avgNumDrops, (int)Mathf.Ceil(avgNumDrops));
            if (dropsCeil > dropsFloor) {
                print(i + " - Drops Floor: " + dropsFloor + ", Drops Ceil: " + dropsCeil);
            }
        }
        // rateOfRain 0 - all 0s
        // rateOfRain 1
        // - distance
        // - 0  - 0
        // - 1  - Drops Floor: 0.9998724, Drops Ceil: 0.0001275188
        // - 50 - Drops Floor: 0.9936435, Drops Ceil: 0.006336221
        // rateOfRain 50
        // - distance
        // - 0  - 0
        // - 1  - Drops Floor: 0.9318145, Drops Ceil: 0.06580618
        // - 50 - Drops Floor: 0.2148043, Drops Ceil: 0.1896227
        // rateOfRain 193
        // - 50 - Drops Floor: 0.1143669, Drops Ceil: 0.3406632

        // Conclusions -
        // At a max range of 50 meters,
        //      no need to call ProbNumRaindrops unless the rateOfRain is greater than 193 mm/hr
        // At a max rateOfRain of 50 mm/hr,
        //      no need to call ProbNumRaindrops unless the distance is greater than 169 m
    }

    public float DropSizeDistribution(float rate = 2) {
        return 8000f * Mathf.Exp(dropSizeDistributionRightHalf * Mathf.Pow(rate, -0.21f));
    }

    public float ProbNumRaindrops(float avgNumDrops, int numDrops) {
        if (avgNumDrops == 0) {
            return 0;
        } else {
            return Mathf.Exp(-avgNumDrops) * (Mathf.Pow(avgNumDrops, (float)numDrops) / (float)Factorial(numDrops));
        }
    }

    public static int Factorial(int k) {
        int count = k;
        int factorial = 1;
        while (count >= 1) {
            factorial = factorial * count;
            count--;
        }
        return factorial;
    }
}
