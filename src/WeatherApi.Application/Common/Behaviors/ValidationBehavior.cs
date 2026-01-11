using FluentValidation;
using MediatR;
using System.Reflection;
using WeatherApi.Domain.Common.Result;

namespace WeatherApi.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            var errors = failures.Select(f => Error.Validation(
                f.PropertyName,
                f.ErrorMessage)).ToList();

            var firstError = errors.First();

            // === FIX: Robust Reflection to handle Ambiguous Methods ===

            // Case 1: Handle Generic Result<T>
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse).GetGenericArguments()[0];

                // FIX: Explicitly find the GENERIC Failure method to avoid ambiguity
                var failureMethod = typeof(Result)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == nameof(Result.Failure) &&
                                m.IsGenericMethod &&
                                m.GetParameters().Length == 1)
                    .First();

                var genericMethod = failureMethod.MakeGenericMethod(resultType);

                return (TResponse)genericMethod.Invoke(null, new object[] { firstError })!;
            }

            // Case 2: Handle Non-Generic Result
            if (typeof(TResponse) == typeof(Result))
            {
                // No reflection needed here, direct cast works
                return (TResponse)(object)Result.Failure(firstError);
            }
        }

        return await next();
    }
}


//using FluentValidation;
//using MediatR;
//using WeatherApi.Domain.Common.Result;

//namespace WeatherApi.Application.Common.Behaviors;

//public class ValidationBehavior<TRequest, TResponse>
//    : IPipelineBehavior<TRequest, TResponse>
//    where TRequest : notnull
//{
//    private readonly IEnumerable<IValidator<TRequest>> _validators;

//    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
//    {
//        _validators = validators;
//    }

//    public async Task<TResponse> Handle(
//        TRequest request,
//        RequestHandlerDelegate<TResponse> next,
//        CancellationToken ct)
//    {
//        if (!_validators.Any())
//            return await next();

//        var context = new ValidationContext<TRequest>(request);

//        var validationResults = await Task.WhenAll(
//            _validators.Select(v => v.ValidateAsync(context, ct)));

//        var failures = validationResults
//            .SelectMany(r => r.Errors)
//            .Where(f => f != null)
//            .ToList();

//        if (failures.Any())
//        {
//            var errors = failures.Select(f => Error.Validation(
//                f.PropertyName,
//                f.ErrorMessage)).ToList();

//            // Return first error as Result pattern
//            if (typeof(TResponse).IsGenericType &&
//                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
//            {
//                var resultType = typeof(TResponse).GetGenericArguments()[0];
//                var failureMethod = typeof(Result)
//                    .GetMethod(nameof(Result.Failure))!
//                    .MakeGenericMethod(resultType);

//                return (TResponse)failureMethod.Invoke(null, new object[] { errors.First() })!;
//            }

//            if (typeof(TResponse) == typeof(Result))
//            {
//                return (TResponse)(object)Result.Failure(errors.First());
//            }
//        }

//        return await next();
//    }
//}