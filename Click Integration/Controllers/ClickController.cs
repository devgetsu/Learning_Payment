using Click_Integration.Configurations;
using Click_Integration.Entities;
using Click_Integration.Models.Complate;
using Click_Integration.Models.Prepare;
using Click_Integration.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            IConfiguration configuration,
            ITelegramService telegramService)
        {
            _context = context;
            _clickConfig = configuration.GetSection("ClickConfig").Get<ClickConfig>()!;
        }

        [HttpGet("Transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var transactions = _context.ClickTransactions.ToList();
            return Ok(transactions);
        }

        [HttpPost("prepare")]
        public async Task<IActionResult> Prepare()
        {
            var form = Request.Form;
            var clickTransId = form["click_trans_id"];
            var serviceId = form["service_id"];
            var clickPaydocId = form["click_paydoc_id"];
            var merchantTransId = form["merchant_trans_id"];
            var amount = form["amount"];
            var action = form["action"];
            var signTime = form["sign_time"];
            var error = form["error"];
            var errorNote = form["error_note"];
            var signString = form["sign_string"];

            var generatedSignString = GenerateSignString(
                                long.Parse(clickTransId),
                                int.Parse(serviceId),
                                _clickConfig.SecretKey,
                                merchantTransId,
                                decimal.Parse(amount),
                                int.Parse(action),
                                signTime);

            if (signString != generatedSignString)
                return BadRequest(new { error = -1, error_note = "Invalid sign_string" });

            if (merchantTransId != "1")
                return BadRequest(new { error = -6, error_note = "The transaction is not found (check parameter merchant_prepare_id)" });

            var clickTransaction = new ClickTransaction
            {
                ClickTransId = long.Parse(clickTransId),
                MerchantTransId = merchantTransId,
                Amount = decimal.Parse(amount),
                SignTime = signTime,
                Status = EOrderPaymentStatus.Pending,
            };

            _context.ClickTransactions.Add(clickTransaction);
            _context.SaveChanges();

            var response = new PrepareResponse()
            {
                ClickTransId = clickTransaction.ClickTransId,
                MerchantTransId = clickTransaction.MerchantTransId,
                MerchantPrepareId = 1,
                Error = 0,
                ErrorNote = "Payment prepared successfully"
            };

            return Ok(response);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> Complete()
        {
            var form = Request.Form;

            var clickTransId = long.Parse(form["click_trans_id"]);
            var serviceId = int.Parse(form["service_id"]);
            var clickPaydocId = long.Parse(form["click_paydoc_id"]);
            var merchantTransId = form["merchant_trans_id"];
            var merchantPrepareId = int.Parse(form["merchant_prepare_id"]);
            var amount = decimal.Parse(form["amount"]);
            var action = int.Parse(form["action"]);
            var error = int.Parse(form["error"]);
            var errorNote = form["error_note"];
            var signTime = form["sign_time"];
            var signString = form["sign_string"];


            var generatedSignString = GenerateSignString(
                clickTransId,
                serviceId,
                _clickConfig.SecretKey,
                merchantTransId,
                merchantPrepareId,
                amount,
                action,
                signTime);


            if (signString != generatedSignString)
                return BadRequest(new { error = -1, error_note = "Invalid sign_string" });

            if (merchantTransId != "1")
                return BadRequest(new { error = -9, error_note = "The transaction is not found (check parameter merchant_prepare_id)" });

            var clickTransaction = _context.ClickTransactions.FirstOrDefault(c => c.ClickTransId == clickTransId);
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

        private string GenerateSignString(long clickTransId, int serviceId, string secretKey, string merchantTransId, decimal amount, int action, string signTime)
        {
            var signString = $"{clickTransId}{serviceId}{secretKey}{merchantTransId}{amount}{action}{signTime}";
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(signString);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
        private string GenerateSignString(long clickTransId, int serviceId, string secretKey, string merchantTransId, int merchantPrepareId, decimal amount, int action, string signTime)
        {
            var signString = $"{clickTransId}{serviceId}{secretKey}{merchantTransId}{merchantPrepareId}{amount}{action}{signTime}";

            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(signString);
                var hashBytes = md5.ComputeHash(inputBytes);

                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

    }
}
