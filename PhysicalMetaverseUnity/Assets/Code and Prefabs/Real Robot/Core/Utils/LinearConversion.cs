
public static class LinearConversion
{
    public static int LinearConversionInt(int oldValue, int oldMin, int oldMax, int newMin, int newMax)
    {
        return (((oldValue - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
    }

    public static float LinearConversionFloat(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
    {
        return (((oldValue - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
    }
}

