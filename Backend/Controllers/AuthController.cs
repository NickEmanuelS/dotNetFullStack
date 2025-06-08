using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        // Lista de usuários em memória (em produção, usar banco de dados)
        private static List<User> users = new();
        private static int nextUserId = 1;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public ActionResult<AuthResponse> Register(RegisterRequest request)
        {
            // Verificar se usuário já existe
            if (users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "E-mail já está em uso" });
            }

            // Validações básicas
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "E-mail e senha são obrigatórios" });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "Senha deve ter pelo menos 6 caracteres" });
            }

            // Criar novo usuário
            var user = new User
            {
                Id = nextUserId++,
                Name = request.Name,
                Email = request.Email,
                PasswordHash = _authService.HashPassword(request.Password)
            };

            users.Add(user);

            // Gerar token
            var token = _authService.GenerateJwtToken(user);

            // Retornar resposta sem a senha
            var userResponse = new User
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return Ok(new AuthResponse
            {
                Token = token,
                User = userResponse
            });
        }

        [HttpPost("login")]
        public ActionResult<AuthResponse> Login(LoginRequest request)
        {
            // Buscar usuário
            var user = users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "E-mail ou senha inválidos" });
            }

            // Verificar senha
            if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return BadRequest(new { message = "E-mail ou senha inválidos" });
            }

            // Gerar token
            var token = _authService.GenerateJwtToken(user);

            // Retornar resposta sem a senha
            var userResponse = new User
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return Ok(new AuthResponse
            {
                Token = token,
                User = userResponse
            });
        }

        [HttpGet("users")]
        public ActionResult GetUsers()
        {
            // Endpoint para debug - remover em produção
            return Ok(users.Select(u => new { u.Id, u.Name, u.Email, u.CreatedAt }));
        }
    }
}