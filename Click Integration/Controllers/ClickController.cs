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
        public IActionResult Prepare(IFormCollection request)
        {
            string clickTransId = request["Request.click_trans_id"];
            string serviceId = request["Request.service_id"];
            string clickPaydocId = request["Request.click_paydoc_id"];
            string merchantTransId = request["Request.merchant_trans_id"];
            decimal amount = Convert.ToDecimal(request["Request.amount"]);
            string action = request["Request.action"];
            string error = request["Request.error"];
            string errorNote = request["Request.error_note"];
            string signTime = request["Request.sign_time"];
            string signString = request["Request.sign_string"];
            string secretKey = "siznigSecretKeyingiz";

            string generatedSignString = GenerateMd5($"{clickTransId}{serviceId}{secretKey}{merchantTransId}{amount}{action}{signTime}");

            if (signString != generatedSignString)
            {
                return BadRequest(new { error = -1, error_note = "Invalid sign_string" });
            }

            //_dbContext.ClickUz.Add(new ClickUz
            //{
            //    ClickTransId = clickTransId,
            //    MerchantTransId = merchantTransId,
            //    Amount = amount,
            //    AmountRub = amount,
            //    SignTime = signTime,
            //    Situation = error
            //});
            //_dbContext.SaveChanges();

            var response = new
            {
                click_trans_id = clickTransId,
                merchant_trans_id = merchantTransId,
                merchant_prepare_id = merchantTransId,
                error = error == "0" ? 0 : -9,
                error_note = error == "0" ? "Payment prepared successfully" : "Do not find a user!!!"
            };

            return Ok(response);
        }

        [HttpPost("complete")]
        public IActionResult Complete(IFormCollection request)
        {
            string clickTransId = request["Request.click_trans_id"];
            string serviceId = request["Request.service_id"];
            string clickPaydocId = request["Request.click_paydoc_id"];
            string merchantTransId = request["Request.merchant_trans_id"];
            string merchantPrepareId = request["Request.merchant_prepare_id"];
            decimal amount = Convert.ToDecimal(request["Request.amount"]);
            string action = request["Request.action"];
            string error = request["Request.error"];
            string errorNote = request["Request.error_note"];
            string signTime = request["Request.sign_time"];
            string signString = request["Request.sign_string"];
            string secretKey = "siznigSecretKeyingiz";

            string generatedSignString = GenerateMd5($"{clickTransId}{serviceId}{secretKey}{merchantTransId}{merchantPrepareId}{amount}{action}{signTime}");

            if (signString != generatedSignString)
            {
                return BadRequest(new { error = -1, error_note = "Invalid sign_string" });
            }

            if (error == "0")
            {
                //var clickRecord = _dbContext.ClickUz.FirstOrDefault(c => c.ClickTransId == clickTransId);
                //if (clickRecord != null)
                //{
                //    clickRecord.Situation = 1;
                //    clickRecord.Status = "success";
                //    _dbContext.SaveChanges();
                //}

                //var order = _dbContext.Orders.FirstOrDefault(o => o.Id == merchantTransId);
                //if (order != null)
                //{
                //    order.Status = "yakunlandi";
                //    _dbContext.SaveChanges();
                //}

                return Ok(new
                {
                    click_trans_id = clickTransId,
                    merchant_trans_id = merchantTransId,
                    merchant_confirm_id = merchantTransId,
                    error = 0,
                    error_note = "Payment Success"
                });
            }
            else
            {
                //    var clickRecord = _dbContext.ClickUz.FirstOrDefault(c => c.ClickTransId == clickTransId);
                //    if (clickRecord != null)
                //    {
                //        clickRecord.Situation = -9;
                //        clickRecord.Status = "error";
                //        _dbContext.SaveChanges();
                //    }

                //    var order = _dbContext.Orders.FirstOrDefault(o => o.Id == merchantTransId);
                //    if (order != null)
                //    {
                //        order.Status = "bekor qilingan";
                //        _dbContext.SaveChanges();
                //    }

                return Ok(new
                {
                    click_trans_id = clickTransId,
                    merchant_trans_id = merchantTransId,
                    merchant_confirm_id = merchantTransId,
                    error = -9,
                    error_note = "Do not find a user!!!"
                });
            }
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

        private string GenerateMd5(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
