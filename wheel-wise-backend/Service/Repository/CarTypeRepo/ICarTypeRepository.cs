using wheel_wise.Model;

namespace wheel_wise.Service.Repository.CarTypeRepo;

public interface ICarTypeRepository
{
    Task<IEnumerable<CarType>> GetAll();
    Task<CarType>? GetByCarModel(string brand, string model);
    void Add(CarType carCarType);
    void Delete(CarType carCarType);
    void Update(CarType carCarType);
}