using System;

namespace EventBus.Message.Events
{
    public class IntegrationEvent
    {
        // public virtual Guid Id { get; } = Guid.NewGuid();

        public DateTime CreationDate { get; } = DateTime.UtcNow;

        public IntegrationEvent()
        {

        }

        public IntegrationEvent(DateTime creationDate)
        {
            // Id = id;
            CreationDate = creationDate;
        }
    }
}
