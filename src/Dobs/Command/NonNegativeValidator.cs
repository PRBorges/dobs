using CliFx.Extensibility;

namespace Dobs;

public class NonNegativeValidator : BindingValidator<int>
{
    public override BindingValidationError? Validate(int value) =>
        value < 0 ? Error("Decimals to display can not be negative.") : Ok();
}
