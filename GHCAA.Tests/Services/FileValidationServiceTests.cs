using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Services;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class FileValidationServiceTests
    {
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;
        private const string JpegContentType = "image/jpeg";
        private const string PngContentType = "image/png";
        private const string WebpContentType = "image/webp";
        private const string PdfContentType = "application/pdf";
        private const string SpreadsheetContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string UnsupportedContentType = "text/html";
        private const string JpegFileName = "image.jpg";
        private const string PngFileName = "image.png";
        private const string WebpFileName = "image.webp";
        private const string PdfFileName = "document.pdf";
        private const string SpreadsheetFileName = "members.xlsx";
        private const string UnsupportedFileName = "unsafe.html";
        private const string UnsupportedFileTypeMessage = "Unsupported file type.";
        private const string UnsupportedFileExtensionMessage = "Unsupported file extension.";
        private const string InvalidFileContentMessage = "File content does not match its declared type.";

        private static readonly byte[] JpegHeader = [0xFF, 0xD8, 0xFF];
        private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x00, 0x00, 0x00, 0x00];
        private static readonly byte[] WebpHeader = [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50];
        private static readonly byte[] PdfHeader = [0x25, 0x50, 0x44, 0x46];
        private static readonly byte[] SpreadsheetHeader = [0x50, 0x4B, 0x03, 0x04];
        private static readonly byte[] InvalidHeader = [0x00];

        private readonly FileValidationService _service = new();

        public static IEnumerable<TestCaseData> ValidFiles()
        {
            yield return new TestCaseData(FileCategory.Image, JpegFileName, JpegContentType, JpegHeader);
            yield return new TestCaseData(FileCategory.Image, PngFileName, PngContentType, PngHeader);
            yield return new TestCaseData(FileCategory.Image, WebpFileName, WebpContentType, WebpHeader);
            yield return new TestCaseData(FileCategory.Document, PdfFileName, PdfContentType, PdfHeader);
            yield return new TestCaseData(FileCategory.Spreadsheet, SpreadsheetFileName, SpreadsheetContentType, SpreadsheetHeader);
        }

        [TestCaseSource(nameof(ValidFiles))]
        public void Validate_AcceptsEverySupportedCategoryAndSignature(
            FileCategory category,
            string fileName,
            string contentType,
            byte[] header)
        {
            using var content = new MemoryStream(header);

            var result = _service.Validate(content, fileName, contentType, header.Length, category, MaxFileSizeBytes);

            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }

        [TestCase(0)]
        [TestCase(MaxFileSizeBytes + 1)]
        public void Validate_RejectsEmptyOrOversizedFiles(long length)
        {
            using var content = new MemoryStream(JpegHeader);

            var result = _service.Validate(content, JpegFileName, JpegContentType, length, FileCategory.Image, MaxFileSizeBytes);

            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }

        [TestCase(null)]
        [TestCase(UnsupportedContentType)]
        public void Validate_RejectsMissingOrUnsupportedContentTypes(string? contentType)
        {
            using var content = new MemoryStream(JpegHeader);

            var result = _service.Validate(content, JpegFileName, contentType, JpegHeader.Length, FileCategory.Image, MaxFileSizeBytes);

            result.Should().BeEquivalentTo(FileValidationResult.Fail(UnsupportedFileTypeMessage));
        }

        [TestCase("")]
        [TestCase(UnsupportedFileName)]
        public void Validate_RejectsMissingOrUnsupportedExtensions(string fileName)
        {
            using var content = new MemoryStream(JpegHeader);

            var result = _service.Validate(content, fileName, JpegContentType, JpegHeader.Length, FileCategory.Image, MaxFileSizeBytes);

            result.Should().BeEquivalentTo(FileValidationResult.Fail(UnsupportedFileExtensionMessage));
        }

        [TestCase(FileCategory.Image, JpegFileName, JpegContentType)]
        [TestCase(FileCategory.Document, PdfFileName, PdfContentType)]
        [TestCase(FileCategory.Spreadsheet, SpreadsheetFileName, SpreadsheetContentType)]
        public void Validate_RejectsContentThatDoesNotMatchTheDeclaredType(
            FileCategory category,
            string fileName,
            string contentType)
        {
            using var content = new MemoryStream(InvalidHeader);

            var result = _service.Validate(content, fileName, contentType, InvalidHeader.Length, category, MaxFileSizeBytes);

            result.Should().BeEquivalentTo(FileValidationResult.Fail(InvalidFileContentMessage));
        }
    }
}
