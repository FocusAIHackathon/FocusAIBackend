using FocusAI.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Cloops.Exceptions;

namespace FocusAI.Backend.Controllers;

[ApiController]
public class HomeController : ControllerBase
{
    const string base_route = "/api";

    [HttpGet]
    [Route(base_route + "/ping")]
    public StatusResponse ping() {
        StatusResponse sr = new StatusResponse("success");
        return sr;
    }


    [HttpGet]
    [Route(base_route + "/invalid-user-exception")]
    public void Exception() {
        throw new InvalidParameterException("not correct input");
    }
}
