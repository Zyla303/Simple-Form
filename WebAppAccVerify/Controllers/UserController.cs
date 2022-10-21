using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace WebAppAccVerify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly DataContext _context;
        public UserController(DataContext context){

            _context = context;

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            if (_context.Users.Any(r => r.Email == request.Email))
            {
                return BadRequest("User already exist.");
            }

            CreatePasswordHash(request.Password, 
                out byte[] passwordHash, 
                out byte[] passwordSalt);
            
            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                VerificationToken = CreateRandomToken()
            };

            _context.Users.Add(user);

            var result = await _context.SaveChangesAsync();

            SendEmail(user);

            return Ok("User successfully created!");

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(r => r.Email == request.Email);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("Not verified!");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Password is incorrect.");
            }

            return Ok($"Welcome back, {user.Email}!");

        }

 //       [HttpPost("Verify")]
 //       public async Task<IActionResult> Verify(string token)
 //       {
 //           var user = await _context.Users.FirstOrDefaultAsync(r => r.VerificationToken == token);

 //           if (user == null)
 //           {
 //               return BadRequest("Invalid token.");
 //           }

 //           user.VerifiedAt = DateTime.Now;
 //           await _context.SaveChangesAsync();

 //           return Ok("User verified!");

 //       }

        [HttpPost("Verification/{token}")]
        public async Task<IActionResult> Verification(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(r => r.VerificationToken == token);

            if (user == null)
            {
                return BadRequest("Invalid token.");
            }

            user.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok("User verified!");

        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        [HttpPost("Verified")]
        public void SendEmail(User user)
        {
            var EmailSender = "";
            var PasswordSender = "";

            using (MailMessage mail = new MailMessage())
            {
                mail.To.Add(new MailAddress(user.Email));
                mail.From = new MailAddress(EmailSender);
                mail.Subject = "Verify Your Account";
                string message = string.Format("Your verify link:<br> <a href='https://localhost:7086/Verification/{0}'>Verification</a>", user.VerificationToken);
                mail.Body = message;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                NetworkCredential NetworkCred = new NetworkCredential(EmailSender, PasswordSender);
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mail);
            }

        }

    }
}
