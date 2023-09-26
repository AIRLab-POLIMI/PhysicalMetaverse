

public static class MathHelper
{
    public static int Remainder(int i, int n) => (i % n + n) % n;

    public static float MapRange(float x, float minIn, float maxIn, float minOut, float maxOut) =>
        minOut + ((maxOut - minOut) / (maxIn - minIn)) * (x - minIn);
}
