using FluentValidation;
using FluentValidation.Results;

namespace DynamoLeagueBlazor.Shared.Helpers;

/// <summary>
///     A total hack used for asynchronous validation with Blazor applications because of the lack of first-class support with EditForm.
///     See <see href="https://github.com/Blazored/FluentValidation/issues/38"/> for one example of this - but is present on all FluentValidation + Blazor libraries.
///     <para>
///         TODO: Revisit this periodically. Hopefully .NET 7?
///     </para>
/// </summary>
public abstract class AsyncAbstractValidator<T> : AbstractValidator<T>
{
    private Task<ValidationResult> _validateTask = null!;

    public override Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
        => _validateTask = base.ValidateAsync(context, cancellation);

    public override ValidationResult Validate(ValidationContext<T> context)
    {
        var result = base.Validate(context);
        _validateTask = Task.FromResult(result);
        return result;
    }

    public Task<ValidationResult> WaitForValidateAsync()
         => _validateTask ?? Task.FromResult<ValidationResult>(null!);

}