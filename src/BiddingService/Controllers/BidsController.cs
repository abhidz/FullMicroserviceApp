using BiddingService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BidsController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Bid>>> GetAuctionBids(string auctionId)
        {
            var bids = await DB.Find<Bid>()
                .Match(a => a.AuctionId == auctionId)
                .Sort(a => a.Descending(a=>a.BidTime)).
                ExecuteAsync();
            return bids;
        }

        [HttpPost]
        public async Task<ActionResult<Bid>> PlaceBid(string auctionId, int amount)
        {
            var auction = await DB.Find<Auction>().OneAsync(auctionId);
            if (auction is null)
            {
                return NotFound("Auction not found for which you are trying to place bid");
            }
            if (auction.Seller == User.Identity.Name)
            {
                return BadRequest("You cannot bid your own auction");
            }
            var bid = new Bid
            {
                Amount = amount,
                AuctionId = auctionId,
                Bidder = User.Identity.Name
            };
            if (auction.AuctionEnd < DateTime.UtcNow)
            {
                bid.BidStatus = BidStatus.Finished;
            }
            else
            {
                var highBid = await DB.Find<Bid>().Match(a => a.AuctionId == auctionId).Sort(b => b.Descending(x => x.Amount)).ExecuteFirstAsync();

                if (highBid is not null && amount > highBid.Amount || highBid is null)
                {
                    bid.BidStatus = amount > auction.ReservePrice ? BidStatus.AcceptedPrice : BidStatus.AcceptedBelowReservePrice;
                }

                if (highBid is not null && bid.Amount < highBid.Amount)
                {
                    bid.BidStatus = BidStatus.TooLowPrice;
                }
            }

            await DB.SaveAsync(bid);
            return Ok(bid);
        }
    }
}
