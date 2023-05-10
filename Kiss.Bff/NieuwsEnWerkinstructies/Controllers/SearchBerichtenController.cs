﻿using System.Security.Claims;
using IdentityModel;
using Kiss.Bff.NieuwsEnWerkinstructies.Data;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.NieuwsEnWerkinstructies.Controllers
{
    [ApiController]
    public class SearchBerichtenController : ControllerBase
    {
        private readonly NieuwsEnWerkinstructiesDbContext _context;

        public SearchBerichtenController(NieuwsEnWerkinstructiesDbContext context)
        {
            _context = context;
        }

        [HttpGet("api/berichten/published")]
        public async Task<IActionResult> GetBerichten([FromQuery] BerichtFilterModel? filterModel, CancellationToken token)
        {
            var pageSize = filterModel?.PageSize ?? 15;
            var page = filterModel?.Page ?? 1;

            var skip = (page - 1) * pageSize;

            IQueryable<Bericht> query = _context.Berichten
                .Where(x => x.PublicatieDatum <= DateTimeOffset.UtcNow)
                .Where(x => !x.PublicatieEinddatum.HasValue || x.PublicatieEinddatum >= DateTimeOffset.UtcNow);

            if (!string.IsNullOrWhiteSpace(filterModel?.Type))
            {
                query = query.Where(x => x.Type == filterModel.Type);
            }

            if (filterModel?.SkillIds != null && filterModel.SkillIds.Any())
            {
                query = query.Where(x => x.Skills.Any(s => filterModel.SkillIds.Contains(s.Id)));
            }

            if (!string.IsNullOrWhiteSpace(filterModel?.Search))
            {
                query = query.Where(x => x.Titel.Contains(filterModel.Search) || x.Inhoud.Contains(filterModel.Search));
            }

            var total = await query.CountAsync(token);

            Response.Headers["X-Current-Page"] = page.ToString();
            Response.Headers["X-Total-Records"] = total.ToString();
            Response.Headers["X-Page-Size"] = total.ToString();
            Response.Headers["X-Total-Pages"] = Math.Ceiling((double)total / pageSize).ToString();

            var userId = User.GetId();

            var result = query
                .Select(x=> new { Bericht = x, Datum = x.DateUpdated > x.PublicatieDatum ? x.DateUpdated.Value : x.PublicatieDatum })
                .OrderByDescending(x => x.Datum)
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new SearchBerichtenResponseModel(
                    x.Bericht.Id,
                    "/api/berichten/" + x.Bericht.Id.ToString(),
                    x.Bericht.Inhoud,
                    x.Bericht.IsBelangrijk,
                    x.Bericht.Gelezen.Any(g => g.GelezenOp >= x.Datum && g.UserId == userId),
                    x.Datum,
                    x.Bericht.Titel,
                    x.Bericht.Type,
                    x.Bericht.Skills
                        .Where(y => !y.IsDeleted)
                        .Select(y => new SearchBerichtenSkillModel(y.Id, y.Naam))
                        .ToList()
                ))
                .AsAsyncEnumerable();

            return Ok(result);
        }
    }

    public record BerichtFilterModel(string? Type, string? Search, int[]? SkillIds, int? Page, int? PageSize);

    internal class SearchBerichtenResponseModel
    {
        public int Id { get; set; }
        public string Inhoud { get; }
        public bool IsBelangrijk { get; }
        public bool IsGelezen { get; }
        public DateTimeOffset Datum { get; }
        public string Url { get; }
        public string Titel { get; }
        public string Type { get; }
        public List<SearchBerichtenSkillModel> Skills { get; }

        public SearchBerichtenResponseModel(int id, string url, string inhoud, bool isBelangrijk, bool isGelezen, DateTimeOffset datum, string titel, string type, List<SearchBerichtenSkillModel> skills)
        {
            Id = id;
            Url = url;
            Inhoud = inhoud;
            IsBelangrijk = isBelangrijk;
            IsGelezen = isGelezen;
            Datum = datum;
            Titel = titel;
            Type = type;
            Skills = skills;
        }
    }

    internal class SearchBerichtenSkillModel
    {
        public int Id { get; }
        public string Naam { get; }

        public SearchBerichtenSkillModel(int id, string naam)
        {
            Id = id;
            Naam = naam;
        }
    }
}