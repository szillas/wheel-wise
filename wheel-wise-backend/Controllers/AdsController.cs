using wheel_wise.Model;
using wheel_wise.Service.Repository;
using Microsoft.AspNetCore.Mvc;
using wheel_wise.ActionFilters;
using wheel_wise.Contracts;
using wheel_wise.Service.Repository.AdvertisementRepo;
using wheel_wise.Service.Repository.CarRepo;
using wheel_wise.Service.Repository.CarTypeRepo;
using wheel_wise.Service.Repository.ColorRepo;
using wheel_wise.Service.Repository.EquipmentRepo;
using wheel_wise.Service.Repository.FuelTypeRepo;
using wheel_wise.Service.Repository.TransmissionRepo;

namespace wheel_wise.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdsController : ControllerBase
{
    private readonly ILogger<AdsController> _logger;
    private readonly IAdvertisementRepository _advertisementRepository;
    private readonly ICarTypeRepository _carTypeRepository;
    private readonly IColorRepository _colorRepository;
    private readonly IFuelTypeRepository _fuelTypeRepository;
    private readonly ITransmissionRepository _transmissionRepository;
    private readonly ICarRepository _carRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public AdsController(ILogger<AdsController> logger, IAdvertisementRepository advertisementRepository, 
        ICarTypeRepository carTypeRepository, IColorRepository colorRepository, IFuelTypeRepository fuelTypeRepository,
        ITransmissionRepository transmissionRepository, ICarRepository carRepository, IEquipmentRepository equipmentRepository)
    {
        _logger = logger;
        _advertisementRepository = advertisementRepository;
        _carTypeRepository = carTypeRepository;
        _colorRepository = colorRepository;
        _fuelTypeRepository = fuelTypeRepository;
        _transmissionRepository = transmissionRepository;
        _carRepository = carRepository;
        _equipmentRepository = equipmentRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Advertisement>>> GetAll()
    {
        return Ok(await _advertisementRepository.GetAll());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Advertisement>> GetById(int id)
    {
        var ad = await _advertisementRepository.GetById(id);
        if (ad == null)
        {
            return NotFound($"Advertisement with id: {id} was not found.");
        }

        return Ok(ad);
    }

    [WarningFilter("Info")]
    [HttpPost]
    public async Task<ActionResult<Advertisement>> PostAd(AdvertisementPostRequest ad)
    {
        Console.WriteLine("Request");
        Console.WriteLine($"Year: {ad.Year}");
        Console.WriteLine($"Price: {ad.Price}");
        Console.WriteLine($"Mileage: {ad.Mileage}");
        Console.WriteLine($"Power: {ad.Power}");
        
        try
        {
            var carModel = await _carTypeRepository.GetByCarModel(ad.Brand, ad.Model);
            var color = await _colorRepository.GetByName(ad.Color);
            var fuel = await _fuelTypeRepository.GetByName(ad.FuelType);
            var transmission = await _transmissionRepository.GetByName(ad.Transmission);

            ICollection<Equipment> equipments = new List<Equipment>();
            foreach (var ads in ad.Equipments)
            {
                Console.WriteLine($"{ads.Key} - {ads.Value}");
                if (ads.Value == true)
                {
                    var equipment = await _equipmentRepository.GetById(ads.Key);
                    if (equipment != null)
                    {
                        equipments.Add(equipment);
                    }
                }
            }

            if (color == null || carModel == null || fuel == null || transmission == null ||
                !Enum.TryParse(ad.Status, out Status status)) return BadRequest();
            
            Console.WriteLine(color.Id);
            Console.WriteLine(carModel.Id);
            Console.WriteLine(fuel.Id);
            Console.WriteLine(status);
                
            Car newCar = new Car
            {
                CarTypeId = carModel.Id,
                ColorId = color.Id,
                Year = ad.Year,
                Price = ad.Price,
                Mileage = ad.Mileage,
                Power = ad.Power,
                FuelTypeId = fuel.Id,
                Status = status,
                TransmissionId = transmission.Id,
                Equipments = equipments
            };
            await _carRepository.Add(newCar);
                
            Advertisement advertisement = new Advertisement
            {
                CarId = newCar.Id,
                Title = "Random title",
                Description = "Desc",
                CreatedAt = DateTime.Now,
                Highlighted = false,
                UserId = null
            };

            await _advertisementRepository.Add(advertisement);
            return CreatedAtAction(nameof(GetAll), new { id = advertisement.Id }, ad);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [WarningFilter("Info")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAdById(int id, Advertisement ad)
    {
        if (id != ad.Id)
        {
            return BadRequest();
        }

        try
        {
            var adToUpdate = await _advertisementRepository.GetById(id);
            if (adToUpdate is null)
            {
                return NotFound();
            }
            
            await _advertisementRepository.UpdateById(id, ad);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCarById(int id)
    {
        var adToDelete = await _advertisementRepository.GetById(id);
        if (adToDelete is null)
        {
            return NotFound();
        }

        await _advertisementRepository.Delete(adToDelete);
        return NoContent();
    }
}