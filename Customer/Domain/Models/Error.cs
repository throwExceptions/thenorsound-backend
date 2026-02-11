using Domain.Enums;

namespace Domain.Models
{
    public class Error
    {
        public ErrorType Type{ get; set; }
        public string ErrorMessage { get; set; }
        public IList<KeyValuePair<string, object>> FormValidationError { get; set; }
    }
}
