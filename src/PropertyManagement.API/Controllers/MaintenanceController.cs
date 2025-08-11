using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;

namespace PropertyManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly PropertyManagementDbContext _context;

        public MaintenanceController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaintenanceRequest>>> GetMaintenanceRequests(
            [FromQuery] MaintenanceStatus? status = null,
            [FromQuery] MaintenancePriority? priority = null,
            [FromQuery] MaintenanceCategory? category = null,
            [FromQuery] int? propertyId = null,
            [FromQuery] int? tenantId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.MaintenanceRequests
                .Include(m => m.Property)
                    .ThenInclude(p => p.Owner)
                .Include(m => m.Tenant)
                .Include(m => m.Images)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(m => m.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(m => m.Priority == priority.Value);

            if (category.HasValue)
                query = query.Where(m => m.Category == category.Value);

            if (propertyId.HasValue)
                query = query.Where(m => m.PropertyId == propertyId.Value);

            if (tenantId.HasValue)
                query = query.Where(m => m.TenantId == tenantId.Value);

            var totalCount = await query.CountAsync();
            var requests = await query
                .OrderByDescending(m => m.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page", page.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());

            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceRequest>> GetMaintenanceRequest(int id)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Property)
                    .ThenInclude(p => p.Owner)
                .Include(m => m.Tenant)
                .Include(m => m.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null)
                return NotFound();

            return Ok(request);
        }

        [HttpPost]
        public async Task<ActionResult<MaintenanceRequest>> CreateMaintenanceRequest(MaintenanceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Generate request number
            var lastRequest = await _context.MaintenanceRequests
                .OrderByDescending(m => m.Id)
                .FirstOrDefaultAsync();

            var requestNumber = $"MR{DateTime.Now:yyyyMM}{(lastRequest?.Id + 1 ?? 1):D4}";
            request.RequestNumber = requestNumber;
            request.RequestDate = DateTime.UtcNow;
            request.Status = MaintenanceStatus.Submitted;

            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMaintenanceRequest), new { id = request.Id }, request);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMaintenanceRequest(int id, MaintenanceRequest request)
        {
            if (id != request.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(request).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaintenanceRequestExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] MaintenanceStatus status)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.Status = status;

            if (status == MaintenanceStatus.Completed)
                request.CompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignMaintenance(int id, [FromBody] AssignMaintenanceRequest assignRequest)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.AssignedTo = assignRequest.AssignedTo;
            request.AssignedToPhone = assignRequest.AssignedToPhone;
            request.ScheduledDate = assignRequest.ScheduledDate;
            request.Status = MaintenanceStatus.InProgress;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteMaintenance(int id, [FromBody] CompleteMaintenanceRequest completeRequest)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.Status = MaintenanceStatus.Completed;
            request.CompletedDate = DateTime.UtcNow;
            request.ActualCost = completeRequest.ActualCost;
            request.CompletionNotes = completeRequest.CompletionNotes;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaintenanceRequest(int id)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            // Only allow deletion if status is Submitted or Cancelled
            if (request.Status != MaintenanceStatus.Submitted && request.Status != MaintenanceStatus.Cancelled)
                return BadRequest("Cannot delete maintenance request that is in progress or completed");

            _context.MaintenanceRequests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("urgent")]
        public async Task<ActionResult<IEnumerable<MaintenanceRequest>>> GetUrgentRequests()
        {
            var urgentRequests = await _context.MaintenanceRequests
                .Include(m => m.Property)
                    .ThenInclude(p => p.Owner)
                .Include(m => m.Tenant)
                .Where(m => (m.Priority == MaintenancePriority.Emergency || m.Priority == MaintenancePriority.High) &&
                           m.Status != MaintenanceStatus.Completed &&
                           m.Status != MaintenanceStatus.Cancelled)
                .OrderBy(m => m.Priority)
                .ThenBy(m => m.RequestDate)
                .ToListAsync();

            return Ok(urgentRequests);
        }

        private bool MaintenanceRequestExists(int id)
        {
            return _context.MaintenanceRequests.Any(e => e.Id == id);
        }
    }

    public class AssignMaintenanceRequest
    {
        public string AssignedTo { get; set; } = string.Empty;
        public string AssignedToPhone { get; set; } = string.Empty;
        public DateTime? ScheduledDate { get; set; }
    }

    public class CompleteMaintenanceRequest
    {
        public decimal? ActualCost { get; set; }
        public string CompletionNotes { get; set; } = string.Empty;
    }
}
