using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.Dtos.Finance
{
    public class DepositRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумма должна быть больше 0")]
        public decimal Amount { get; set; }
    }
}
