namespace ProjectSilverSquad
{
	internal static class MathUtils
	{
		internal static float NormalizationCustom(float value, float min, float max, float minNormalizedValue, float maxNormalizedValue)
		{
			return (Normalization01(value, min, max) * (maxNormalizedValue - minNormalizedValue)) + minNormalizedValue;
		}


		internal static float Normalization01(float value, float min, float max)
		{
			return (value - min) / (max - min);
		}
	}
}
