using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
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

            string photoBase64 = "";
            if (!string.IsNullOrEmpty(member.PhotoPath))
            {
                var fullPath = Path.Combine("wwwroot", member.PhotoPath.TrimStart('/', '\\'));
                if (System.IO.File.Exists(fullPath))
                {
                    var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath, cancellationToken);
                    photoBase64 = Convert.ToBase64String(fileBytes);
                }
            }

            var photoElement = !string.IsNullOrEmpty(photoBase64)
                ? $"<image href='data:image/jpeg;base64,{photoBase64}' x='230' y='50' width='100' height='100' clip-path='inset(0% round 10px)'/>"
                : "<rect x='230' y='50' width='100' height='100' fill='#bdc3c7' rx='10'/>";

            var svg = $@"<svg width='350' height='200' viewBox='0 0 350 200' xmlns='http://www.w3.org/2000/svg'>
                <defs>
                    <linearGradient id='cardGrad' x1='0%' y1='0%' x2='100%' y2='100%'>
                        <stop offset='0%' style='stop-color:#1a2a6c;stop-opacity:1' />
                        <stop offset='50%' style='stop-color:#b21f1f;stop-opacity:1' />
                        <stop offset='100%' style='stop-color:#fdbb2d;stop-opacity:1' />
                    </linearGradient>
                </defs>
                <rect width='100%' height='100%' fill='url(#cardGrad)' rx='15'/>
                <rect x='10' y='10' width='330' height='180' fill='#ffffff22' rx='10' stroke='#ffffff33'/>
                
                <text x='25' y='40' font-family='Outfit, sans-serif' font-size='18' font-weight='bold' fill='white' style='text-transform:uppercase; letter-spacing:1px'>GHCAA Digital ID</text>
                
                <text x='25' y='85' font-family='Outfit, sans-serif' font-size='16' font-weight='600' fill='white'>{member.FullName}</text>
                <text x='25' y='110' font-family='Outfit, sans-serif' font-size='12' fill='#ecf0f1'>ID: {member.MembershipNumber ?? "PENDING"}</text>
                <text x='25' y='130' font-family='Outfit, sans-serif' font-size='12' fill='#ecf0f1'>Type: {member.MembershipType}</text>
                
                <rect x='25' y='150' width='80' height='25' rx='5' fill='{(member.Status == Enums.MembershipStatus.Active ? "#27ae60" : "#d35400")}'/>
                <text x='65' y='167' font-family='Outfit, sans-serif' font-size='11' font-weight='bold' fill='white' text-anchor='middle'>{member.Status}</text>

                {photoElement}
            </svg>";

            var bytes = System.Text.Encoding.UTF8.GetBytes(svg);
            return $"data:image/svg+xml;base64,{Convert.ToBase64String(bytes)}";
        }

        public async Task<string> GenerateCertificateDataUriAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) throw new KeyNotFoundException("Member not found");

            var svg = $@"<svg width='800' height='600' viewBox='0 0 800 600' xmlns='http://www.w3.org/2000/svg'>
                <defs>
                    <pattern id='borderPattern' x='0' y='0' width='40' height='40' patternUnits='userSpaceOnUse'>
                        <path d='M0 20 L20 0 L40 20 L20 40 Z' fill='none' stroke='#d4af37' stroke-width='1'/>
                    </pattern>
                </defs>
                <rect width='100%' height='100%' fill='#fffcf0'/>
                <rect x='20' y='20' width='760' height='560' fill='none' stroke='#d4af37' stroke-width='15'/>
                <rect x='45' y='45' width='710' height='510' fill='none' stroke='#2c3e50' stroke-width='2'/>
                
                <text x='400' y='140' font-family='Garamond, serif' font-size='50' font-weight='bold' text-anchor='middle' fill='#2c3e50'>CERTIFICATE OF MEMBERSHIP</text>
                <text x='400' y='200' font-family='Outfit, sans-serif' font-size='22' text-anchor='middle' fill='#7f8c8d'>This is to certify that</text>
                
                <text x='400' y='280' font-family='Outfit, sans-serif' font-size='42' font-weight='bold' text-anchor='middle' fill='#b21f1f'>{member.FullName}</text>
                
                <text x='400' y='350' font-family='Outfit, sans-serif' font-size='22' text-anchor='middle' fill='#7f8c8d'>is a duly registered and recognized member of</text>
                <text x='400' y='410' font-family='Outfit, sans-serif' font-size='28' font-weight='bold' text-anchor='middle' fill='#2c3e50'>GHC Alumni Association (GHCAA)</text>
                
                <text x='400' y='470' font-family='Outfit, sans-serif' font-size='18' text-anchor='middle' fill='#7f8c8d'>Membership No: {member.MembershipNumber ?? "N/A"}</text>
                <text x='400' y='500' font-family='Outfit, sans-serif' font-size='16' text-anchor='middle' fill='#95a5a6'>Issued on {DateTime.Now:MMMM dd, yyyy}</text>

                <line x1='100' y1='540' x2='300' y2='540' stroke='#2c3e50' stroke-width='1'/>
                <text x='200' y='560' font-family='Outfit, sans-serif' font-size='14' text-anchor='middle' fill='#7f8c8d'>President</text>

                <line x1='500' y1='540' x2='700' y2='540' stroke='#2c3e50' stroke-width='1'/>
                <text x='600' y='560' font-family='Outfit, sans-serif' font-size='14' text-anchor='middle' fill='#7f8c8d'>General Secretary</text>
                
                <circle cx='400' cy='530' r='40' fill='#d4af37' fill-opacity='0.2' stroke='#d4af37' stroke-width='2'/>
                <text x='400' y='537' font-family='Outfit, sans-serif' font-size='10' font-weight='bold' text-anchor='middle' fill='#d4af37'>OFFICIAL SEAL</text>
            </svg>";

            var bytes = System.Text.Encoding.UTF8.GetBytes(svg);
            return $"data:image/svg+xml;base64,{Convert.ToBase64String(bytes)}";
        }
    }
}
