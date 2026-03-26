using Microsoft.AspNetCore.Mvc;

namespace GoldenCrown.API.Dtos;

public class BalanceRequest
{
    [FromQuery] public string Currency { get; set; }   
}