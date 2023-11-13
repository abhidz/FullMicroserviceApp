using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery]SearchParams parameter)
        {
            var query = DB.PagedSearch<Item, Item>();
            query.Sort(x => x.Ascending(a => a.Make));
            if (!string.IsNullOrWhiteSpace(parameter.SearchTerm))
            {
                query.Match(Search.Full, parameter.SearchTerm).SortByTextScore();
            }
            query = parameter.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(x => x.Make)),
                "new" => query.Sort(x => x.Descending(x => x.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
            };

            query = parameter.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd<DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };

            if (!string.IsNullOrEmpty(parameter.Seller))
            {
                query.Match(x => x.Seller == parameter.Seller);
            }

            if (!string.IsNullOrEmpty(parameter.Winner))
            {
                query.Match(x => x.Winner == parameter.Winner);
            }
            query.PageNumber(parameter.PageNumber);
            query.PageSize(parameter.PageSize);
            var result = await query.ExecuteAsync();

            return Ok(new
            {   
                result = result.Results,
                pageCount = result.PageCount,
                totalCount= result.TotalCount
            });
        }
    }
}
