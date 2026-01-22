using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WayBillController : ControllerBase
{
    private readonly IWayBillService _waybillService;
    private readonly ILogger<WayBillController> _logger;

    public WayBillController(IWayBillService waybillService, ILogger<WayBillController> logger)
    {
        _waybillService = waybillService;
        _logger = logger;
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<WayBillDto>>> GetAllWayBill()
    {
        var wayBills = await _waybillService.GetAllWayBills();
        return Ok(wayBills);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WayBillDto>> GetWayBillById(int id)
    {
        var wayBill = await _waybillService.GetWayBillById(id);
        if (wayBill == null)
            return NotFound($"No waybill found");
        return Ok(wayBill);
    }

    [HttpGet("items/{id:int}")]
    public async Task<ActionResult<List<WayBillItemsDto>>> GetAllWayBillItems(int id)
    { 
        var wayBillItems = await _waybillService.GetAllWayBillItemsByWayBillId(id);
        if (wayBillItems == null)
            return NotFound();
        return Ok(wayBillItems);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateWayBill(WayBillDto dto)
    {
        try
        {
            var result = await _waybillService.CreateWayBill(dto);
            if (!result)
                return BadRequest("Way bill already exist");
            return Ok(result);
        }
        catch(Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    //Damage Items Query 

    [HttpGet("all/damage/{waybillId:int}")]
    public async Task<ActionResult<List<WayBillDamageItemDto>>> GetAllWaybillDamageItems(int waybillId)
    {
        try
        {
            var damageitems = await _waybillService.GetAllDamageItems(waybillId);
            if (damageitems == null)
                return NotFound();
            return Ok(damageitems);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }



    [HttpPost("damage")]
    public async Task<IActionResult> CreateDamageWayBill(List<WayBillDamageItemDto> dtos)
    {
        try
        {
            var result = await _waybillService.AddDamageWayBillItems(dtos);
            if (!result)
                return BadRequest("Adding damage items failed");
            return Ok(result);
        }
        catch(Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
