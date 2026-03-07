using Microsoft.AspNetCore.Mvc;
using ModbusIzleme.Interfaces;
using ModbusIzleme.Models;

namespace ModbusIzleme.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CihazController : ControllerBase
{
    private readonly ICihazVerisiDeposu _veriDeposu;

    public CihazController(ICihazVerisiDeposu veriDeposu)
    {
        _veriDeposu = veriDeposu;
    }

    [HttpGet("son-veri")]
    [ProducesResponseType(typeof(CihazVerisi), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult<CihazVerisi> SonVeriGetir()
    {
        var veri = _veriDeposu.SonVeri;
        if (veri is null) return NoContent();
        return Ok(veri);
    }
}
