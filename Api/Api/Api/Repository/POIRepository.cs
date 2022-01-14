﻿using Api.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Api.Repository
{
    public class POIRepository : IPOIRepository
    {
        private readonly NIKEContext _context;
        public POIRepository(NIKEContext context)
        {
            _context = context;
        }

        public async Task<POI> Get(double Longitude, double Latitude)
        {
            var thingToLookat = await _context.POI.AsNoTracking().Include(x => x.City).ThenInclude(c => c.Country).FirstOrDefaultAsync(POI => POI.Longitude == Longitude && POI.Latitude == Latitude);
            return thingToLookat; 
        }

        public async Task<POI> Set(POIDto pOIDto)
        {
            var city = await _context.Cities.FirstOrDefaultAsync(c => c.Name == pOIDto.City);

            if (city == null)
            {
                var country = await _context.Countries.FirstOrDefaultAsync(co => co.Name == pOIDto.Country);
                if (country == null)
                {
                    country = new Country { Name = pOIDto.Country };
                    await _context.Countries.AddAsync(country);
                    await _context.SaveChangesAsync();
                }
                city = new City { CountryId = country.Id, Name = pOIDto.City };
                await _context.Cities.AddAsync(city);
                await _context.SaveChangesAsync();
            }
            var poi = new POI { Name = pOIDto.Name, Longitude = pOIDto.Longitude, Latitude = pOIDto.Latitude, CityID = city.Id };

            await _context.AddAsync(poi);

            await _context.SaveChangesAsync();

            return poi; 
        }
    }
}
