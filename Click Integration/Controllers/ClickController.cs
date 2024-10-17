using Click_Integration.Configurations;
using Click_Integration.Entities;
using Click_Integration.Models.Complate;
using Click_Integration.Models.Prepare;
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
        private readonly ITelegramService _telegramService;
        public ClickController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ITelegramService telegramService)
        {
            _context = context;
            _clickConfig = configuration.GetSection("ClickConfig").Get<ClickConfig>();
            _telegramService = telegramService;
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
            var form = Request.Form;  // Request.Form orqali to'g'ridan-to'g'ri olish
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

            // Sign stringni yaratish va tekshirish
            var generatedSignString = GenerateSignString(
                                long.Parse(clickTransId),
                                int.Parse(serviceId),
                                _clickConfig.SecretKey,
                                merchantTransId,
                                decimal.Parse(amount),
                                int.Parse(action),
                                signTime);

            await _telegramService.SendMessage(generatedSignString);
            
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





        [HttpPost("complate")]
        public async Task<IActionResult> Complete([FromForm] CompleteRequest completeRequest)
        {
            try
            {
                Console.WriteLine(HttpContext.Request.ToString());
            }
            catch (Exception ex)
            {
            }
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
            try
            {
                Console.WriteLine(HttpContext.Request.ToString());
            }
            catch (Exception ex)
            {
            }
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
