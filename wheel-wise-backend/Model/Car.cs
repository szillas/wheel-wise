namespace wheel_wise.Model;

public class Car
{
    // Required properties
    public int Id { get; init; }
    public int ColorId { get; init; }
    public Color Color { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public int Power { get; set; }
    public int FuelTypeId { get; init; }
    public FuelType FuelType { get; set; }
    public Status Status { get; set; }
    public int TransmissionId { get; set; }
    public Transmission Transmission { get; set; } 
    public int TypeId { get; init; }
    public Type Type { get; init; }
    public ICollection<Equipment> Equipments { get; set; }
}