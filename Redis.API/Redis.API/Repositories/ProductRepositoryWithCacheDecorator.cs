using Redis.API.Models;
using Redis.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace Redis.API.Repositories
{
    public class ProductRepositoryWithCacheDecorator : IProductRepository
    {
        // caching key
        private const string productKey = "productCaches";

        // constructor
        private readonly IProductRepository _productRepository;
        private readonly RedisService _redisService;
        private readonly IDatabase _cacheRepository;
        public ProductRepositoryWithCacheDecorator(IProductRepository productRepository, RedisService redisService)
        {
            _productRepository = productRepository;
            _redisService = redisService;
            _cacheRepository = _redisService.GetDb(0);
        }       

        // db (private)
        private async Task<List<Product>> LoadToCacheFromDbAsync() // caching releated data
        {
            var products = await _productRepository.GetAsync();
            products.ForEach(p =>
            {
                _cacheRepository.HashSetAsync(productKey, p.Id, JsonSerializer.Serialize(p));
            });

            return products;
        }

        // implemented methods.
        public async Task<Product> CreateAsync(Product product)
        {
            var newProduct = await _productRepository.CreateAsync(product); // create database

            if(await _cacheRepository.KeyExistsAsync(productKey)) // is exists?
            {
                await _cacheRepository.HashSetAsync(productKey, product.Id, JsonSerializer.Serialize<Product>(newProduct)); // caching
            }

            return newProduct;
        }

        public async Task<List<Product>> GetAsync()
        {
            if(!await _cacheRepository.KeyExistsAsync(productKey)) // if the data is not in the cache:
            {
                return await LoadToCacheFromDbAsync(); // caching
            }

            var cacheProducts = (await _cacheRepository.HashGetAllAsync(productKey)).ToList();
            var products = new List<Product>();
            foreach (var item in cacheProducts)
            {
                var product = JsonSerializer.Deserialize<Product>(item.Value);
                products.Add(product);
            }

            return products;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            if (!await _cacheRepository.KeyExistsAsync(productKey)) // if the data is not in the cache:
            {
                var product = await _cacheRepository.HashGetAsync(productKey, id);
                return product.HasValue ? JsonSerializer.Deserialize<Product>(product) : null;
            }

            var products = await LoadToCacheFromDbAsync();
            return products.FirstOrDefault(x => x.Id == id);
        }
    }
}
