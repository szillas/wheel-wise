using Microsoft.AspNetCore.Mvc;
using wheel_wise.Model;
using wheel_wise.Service.Repository.CarTypeRepo;
using wheel_wise.Service.Repository.ColorRepo;

namespace wheel_wise.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ColorController : ControllerBase
{
    private ILogger<ColorController> _logger;
    private IColorRepository _colorRepository;

    public ColorController(ILogger<ColorController> logger, IColorRepository colorRepository)
    {
        _logger = logger;
        _colorRepository = colorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var colors = await _colorRepository.GetAll();
            return Ok(colors);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while trying to list all colors.");
            return StatusCode(500, "An error occured while trying to list all colors.");
        }
    }

    [HttpGet("getbyname/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        try
        {
            var color = await _colorRepository.GetByName(name);
            if (color == null)
            {
                return NotFound("No color with this name found!");
            }

            return Ok(color);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while trying to get a specific color.");
            return StatusCode(500, "An error occured while trying to get a specific color.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddColor(Color color)
    {
        try
        {
            _colorRepository.Add(color);
            bool colorAdded = await _colorRepository.GetByName(color.Name) != null;
            if (!colorAdded)
            {
                throw new NullReferenceException("Color wasn't added!");
            }

            return Ok($"{color.Name} added");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while trying to ADD a color.");
            return StatusCode(500, "An error occured while trying to ADD a color.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteColor(int id)
    {
        try
        {
            var color = await _colorRepository.GeById(id);
            if (color == null)
            {
                return NotFound("Can't find the color to delete");
            }

            _colorRepository.Delete(color);
            return NoContent();

        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while trying to delete a color.");
            return StatusCode(500, "An error occured while trying to delete a color.");
        }
    }
}