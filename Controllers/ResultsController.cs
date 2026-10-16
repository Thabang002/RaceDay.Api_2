using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models.DTOs;
using RaceDay.Api.Models.Entities;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
[Tags("Results")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public ResultsController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    private string CurrentRole => User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;

    /// <summary>Capture a participant's result (Organiser only).</summary>
    [HttpPost("enrolments/{id}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(int id, [FromBody] CreateResultDto dto)
    {
        var enrolment = await _db.Enrolments.Include(e => e.Event).FirstOrDefaultAsync(e => e.EnrolmentId == id);
        if (enrolment == null) return NotFound();
        if (enrolment.Event!.CreatedBy != CurrentUserId) return Forbid();

        if (await _db.Results.AnyAsync(r => r.EnrolmentId == id))
            return BadRequest(new { message = "Result already exists." });

        var result = new Result
        {
            EnrolmentId = id,
            FinishTime = dto.FinishTime,
            Position = dto.Position,
            PacePerKm = dto.PacePerKm,
            Status = dto.Status,
            Notes = dto.Notes
        };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { result.ResultId, result.EnrolmentId, result.Position });
    }

    /// <summary>Get specific result.</summary>
    [HttpGet("results/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var r = await _db.Results
            .Include(x => x.Enrolment).ThenInclude(e => e!.User)
            .Include(x => x.Enrolment).ThenInclude(e => e!.Event)
            .Include(x => x.Enrolment).ThenInclude(e => e!.Category)
            .FirstOrDefaultAsync(x => x.ResultId == id);
        if (r == null) return NotFound();

        if (CurrentRole != "Organiser" && r.Enrolment!.UserId != CurrentUserId) return Forbid();

        return Ok(Map(r));
    }

    /// <summary>Update a result (Organiser only).</summary>
    [HttpPut("results/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateResultDto dto)
    {
        var r = await _db.Results.Include(x => x.Enrolment).ThenInclude(e => e!.Event)
            .FirstOrDefaultAsync(x => x.ResultId == id);
        if (r == null) return NotFound();
        if (r.Enrolment!.Event!.CreatedBy != CurrentUserId) return Forbid();

        r.FinishTime = dto.FinishTime;
        r.Position = dto.Position;
        r.PacePerKm = dto.PacePerKm;
        r.Status = dto.Status;
        r.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        return Ok(new { r.ResultId });
    }

    /// <summary>Get all results for a participant.</summary>
    [HttpGet("users/{id}/results")]
    public async Task<IActionResult> GetByUser(int id)
    {
        if (CurrentRole != "Organiser" && CurrentUserId != id) return Forbid();
        if (!await _db.Users.AnyAsync(u => u.UserId == id)) return NotFound();

        var results = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.User)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
            .Where(r => r.Enrolment!.UserId == id).ToListAsync();

        return Ok(results.Select(Map));
    }

    /// <summary>Get all results for an event (owner Organiser only).</summary>
    [HttpGet("events/{id}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetByEvent(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        if (ev.CreatedBy != CurrentUserId) return Forbid();

        var results = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.User)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
            .Where(r => r.Enrolment!.EventId == id)
            .OrderBy(r => r.Position)
            .ToListAsync();

        return Ok(results.Select(Map));
    }

    /// <summary>Get leaderboard (public).</summary>
    [HttpGet("results/leaderboard")]
    [AllowAnonymous]
    public async Task<IActionResult> Leaderboard([FromQuery] int eventId, [FromQuery] int? categoryId)
    {
        if (eventId == 0) return BadRequest(new { message = "event_id is required." });

        var q = _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.User)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
            .Where(r => r.Enrolment!.EventId == eventId);

        if (categoryId.HasValue) q = q.Where(r => r.Enrolment!.CategoryId == categoryId);

        var list = await q.OrderBy(r => r.Position).ToListAsync();
        return Ok(list.Select(Map));
    }

    private static ResultDto Map(Result r) => new(
        r.ResultId, r.EnrolmentId,
        r.Enrolment?.User?.FullName, r.Enrolment?.Event?.Title, r.Enrolment?.Category?.Name,
        r.FinishTime, r.Position, r.PacePerKm, r.Status, r.Notes);
}