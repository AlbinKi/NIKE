﻿using Api.Model;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Api.Repository
{
    public class EntryRepository : IEntryRepository
    {
        private readonly NIKEContext _context;
        
        public EntryRepository(NIKEContext context)
        {
            _context = context;
        }

     

        public async Task<Entry> Get(long poiID, long userID)
        {
            return await _context.Entries.AsNoTracking().FirstOrDefaultAsync(x => x.POIID == poiID && x.UserId == userID);

        }

        public async Task<Entry> GetWithTracking(long entryID)
        {
            return await _context.Entries.FirstOrDefaultAsync(x => x.Id == entryID);
        }

        public async Task<Entry> Set(Entry entry)
        {

            await _context.AddAsync(entry);

            await _context.SaveChangesAsync();

            return entry;   
        }

        public async Task UpdateEntry()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEntry(Entry entryRemove)
        {
            _context.Remove(entryRemove);
            await _context.SaveChangesAsync();
        }
    }
}
