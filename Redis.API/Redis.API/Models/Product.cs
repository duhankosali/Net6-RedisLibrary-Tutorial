namespace Redis.API.Models
{
    public class Product
    {
        public int Id { get; set; } // primary key
        public string Name { get; set; }
        public decimal Price { get; set; }  
    }
}
