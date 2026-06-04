using AutoMapper;
using FluentValidation;
using GoldenCrown.Api.Attributes;
using GoldenCrown.Api.Dtos;
using GoldenCrown.Api.Dtos.Finance;
using GoldenCrown.Application;
using GoldenCrown.Application.Dtos.Finance;
using GoldenCrown.Application.Features.Finance.Deposit;
using GoldenCrown.Application.Features.Finance.GetBalance;
using GoldenCrown.Application.Features.Finance.GetTransactionHistory;
using GoldenCrown.Application.Features.Finance.Transfer;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GoldenCrown.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MyAuthorize]
    public class FinanceController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public FinanceController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
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

            var transferResult = await _mediator.Send(new TransferCommand(GetUserid(), request.ReceiverLogin, request.Amount, request.Currency, request.ReceiverCurrency));
            if (transferResult.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(new { Message = transferResult.ErrorMessage });
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<TransactionHistoryResponse>>> GetTransactionHistoryAsync(
            [FromQuery] TransactionHistoryRequest request, 
            IValidator<TransactionHistoryRequest> validator)
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            Result<List<TransactionHistoryDto>> historyResult = await _mediator.Send(new GetTransactionHistoryQuery(GetUserid(), request.From, request.To, request.Offset, request.Limit));
            if (historyResult.IsSuccess)
            {
                IEnumerable<TransactionHistoryResponse> response = historyResult.Value!.Select(dto => _mapper.Map<TransactionHistoryResponse>(dto));
                return Ok(response);    
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