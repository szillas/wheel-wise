namespace wheel_wise.Model;

public class Advertisement
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Highlighted { get; set; }
    public int UserId { get; init; }
    public User User { get; init; }
    public int CarId { get; init; }
    public Car Car { get; set; }
}