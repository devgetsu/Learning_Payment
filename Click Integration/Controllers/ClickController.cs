using Click_Integration.Configurations;
using Click_Integration.Entities;
using Click_Integration.Models.Complate;
using Click_Integration.Models.Prepare;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Click_Integration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClickController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ClickConfig _clickConfig;

        public ClickController(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _clickConfig = configuration.GetSection("ClickConfig").Get<ClickConfig>();
        }

        [HttpGet("Transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var transactions = _context.ClickTransactions.ToList();
            return Ok(transactions);
        }

        [HttpPost("prepare")]
        public async Task<IActionResult> Prepare(PrepareRequest prepareRequest)
        {
            var generatedSignString = GenerateSignString(
                                prepareRequest.ClickTransId,
                                prepareRequest.ServiceId,
                                _clickConfig.SecretKey,
                                prepareRequest.MerchantTransId,
                                prepareRequest.Amount,
                                prepareRequest.Action,
                                prepareRequest.SignTime);

            if (prepareRequest.SignString != generatedSignString)
                return BadRequest(new { error = -1, error_note = "Invalid sign_string" });

            if (prepareRequest.MerchantTransId != "1")
                return BadRequest(new { error = -6, error_note = "The transaction is not found (check parameter merchant_prepare_id)" });

            var clickTransaction = new ClickTransaction
            {
                ClickTransId = prepareRequest.ClickTransId,
                MerchantTransId = prepareRequest.MerchantTransId,
                Amount = prepareRequest.Amount,
                SignTime = DateTime.Parse(prepareRequest.SignTime),
                Status = EOrderPaymentStatus.Pending,
            };

            _context.ClickTransactions.Add(clickTransaction);
            _context.SaveChanges();

            var response = new PrepareResponse()
            {
                ClickTransId = clickTransaction.ClickTransId,
                MerchantTransId = clickTransaction.MerchantTransId,
                MerchantPrepareId = 1, //OrderId ni bervorelikchi
                Error = 0,
                ErrorNote = "Payment prepared successfully"
            };

            return Ok(response);
        }

        [HttpPost("complate")]
        public async Task<IActionResult> Complete(CompleteRequest completeRequest)
        {
            var generatedSignString = GenerateSignString(
                                completeRequest.ClickTransId,
                                completeRequest.ServiceId,
                                _clickConfig.SecretKey,
                                completeRequest.MerchantTransId,
                                completeRequest.MerchantPrepareId,
                                completeRequest.Amount,
                                completeRequest.Action,
                                completeRequest.SignTime);

            if (completeRequest.SignString != generatedSignString)
                return BadRequest(new { error = -1, error_note = "Invalid sign_string" });

            if (completeRequest.MerchantTransId != "1")
                return BadRequest(new { error = -6, error_note = "The transaction is not found (check parameter merchant_prepare_id)" });

            var clickTransaction = _context.ClickTransactions.FirstOrDefault(c => c.ClickTransId == completeRequest.ClickTransId);
            if (clickTransaction != null)
                clickTransaction.Status = EOrderPaymentStatus.Paid;

            _context.SaveChanges();

            return Ok(new CompleteResponse()
            {
                ClickTransId = clickTransaction.ClickTransId,
                MerchantTransId = clickTransaction.MerchantTransId,
                MerchantConfirmId = clickTransaction.Id,
                Error = 0,
                ErrorNote = "Payment Success"
            });
        }

        [HttpGet("generate-click-link")]
        public async Task<IActionResult> GenereteClickUrl(int orderId, decimal amount)
        {
            var clickBaseUrl = "https://my.click.uz/services/pay";
            var returnUrl = "https://www.urphacapital.uz/courses";

            StringBuilder clickUrl = new StringBuilder(clickBaseUrl);
            clickUrl.Append("?service_id=" + _clickConfig.ServiceId);
            clickUrl.Append("&merchant_id=" + _clickConfig.MerchantId);
            clickUrl.Append("&amount=" + amount);
            clickUrl.Append("&transaction_param=" + orderId);
            clickUrl.Append("&return_url=" + returnUrl);

            return Ok(clickUrl.ToString());
        }

        private string GenerateSignString(params object[] parameters)
        {
            var input = string.Join("", parameters);
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input + _clickConfig.SecretKey);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
