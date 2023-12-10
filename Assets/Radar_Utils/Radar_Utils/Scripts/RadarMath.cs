using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarMath 
{
    private float sigma;
    private float mu;
    public static Vector3 GetVectorFromAngle(float angle) {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static Vector3 GetRadarVectorFromAngle(float angle) {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));
    }

    public static float NextGaussian()
    {
        float v1, v2, s;
        do
        {
            v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);
        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

        return v1 * s;
    }

    //public static float NextGaussian(float mean, float standard_deviation, float min, float max)
    //{
    //    float x;
    //    do
    //    {
    //        x = NextGaussian(mean, standard_deviation);
    //    } while (x < min || x > max);
    //    retun x;
    //}
    public static float generateNormalRandom(float mu, float sigma)
    {
        float rand1 = Random.Range(0.0f, 1.0f);
        float rand2 = Random.Range(0.0f, 1.0f);

        float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

        return (mu + sigma * n);
    }

    public static float GaussianMath(float mu, float sigma)
    {
        float x1, x2, w, y1; //, y2;

        do
        {
            x1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            x2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            w = x1 * x1 + x2 * x2;
        } while (Mathf.Abs(w) >= 1f);

        w = Mathf.Sqrt((-2f * Mathf.Log(w)) / w);
        y1 = x1 * w;
        // y2 = x2 * w;
        //Debug.Log((y1 * sigma) + mu);
        return (y1 * sigma) + mu;
    }
}
