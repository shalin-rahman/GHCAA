using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GHCAA.Infrastructure.Services
{
    public class IDCardService : IIDCardService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public IDCardService(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
            // Set QuestPDF license (Community is free for individual developers/small organizations)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private string GetQrDataUri(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);
            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeAsPngByteArr)}";
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

            var verifyUrl = $"{_config["GeneralSettings:PortalBaseUrl"]}/verify/{member.MembershipNumber ?? member.Id.ToString()}";
            var qrBase64 = GetQrDataUri(verifyUrl).Replace("data:image/png;base64,", "");

            var photoElement = !string.IsNullOrEmpty(photoBase64)
                ? $"<image href='data:image/jpeg;base64,{photoBase64}' x='230' y='50' width='90' height='90' clip-path='inset(0% round 10px)'/>"
                : "<rect x='230' y='50' width='90' height='90' fill='#bdc3c7' rx='10'/>";

            var svg = $@"<svg width='350' height='200' viewBox='0 0 350 200' xmlns='http://www.w3.org/2000/svg'>
                <defs>
                    <linearGradient id='cardGrad' x1='0%' y1='0%' x2='100%' y2='100%'>
                        <stop offset='0%' style='stop-color:#111;stop-opacity:1' />
                        <stop offset='100%' style='stop-color:#c5a059;stop-opacity:1' />
                    </linearGradient>
                </defs>
                <rect width='100%' height='100%' fill='url(#cardGrad)' rx='15'/>
                <rect x='10' y='10' width='330' height='180' fill='#ffffff11' rx='10' stroke='#ffffff22'/>
                
                <text x='25' y='40' font-family='sans-serif' font-size='14' font-weight='bold' fill='#c5a059' style='text-transform:uppercase; letter-spacing:1px'>GHC Alumni Association</text>
                
                <text x='25' y='85' font-family='sans-serif' font-size='16' font-weight='900' fill='white'>{member.FullName}</text>
                <text x='25' y='110' font-family='sans-serif' font-size='10' fill='#bdc3c7' font-weight='bold'>M-ID: {member.MembershipNumber ?? "PENDING"}</text>
                <text x='25' y='125' font-family='sans-serif' font-size='10' fill='#bdc3c7'>{member.MembershipType} Member</text>
                
                <image href='data:image/png;base64,{qrBase64}' x='25' y='145' width='35' height='35' />
                <text x='65' y='160' font-family='sans-serif' font-size='8' fill='#bdc3c7'>Scan to verify dossier</text>

                {photoElement}
            </svg>";

            var bytes = System.Text.Encoding.UTF8.GetBytes(svg);
            return $"data:image/svg+xml;base64,{Convert.ToBase64String(bytes)}";
        }

        public async Task<string> GenerateCertificateDataUriAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) throw new KeyNotFoundException("Member not found");

            var verifyUrl = $"{_config["GeneralSettings:PortalBaseUrl"]}/verify/{member.MembershipNumber ?? member.Id.ToString()}";
            var qrBase64 = GetQrDataUri(verifyUrl).Replace("data:image/png;base64,", "");

            var svg = $@"<svg width='800' height='550' viewBox='0 0 800 550' xmlns='http://www.w3.org/2000/svg'>
                <rect width='100%' height='100%' fill='#fffaf0'/>
                <rect x='20' y='20' width='760' height='510' fill='none' stroke='#c5a059' stroke-width='15' rx='10'/>
                
                <text x='400' y='120' font-family='serif' font-size='42' font-weight='bold' text-anchor='middle' fill='#111'>CERTIFICATE OF MEMBERSHIP</text>
                <text x='400' y='170' font-family='sans-serif' font-size='18' text-anchor='middle' fill='#666'>This institutional record certifies that</text>
                
                <text x='400' y='240' font-family='sans-serif' font-size='36' font-weight='900' text-anchor='middle' fill='#c5a059'>{member.FullName}</text>
                
                <text x='400' y='300' font-family='sans-serif' font-size='18' text-anchor='middle' fill='#666'>is a lifetime recognized member of the</text>
                <text x='400' y='340' font-family='sans-serif' font-size='24' font-weight='bold' text-anchor='middle' fill='#111'>Govt. Haraganga College Alumni Association</text>
                
                <text x='400' y='390' font-family='sans-serif' font-size='14' text-anchor='middle' fill='#999'>Registry ID: {member.MembershipNumber ?? "N/A"}</text>
                <text x='400' y='415' font-family='sans-serif' font-size='12' text-anchor='middle' fill='#999'>Generated on {DateTime.Now:dd MMM yyyy}</text>

                <image href='data:image/png;base64,{qrBase64}' x='375' y='440' width='50' height='50' />
                <text x='400' y='505' font-family='sans-serif' font-size='8' text-anchor='middle' fill='#999'>Electronic Dossier Verification QR</text>
            </svg>";

            var bytes = System.Text.Encoding.UTF8.GetBytes(svg);
            return $"data:image/svg+xml;base64,{Convert.ToBase64String(bytes)}";
        }

        public async Task<byte[]> GenerateIDCardPdfAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) throw new KeyNotFoundException();

            var verifyUrl = $"{_config["GeneralSettings:PortalBaseUrl"]}/verify/{member.MembershipNumber ?? member.Id.ToString()}";
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(250, 150);
                    page.Margin(0);
                    page.Background("#111");

                    page.Content().Padding(10).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item().Text("GHC ALUMNI ASSOCIATION").FontSize(10).Bold().FontColor("#c5a059");
                                inner.Item().PaddingTop(15).Text(member.FullName).FontSize(14).ExtraBold().FontColor(Colors.White);
                                inner.Item().Text($"M-ID: {member.MembershipNumber ?? "PENDING"}").FontSize(8).FontColor(Colors.Grey.Lighten1);
                                inner.Item().Text($"{member.MembershipType} Member").FontSize(8).FontColor(Colors.Grey.Lighten1);
                            });

                            if (!string.IsNullOrEmpty(member.PhotoPath))
                            {
                                var fullPath = Path.Combine("wwwroot", member.PhotoPath.TrimStart('/', '\\'));
                                if (File.Exists(fullPath))
                                {
                                    row.ConstantItem(60).Height(60).Image(fullPath).FitArea();
                                }
                            }
                        });

                        col.Item().AlignBottom().Row(row =>
                        {
                            using var qrGenerator = new QRCodeGenerator();
                            using var qrCodeData = qrGenerator.CreateQrCode(verifyUrl, QRCodeGenerator.ECCLevel.Q);
                            using var qrCode = new PngByteQRCode(qrCodeData);
                            byte[] qrBytes = qrCode.GetGraphic(20);
                            
                            row.ConstantItem(30).Height(30).Image(qrBytes);
                            row.RelativeItem().PaddingLeft(5).AlignMiddle().Text("Scan to verify alumni status").FontSize(6).FontColor(Colors.Grey.Lighten2);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateCertificatePdfAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) throw new KeyNotFoundException();

            var verifyUrl = $"{_config["GeneralSettings:PortalBaseUrl"]}/verify/{member.MembershipNumber ?? member.Id.ToString()}";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(40);
                    page.Background("#fffaf0");

                    page.Content().Border(5).BorderColor("#c5a059").Padding(50).Column(col =>
                    {
                        col.Item().AlignCenter().Text("CERTIFICATE OF MEMBERSHIP").FontSize(40).ExtraBold().FontColor("#111");
                        col.Item().PaddingTop(20).AlignCenter().Text("This institutional record certifies that").FontSize(18).Italic().FontColor(Colors.Grey.Darken1);
                        
                        col.Item().PaddingTop(30).AlignCenter().Text(member.FullName).FontSize(48).Black().FontColor("#c5a059");
                        
                        col.Item().PaddingTop(30).AlignCenter().Text("is a lifetime recognized member of the").FontSize(18).FontColor(Colors.Grey.Darken1);
                        col.Item().AlignCenter().Text("Govt. Haraganga College Alumni Association").FontSize(24).Bold().FontColor("#111");
                        
                        col.Item().PaddingTop(50).Row(row =>
                        {
                            row.RelativeItem().Column(c => {
                                c.Item().PaddingTop(20).BorderTop(1).AlignCenter().Text("President").FontSize(12).FontColor(Colors.Grey.Medium);
                            });
                            row.ConstantItem(100);
                            row.RelativeItem().Column(c => {
                                c.Item().PaddingTop(20).BorderTop(1).AlignCenter().Text("General Secretary").FontSize(12).FontColor(Colors.Grey.Medium);
                            });
                        });

                        col.Item().AlignBottom().AlignCenter().Column(c => {
                            using var qrGenerator = new QRCodeGenerator();
                            using var qrCodeData = qrGenerator.CreateQrCode(verifyUrl, QRCodeGenerator.ECCLevel.Q);
                            using var qrCode = new PngByteQRCode(qrCodeData);
                            byte[] qrBytes = qrCode.GetGraphic(20);

                            c.Item().Width(60).Height(60).Image(qrBytes);
                            c.Item().PaddingTop(5).Text($"Registry No: {member.MembershipNumber ?? "PENDING"}").FontSize(10).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"Issued on {DateTime.Now:dd MMMM yyyy}").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
