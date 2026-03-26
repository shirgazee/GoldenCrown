using Microsoft.AspNetCore.Mvc;

namespace GoldenCrown.Api.Dtos.Finance;

public class BalanceRequest
{
    [FromQuery] public string Currency { get; set; }   
}