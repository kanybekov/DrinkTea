using DrinkTea.Shared.Enums;
using System;
using System.Collections.Generic;

namespace DrinkTea.Shared.Models.Requests
{
    public class StartBrewingDto
    {
        public Guid TeaId { get; set; }
        public decimal Grams { get; set; }
        public List<Guid> VisitIds { get; set; } = new();
    }

    public class JoinSessionDto
    {
        public Guid VisitId { get; set; }
    }

    public class RegisterUserRequest
    {
        public string FullName { get; set; } = "";
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public UserRoles Role { get; set; }
    }

    public class TopUpRequest
    {
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
    }

    public class CheckInRequest
    {
        public Guid? UserId { get; set; }
        public string? Note { get; set; }
    }

    public class CheckoutRequest
    {
        public decimal InternalAmount { get; set; }
        public decimal ExternalAmount { get; set; }
        public PaymentMethod Method { get; set; }
    }
}
