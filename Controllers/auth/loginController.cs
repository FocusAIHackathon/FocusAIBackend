using FocusAI.Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace FocusAI.Backend.Controllers;

[ApiController]
public class LoginController : ControllerBase
{
    const string base_route = "/api/auth";

    [HttpPost]
    [Route(base_route + "/login")]
    public JWTResponse issueJWT([FromBody] LoginRequest lr) {
        string jwt = Auth.issueJWT(lr);
        return new JWTResponse(jwt);
    }
}
