using API.DTOs.Response;
using Application.Exceptions;
using Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Authentication;
using Error = Domain.Models.Error;

namespace API.Controllers
{
    public class BaseController<T> : ControllerBase
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> TryExecuteAsync<TRequest, TResult>(
                TRequest request,
                IValidator<TRequest> validator,
                Func<Task<TResult>> function,
                ILogger<T> logger)
                where TResult : IActionResult
        {
            try
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.ToDictionary()
                        .SelectMany(kvp => kvp.Value.Select(msg => new KeyValuePair<string, object>(kvp.Key, msg)))
                        .ToList();

                    throw new FormValidationException("Validation failed.", errors);
                }

                return await function();
            }
            catch (NotFoundException ex)
            {
                logger.LogInformation(
                    "Not found exception in {name}. Entity: {entityName}, ID: {entityId}",
                    typeof(T).Name,
                    ex.EntityName,
                    ex.EntityId);
                return NotFoundResponse(ex);
            }
            catch (ValidationException ex5)
            {
                logger.LogInformation("Validation exception in {name}. {ex}", typeof(T).Name, ex5.ToString());
                return ErrorResponse(ex5, ErrorType.ValidationError, HttpStatusCode.BadRequest, new List<KeyValuePair<string, object>>());
            }
            catch (HttpRequestException ex)
            {
                logger.LogError("Exception in " + typeof(T).Name + ".", ex);
                return ErrorResponse(ex, ErrorType.HttpClientError, HttpStatusCode.InternalServerError, new List<KeyValuePair<string, object>>());
            }
            catch (DuplicateException ex2)
            {
                logger.LogInformation("Duplicate exception in {name}. {ex}", typeof(T).Name, ex2.ToString());
                return ErrorResponse(ex2, ErrorType.DuplicateError, HttpStatusCode.Conflict, ex2.FormValidationError);
            }
            catch (BadRequestException ex3)
            {
                logger.LogInformation("Bad request exception in {name}. {ex}", typeof(T).Name, ex3.ToString());
                return ErrorResponse(ex3, ErrorType.ArgumentError, HttpStatusCode.BadRequest, ex3.FormValidationError);
            }
            catch (FormValidationException ex4)
            {
                logger.LogInformation("Bad request exception in {name}. {ex}", typeof(T).Name, ex4.ToString());
                return ErrorResponse(ex4, ErrorType.ValidationError, HttpStatusCode.BadRequest, ex4.FormValidationError);
            }
            catch (AuthenticationException ex6)
            {
                logger.LogInformation("Authentication exception in {name}. {ex}", typeof(T).Name, ex6.ToString());
                return ErrorResponse(ex6, ErrorType.AuthenticationFailed, HttpStatusCode.InternalServerError, new List<KeyValuePair<string, object>>());
            }
            catch (Exception ex7)
            {
                logger.LogError("Exception in " + typeof(T).Name + ".", ex7);
                return ErrorResponse(ex7, ErrorType.UnknownError, HttpStatusCode.InternalServerError, new List<KeyValuePair<string, object>>());
            }
        }

        private IActionResult ErrorResponse(Exception ex, ErrorType errorType, HttpStatusCode statusCode, IList<KeyValuePair<string, object>> formValidationError)
        {
            return StatusCode((int)statusCode, new BaseResponseDto<Error>
            {
                Error = new Error
                {
                    Type = errorType,
                    ErrorMessage = ex.Message,
                    FormValidationError = formValidationError,
                },
            });
        }

        private IActionResult NotFoundResponse(NotFoundException ex)
        {
            return NotFound(new BaseResponseDto<object>
            {
                Result = null,
                Error = new Error
                {
                    Type = ErrorType.NotFoundError,
                    ErrorMessage = ex.Message,
                    FormValidationError = new List<KeyValuePair<string, object>>()
                }
            });
        }
    }
}

public static class ValidationResultExtensions
{
    public static IDictionary<string, string[]> ToDictionary(this ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );
    }
}
