public interface IGrowthValue
{
	float BaseValue { get; }

	float Derivative { get; }

	float SecondDerivative { get; }
}
