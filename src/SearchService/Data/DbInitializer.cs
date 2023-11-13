using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using System.Text.Json;

namespace SearchService.Data
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            await DB.InitAsync("SearchDb", MongoClientSettings
           .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnectionString")));

            //Creating index to search based on following columns
            await DB.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Colour, KeyType.Text)
                .CreateAsync();

            var count = await DB.CountAsync<Item>();
            if(count == 0)
            {
                await Console.Out.WriteLineAsync("No data");
                var itemData = await File.ReadAllTextAsync("Data/auctions.json");
                var options= new JsonSerializerOptions { PropertyNameCaseInsensitive = true };  
                var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);
                await DB.SaveAsync(items);
            }
        }
    }
}
