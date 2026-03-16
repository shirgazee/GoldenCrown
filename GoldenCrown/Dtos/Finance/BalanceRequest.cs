using Microsoft.AspNetCore.Mvc;

namespace GoldenCrown.Dtos.Finance;

public class BalanceRequest
{
    [FromQuery] public string Currency { get; set; }   
}