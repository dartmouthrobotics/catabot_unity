using UnityEngine;
public class LidarMath
{
    public static float MaxLidarDistance = 10f;

    float lambda = 0.2f;

    static float backscatterCoefficient90Percent = 0.9f / Mathf.PI;

    static float threshold = 0.000005f;

    static float dropSizeMin = 0.5f;
    static float dropSizeMax = 6;

    static float extinctionEfficiency = 2;
    static float backscatterEfficiency = 2;

    static float extinctionCrossSectionMean = extinctionEfficiency * ((Mathf.PI * Mathf.Pow(((dropSizeMin + dropSizeMax) / 2f), 2f)) / 4f);
    static float backscatterCrossSectionMean = backscatterEfficiency * ((Mathf.PI * Mathf.Pow(((dropSizeMin + dropSizeMax) / 2f), 2f)) / 4f);

    public double ProbabilityMassFunction(int k)
    {
        //exp(-1*lambda) lambda^x / x!
        int kFactorial = Factorial(k);
        double p_dist = Mathf.Exp(-1 * lambda) * Mathf.Pow(lambda, kFactorial) / kFactorial;        
        return p_dist;
    }

    public static int Factorial(int k)
    {
        int count = k;
        int factorial = 1;
        while (count >= 1)
        {
            factorial = factorial * count;
            count--;
        }
        return factorial;
    }
    //Vbeami = π ∗ rbeami^2 ∗ Z/k
    public static float Beam(float radius, float dist, int k)
    {
        int num_sections = 10;
        float Beam_vol = 0;
        for (int i = 1; i <= num_sections; i++)
        {
            Beam_vol += (Mathf.PI * Mathf.Pow(radius, 2.0f) * dist / k);
        }
        return Beam_vol;
    }
    /// <summary>
    /// N (D) = 8000 · exp(−4.1 · R−0.21 · D)
    /// Returns the average number of raindrops in a unit area (cubic meter?)
    /// </summary>
    /// <param name="rate">How much rain will fall in mm / hr</param>
    /// <param name="dropSizeMin">Min size of the average raindrop distribution in mm</param>
    /// <param name="dropSizeMax">Max size of the average raindrop distribution in mm</param>
    /// <returns></returns>
    public static float DropSizeDistribution(float rate = 2)
    {

        float numDrops = 8000 * Mathf.Exp(-4.1f * Mathf.Pow(rate, -0.21f) * ((dropSizeMax - dropSizeMin) / 2f));
        return numDrops;
    }

    public static float ProbNumRaindrops(float avgNumDrops, int numDrops)
    {
        return Mathf.Exp(-avgNumDrops) * (Mathf.Pow(avgNumDrops, (float) numDrops) / (float) Factorial(numDrops));
    }

    public static float RelativePower(float distance, float backscatterCoefficient, float scatteringCoefficient) {
        return (backscatterCoefficient * Mathf.Exp(-2f * scatteringCoefficient * distance)) / Mathf.Pow(distance, 2);
    }

    public static float MinRandomValue(int numRands) {
        float smallestVal = 1f;
        for (int j = 0; j < Mathf.Min(numRands, 50f); j++) {
            float value = Random.value;
            if (value < smallestVal) {
                smallestVal = value;
            }
        }
        return smallestVal;
    }

    public static float CalculateLidarDistanceWithRain(float distance, bool hit, float dropDistribution, float beamVolumeOneMeter) {
        if(hit) { // Lidar actually hit something (assuming the rain doesn't get in the way)
            float avgNumDrops = dropDistribution * beamVolumeOneMeter * distance;
            int numRaindrops = (ProbNumRaindrops(avgNumDrops, (int)Mathf.Floor(avgNumDrops)) > ProbNumRaindrops(avgNumDrops, (int)Mathf.Ceil(avgNumDrops))) ? (int)Mathf.Floor(avgNumDrops) : (int)Mathf.Ceil(avgNumDrops);
            //Debug.Log("AvgNumDrops: " + avgNumDrops);
            //Debug.Log("Num Drops: " + numRaindrops);

            if (numRaindrops < 1) {
                return distance;
            } else {
                float coefficientBase = numRaindrops / beamVolumeOneMeter;
                float extinctionCoefficientFromRain = coefficientBase * extinctionCrossSectionMean / distance;
                float rainDistance = distance * MinRandomValue(numRaindrops);
                float rainBackscatterCoefficient = coefficientBase * backscatterCrossSectionMean / rainDistance;

                float RIO = RelativePower(distance, backscatterCoefficient90Percent, extinctionCoefficientFromRain);
                float RIRD = RelativePower(rainDistance, rainBackscatterCoefficient, 0);
                //Debug.Log("RIO: " + RIO + ", RIRD: " + RIRD);
                if(RIO > RIRD) { // Returned Intesity of Object is greater than RI of Rain Drops
                    return RIO > threshold ? distance : 0;
                } else { // Rain Drops beat the actual Object
                    return RIRD > threshold ? rainDistance : 0;
                }
            }
        } else { // Lidar did not hit something within the max distance
            float avgNumDrops = dropDistribution * beamVolumeOneMeter * MaxLidarDistance;
            int numRaindrops = (ProbNumRaindrops(avgNumDrops, (int)Mathf.Floor(avgNumDrops)) > ProbNumRaindrops(avgNumDrops, (int)Mathf.Ceil(avgNumDrops))) ? (int)Mathf.Floor(avgNumDrops) : (int)Mathf.Ceil(avgNumDrops);
            //Debug.Log("AvgNumDrops: " + avgNumDrops);
            //Debug.Log("Num Drops: " + numRaindrops);
            float coefficientBase = numRaindrops / beamVolumeOneMeter;
            float rainDistance = MaxLidarDistance * MinRandomValue(numRaindrops);
            float rainBackscatterCoefficient = coefficientBase * backscatterCrossSectionMean / rainDistance;
            float RIRD = RelativePower(rainDistance, rainBackscatterCoefficient, 0);
            //Debug.Log("RIRD: " + RIRD);
            return RIRD > threshold ? rainDistance : 0;
        }
    }
}
