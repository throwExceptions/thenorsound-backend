namespace Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public string EntityName { get; set; }
        public string EntityId { get; set; }

        public NotFoundException(string message, string entityName = "", string entityId = "")
            : base(message)
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }
}