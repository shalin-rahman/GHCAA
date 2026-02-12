using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class IDCardService : IIDCardService
    {
        private readonly ApplicationDbContext _db;

        public IDCardService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateIDCardDataUriAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) throw new KeyNotFoundException("Member not found");

            var svg = $@"<svg width='350' height='200' xmlns='http://www.w3.org/2000/svg'>
                <rect width='100%' height='100%' fill='#2c3e50' rx='10'/>
                <text x='20' y='40' font-family='Arial' font-size='20' fill='white'>GHCAA ID CARD</text>
                <text x='20' y='80' font-family='Arial' font-size='16' fill='white'>Name: {member.FullName}</text>
                <text x='20' y='110' font-family='Arial' font-size='14' fill='white'>No: {member.MembershipNumber ?? "PENDING"}</text>
                <text x='20' y='140' font-family='Arial' font-size='14' fill='white'>Status: {member.Status}</text>
                <circle cx='300' cy='100' r='40' fill='#ecf0f1'/>
            </svg>";

            var bytes = System.Text.Encoding.UTF8.GetBytes(svg);
            return $"data:image/svg+xml;base64,{Convert.ToBase64String(bytes)}";
        }

        public async Task<string> GenerateCertificateDataUriAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) throw new KeyNotFoundException("Member not found");

            var svg = $@"<svg width='800' height='600' xmlns='http://www.w3.org/2000/svg'>
                <rect width='100%' height='100%' fill='white' stroke='#2c3e50' stroke-width='20'/>
                <rect x='30' y='30' width='740' height='540' fill='none' stroke='#34495e' stroke-width='2'/>
                <text x='400' y='120' font-family='Arial' font-size='40' font-weight='bold' text-anchor='middle' fill='#2c3e50'>CERTIFICATE OF MEMBERSHIP</text>
                <text x='400' y='200' font-family='Arial' font-size='20' text-anchor='middle' fill='#7f8c8d'>This is to certify that</text>
                <text x='400' y='280' font-family='Arial' font-size='36' font-weight='bold' text-anchor='middle' fill='#2980b9'>{member.FullName}</text>
                <text x='400' y='340' font-family='Arial' font-size='20' text-anchor='middle' fill='#7f8c8d'>is a registered {member.MembershipType} member of</text>
                <text x='400' y='400' font-family='Arial' font-size='24' font-weight='bold' text-anchor='middle' fill='#2c3e50'>GHC Alumni Association (GHCAA)</text>
                <text x='400' y='460' font-family='Arial' font-size='16' text-anchor='middle' fill='#7f8c8d'>Membership Number: {member.MembershipNumber}</text>
                <line x1='250' y1='520' x2='550' y2='520' stroke='#2c3e50' stroke-width='1'/>
                <text x='400' y='545' font-family='Arial' font-size='14' text-anchor='middle' fill='#7f8c8d'>General Secretary</text>
            </svg>";

            var bytes = System.Text.Encoding.UTF8.GetBytes(svg);
            return $"data:image/svg+xml;base64,{Convert.ToBase64String(bytes)}";
        }
    }
}
