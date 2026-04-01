using DrinkTea.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DrinkTea.Shared.Models.Requests
{
    public record StartBrewingDto(Guid TeaId, decimal Grams, List<Guid> VisitIds);

    /// <summary>
    /// 	DTO для добавления участника.
    /// </summary>
    public record JoinSessionDto(Guid VisitId);
    public record RegisterUserRequest(string FullName, string Login, string Password, UserRoles Role);
    public record TopUpRequest(decimal Amount, PaymentMethod Method);
    public record CheckInRequest(Guid? UserId, string? Note);

    public record CheckoutRequest(decimal InternalAmount, decimal ExternalAmount, PaymentMethod Method);

}
