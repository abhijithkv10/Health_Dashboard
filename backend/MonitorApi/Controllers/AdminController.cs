using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorApi.Models;
using MonitorApi.Services;

namespace MonitorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IInstanceService _instances;

    public AdminController(IInstanceService instances)
    {
        _instances = instances;
    }

    [HttpGet("instances")]
    public async Task<ActionResult<List<InstanceConfig>>> GetAll()
    {
        return await _instances.GetAllAsync();
    }

    [HttpPost("instances")]
    public async Task<ActionResult<InstanceConfig>> Add([FromBody] InstanceConfig instance)
    {
        var created = await _instances.AddAsync(instance);
        return Created($"/api/admin/instances/{created.Id}", created);
    }

    [HttpPut("instances/{id}")]
    public async Task<ActionResult<InstanceConfig>> Update(int id, [FromBody] InstanceConfig instance)
    {
        var updated = await _instances.UpdateAsync(id, instance);
        if (updated == null) return NotFound();
        return updated;
    }

    [HttpDelete("instances/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _instances.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
