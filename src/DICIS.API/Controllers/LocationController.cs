using DICIS.Core.Data;
using DICIS.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DICIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    [HttpGet("countries")]
    public ActionResult<LocationResponse> GetCountries()
    {
        var countries = LocationData.GetCountries();
        return Ok(new LocationResponse { Countries = countries });
    }

    [HttpGet("states")]
    public ActionResult<LocationResponse> GetStates([FromQuery] string country)
    {
        if (string.IsNullOrEmpty(country))
        {
            return BadRequest(new { message = "Country is required" });
        }

        var states = LocationData.GetStates(country);
        return Ok(new LocationResponse { States = states });
    }

    [HttpGet("local-governments")]
    public ActionResult<LocationResponse> GetLocalGovernments([FromQuery] string country, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(state))
        {
            return BadRequest(new { message = "Country and State are required" });
        }

        var localGovernments = LocationData.GetLocalGovernments(country, state);
        return Ok(new LocationResponse { LocalGovernments = localGovernments });
    }
}
