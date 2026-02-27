namespace Application.Exceptions
{
    public class BadRequestException : ArgumentException
    {
        public IList<KeyValuePair<string, object>> FormValidationError { get; set; }

        public BadRequestException(string message, IList<KeyValuePair<string, object>>? formValidationError = null)
            : base(message)
        {
            FormValidationError = formValidationError ?? new List<KeyValuePair<string, object>>();
        }
    }
}
