using Backend.Dtos.BookAccolade;
using Backend.enums;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookAccoladeController : ControllerBase
{
    private readonly IBookAccoladeService _accoladeService;

    public BookAccoladeController(IBookAccoladeService accoladeService)
    {
        _accoladeService = accoladeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookAccoladeResponse>>> GetAllAccolades()
    {
        var accolades = await _accoladeService.GetAllAccolades();
        return Ok(accolades);
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<BookAccoladeResponse>>> GetAccoladesByType(AccoladeType type)
    {
        var accolades = await _accoladeService.GetAccoladesByType(type);
        return Ok(accolades);
    }

    [HttpGet("book/{bookId}")]
    public async Task<ActionResult<IEnumerable<BookAccoladeResponse>>> GetAccoladesByBookId(Guid bookId)
    {
        var accolades = await _accoladeService.GetAccoladesByBookId(bookId);
        return Ok(accolades);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookAccoladeResponse>> GetAccoladeById(Guid id)
    {
        try
        {
            var accolade = await _accoladeService.GetAccoladeById(id);
            return Ok(accolade);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<BookAccoladeResponse>> CreateAccolade(CreateBookAccoladeRequest request)
    {
        try
        {
            var accolade = await _accoladeService.CreateAccolade(request);
            return CreatedAtAction(nameof(GetAccoladeById), new { id = accolade.ID }, accolade);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<BookAccoladeResponse>> UpdateAccolade(Guid id, UpdateBookAccoladeRequest request)
    {
        try
        {
            var accolade = await _accoladeService.UpdateAccolade(id, request);
            return Ok(accolade);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult> DeleteAccolade(Guid id)
    {
        var result = await _accoladeService.DeleteAccolade(id);
        if (!result)
        {
            return NotFound($"Accolade with ID {id} not found");
        }

        return NoContent();
    }
}
