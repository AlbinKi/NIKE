﻿using Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Repository
{
    public interface IPOIRepository
    {
        Task<POI> Get(double Longitude, double Latitude);
        Task<POI> Set(POIDto pOIDto);
    }
}
