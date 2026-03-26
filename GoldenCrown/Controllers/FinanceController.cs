using FluentValidation;
using GoldenCrown.API.Attributes;
using GoldenCrown.API.Dtos;
using GoldenCrown.Application.Dtos.Finance;
using GoldenCrown.Application.Features.Finance.Deposit;
using GoldenCrown.Application.Features.Finance.GetBalance;
using GoldenCrown.Application.Features.Finance.GetTransactionHistory;
using GoldenCrown.Application.Features.Finance.Transfer;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GoldenCrown.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MyAuthorize]
    public class FinanceController : Controller
    {
        private readonly IMediator _mediator;

        public FinanceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalanceAsync(BalanceRequest request, IValidator<BalanceRequest> validator)
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }
            var balanceResult = await _mediator.Send(new GetBalanceQuery(GetUserid(), request.Currency));

            if (balanceResult.IsSuccess)
            {
                return Ok(new BalanceResponse
                {
                    Balance = balanceResult.Value
                });
            }

            return BadRequest(new { Message = balanceResult.ErrorMessage });
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> DepositAsync([FromBody] DepositRequest request, IValidator<DepositRequest> validator)
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var result = await _mediator.Send(new DepositCommand(GetUserid(), request.Amount, request.Currency));
            if (result.IsSuccess)
            {
                return Ok();
            }
            return BadRequest(new { Message = result.ErrorMessage });
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferAsync([FromBody] TransferRequest request, IValidator<TransferRequest> validator)
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var transferResult = await _mediator.Send(new TransferCommand(GetUserid(), request.ReceiverLogin, request.Amount, request.Currency));
            if (transferResult.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(new { Message = transferResult.ErrorMessage });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetTransactionHistoryAsync([FromQuery] TransactionHistoryRequest request, IValidator<TransactionHistoryRequest> validator)
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var historyResult = await _mediator.Send(new GetTransactionHistoryQuery(GetUserid(), request.From, request.To, request.Offset, request.Limit));
            if (historyResult.IsSuccess)
            {
                return Ok(historyResult.Value);
            }
            return BadRequest(new { Message = historyResult.ErrorMessage });
        }

        internal int GetUserid()
        {
            var userId = HttpContext.Items[Constants.UserIdContextParameter] as int?;
            return userId!.Value;
        }
    }
}