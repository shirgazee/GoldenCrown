using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.Dtos.Finance
{
    public class TransactionHistoryRequest
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Значение limit должно быть не меньше 1")]
        public int Limit { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Значение offset не может быть отрицательным")]
        public int Offset { get; set; }
    }
}
