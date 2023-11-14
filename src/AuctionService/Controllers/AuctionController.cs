using AuctionService.Data;
using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _endpoint;

        public AuctionController(AuctionDbContext context, IMapper mapper, IPublishEndpoint endpoint)
        {
            _context = context;
            _mapper = mapper;
            _endpoint = endpoint;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDTO>>> Get()
        {
            var auctionList = await _context.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();
            return _mapper.Map<List<AuctionDTO>>(auctionList);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDTO>> GetById(Guid id)
        {
            var auction = await _context.Auctions.Include(x => x.Item).Where(x => x.Id == id).FirstOrDefaultAsync();
            if(auction == null)
            {
                return NotFound("Auction not found");
            }
            return _mapper.Map<AuctionDTO>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDTO>> Post(CreateAuctionDTO auctionDTO)
        {
            var auction = _mapper.Map<Auction>(auctionDTO);
            auction.Seller = "test";
            _context.Auctions.Add(auction);
            var result = await _context.SaveChangesAsync() > 0;
            // Publishing message to queue
            var newAuction = _mapper.Map<AuctionDTO>(auction);
            await _endpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

            if(!result)
            {
                return BadRequest("Changes not saved");
            }
            return CreatedAtAction(nameof(GetById), new { auction.Id },newAuction);
        }

        [HttpPut]
        public async Task<ActionResult> Put(Guid id, UpdateAuctionDTO auctionDTO)
        {
            var auction = await _context.Auctions.Include(x => x.Item).Where(x => x.Id == id).FirstOrDefaultAsync();
            if(auction is null)
            {
                return NotFound("Data not found to update auction");
            }
            auction.Item.Model = auctionDTO.Model ?? auction.Item.Model;
            auction.Item.Make = auctionDTO.Make ?? auction.Item.Make;
            auction.Item.Year = auctionDTO.Year ?? auction.Item.Year;
            auction.Item.Colour = auctionDTO.Colour ?? auction.Item.Colour;
            auction.Item.Mileage = auctionDTO.Mileage ?? auction.Item.Mileage;
            // Publishing message to queue
            await _endpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
            var result = await _context.SaveChangesAsync() > 0;
           
            if (!result)
            {
                return BadRequest("Changes not updated");
            }
            return Ok(result);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction is null)
            {
                return NotFound("Data not found to delete auction");
            }
            _context.Auctions.Remove(auction);
            await _endpoint.Publish<AuctionDeleted>(new { id = auction.Id.ToString() });

            var result = await _context.SaveChangesAsync() > 0;
            if (!result)
            {
                return BadRequest("Changes not deleted");
            }
            return Ok();
        }
    }
}
