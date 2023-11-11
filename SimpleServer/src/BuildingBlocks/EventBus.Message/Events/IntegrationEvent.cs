using System;

namespace EventBus.Message.Events
{
    public class IntegrationEvent
    {
        public Guid Id { get; private set; }

        public DateTime CreationDate { get; private set; }
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        public IntegrationEvent(Guid id, DateTime creationDate)
        {
            Id = id;
            CreationDate = creationDate;
        }
    }
}
