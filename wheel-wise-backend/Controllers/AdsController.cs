using Microsoft.AspNetCore.Authorization;
using wheel_wise.Model;
using wheel_wise.Service.Repository;
using Microsoft.AspNetCore.Mvc;
using wheel_wise.ActionFilters;
using wheel_wise.Contracts;
using wheel_wise.Model.Filters;
using wheel_wise.Service.Repository.AdvertisementRepo;
using wheel_wise.Service.Repository.CarRepo;
using wheel_wise.Service.Repository.CarTypeRepo;
using wheel_wise.Service.Repository.ColorRepo;
using wheel_wise.Service.Repository.EquipmentRepo;
using wheel_wise.Service.Repository.FuelTypeRepo;
using wheel_wise.Service.Repository.TransmissionRepo;
using wheel_wise.Service.Repository.UserRepo;

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
    private readonly IUserRepository _userRepository;

    public AdsController(ILogger<AdsController> logger, IAdvertisementRepository advertisementRepository, 
        ICarTypeRepository carTypeRepository, IColorRepository colorRepository, IFuelTypeRepository fuelTypeRepository,
        ITransmissionRepository transmissionRepository, ICarRepository carRepository, IEquipmentRepository equipmentRepository,
        IUserRepository userRepository)
    {
        _logger = logger;
        _advertisementRepository = advertisementRepository;
        _carTypeRepository = carTypeRepository;
        _colorRepository = colorRepository;
        _fuelTypeRepository = fuelTypeRepository;
        _transmissionRepository = transmissionRepository;
        _carRepository = carRepository;
        _equipmentRepository = equipmentRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Advertisement>>> GetAll()
    {
        try
        {
            return Ok(await _advertisementRepository.GetAll());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "Server error.");
        }
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

    [HttpGet("highlighted")]
    public async Task<ActionResult<IEnumerable<Advertisement>>> GetHighlightedAds()
    {
        return Ok(await _advertisementRepository.GetHighlightedAds());
    }

    [WarningFilter("Info")]
    [HttpPost, Authorize]
    public async Task<ActionResult<Advertisement>> PostAd(AdvertisementPostRequest ad)
    {
        Console.WriteLine("Request");
        Console.WriteLine($"Year: {ad.Year}");
        Console.WriteLine($"Price: {ad.Price}");
        Console.WriteLine($"Mileage: {ad.Mileage}");
        Console.WriteLine($"Power: {ad.Power}");
        Console.WriteLine($"User {ad.UserName}");
        
        try
        {
            var carModel = await _carTypeRepository.GetByCarModel(ad.Brand, ad.Model);
            var color = await _colorRepository.GetByName(ad.Color);
            var fuel = await _fuelTypeRepository.GetByName(ad.FuelType);
            var transmission = await _transmissionRepository.GetByName(ad.Transmission);
            var user = await _userRepository.GetByName(ad.UserName);

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

            if (color == null || carModel == null || fuel == null || transmission == null || user == null ||
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
                Title = ad.Title,
                Description = ad.Description,
                CreatedAt = DateTime.Now,
                Highlighted = false,
                UserId = user.Id
            };

             await _advertisementRepository.Add(advertisement);
            return CreatedAtAction(nameof(GetAll), new { id = advertisement.Id }, advertisement);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("SimpleForm")]
    public async Task<ActionResult<IEnumerable<Advertisement>>> GetBySimpleFilter(SimpleFilter simpleFilter)
    {
        Console.WriteLine(simpleFilter);
        return Ok(await _advertisementRepository.GetBySimpleFilter(simpleFilter));
    }

    [WarningFilter("Info")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAdById(int id, AdvertisementPostRequest ad)
    {
        try
        {
            var adToUpdate = await _advertisementRepository.GetById(id);
            if (adToUpdate is null)
            {
                return NotFound();
            }
            
            var carModel = await _carTypeRepository.GetByCarModel(ad.Brand, ad.Model);
            var color = await _colorRepository.GetByName(ad.Color);
            var fuel = await _fuelTypeRepository.GetByName(ad.FuelType);
            var transmission = await _transmissionRepository.GetByName(ad.Transmission);
            var user = await _userRepository.GetByName(ad.UserName);

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

            if (color == null || carModel == null || fuel == null || transmission == null || user == null ||
                !Enum.TryParse(ad.Status, out Status status)) return BadRequest();

            var car = await _carRepository.GetById(adToUpdate.CarId);
            if (car == null)
            {
                return NotFound();
            }
            car.FuelType = fuel;
            car.Year = ad.Year;
            car.Price = ad.Price;
            car.Mileage = ad.Mileage;
            car.Power = ad.Power;
            car.Transmission = transmission;
            car.Equipments = equipments;
            car.Status = status;
            
            await _carRepository.Update(car);

            adToUpdate.Title = ad.Title;
            adToUpdate.Description = ad.Description;
            
            await _advertisementRepository.UpdateById(id, adToUpdate);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> DeleteCarById(int id)
    {
        try
        {
            var adToDelete = await _advertisementRepository.GetById(id);
            if (adToDelete is null)
            {
                return NotFound();
            }

            var car = adToDelete.Car;
            await _advertisementRepository.Delete(adToDelete);
            await _carRepository.Delete(car);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}