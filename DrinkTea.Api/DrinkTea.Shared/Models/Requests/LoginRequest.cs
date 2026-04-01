using System;
using System.Collections.Generic;
using System.Text;

namespace DrinkTea.Shared.Models.Requests
{
    public record LoginRequest(string Username, string Password);
}
