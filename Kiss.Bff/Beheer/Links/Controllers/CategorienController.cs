using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kiss.Bff.Beheer.Links.Data.Entities;
using Kiss.Bff.NieuwsEnWerkinstructies.Controllers;
using Microsoft.Build.Framework;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using NuGet.Common;
using Kiss.Bff.Beheer.Data;

namespace Kiss.Bff.Beheer.Links.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategorienController : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public CategorienController(BeheerDbContext context)
        {
            _context = context;
        }

        // GET: api/Categorien
        [HttpGet]
        public IActionResult GetLinks()
        {
            if (_context.Links == null)
            {
                return NotFound();
            }

            var result = _context
               .Links
               .GroupBy(x => x.Categorie)
               .Select(categorieGroep => categorieGroep.Key  )
               .AsAsyncEnumerable();

            return Ok(result);
        }
         

    }
}
