namespace Application.Exceptions
{
    public class FormValidationException : ArgumentException
    {
        public IList<KeyValuePair<string, object>> FormValidationError { get; set; }

        public FormValidationException(string message, IList<KeyValuePair<string, object>> formValidationError)
            : base(message)
        {
            FormValidationError = formValidationError ?? new List<KeyValuePair<string, object>>();
        }
    }
}
