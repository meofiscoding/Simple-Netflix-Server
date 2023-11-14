using System;

namespace EventBus.Message.Events
{
    public class MembershipActivatedEvent : IntegrationEvent
    {
        public string UserId { get; set; }
    }
}
