#pragma warning disable CA2000
#pragma warning disable IDE0058

using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using PuppeteerSharp;

namespace WebsiteToPdf;
internal static class Utilities
{
	internal static void RemoveFiles (string [] files)
	{
		foreach (string file in files)
		{
			File.Delete(file);
		}
	}

	internal static void MergePdfFiles (string [] inputPdfPaths, string outputPdfPath)
	{
		using FileStream outputPdfStream = new(outputPdfPath, FileMode.Create);
		PdfWriter writer = new(outputPdfStream, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0).UseSmartMode());
		PdfMerger merger = new(new PdfDocument(writer));

		foreach (string inputPdfPath in inputPdfPaths)
		{
			using PdfReader reader = new(inputPdfPath);
			using PdfDocument inputDoc = new(reader);
			merger.Merge(inputDoc, 1, inputDoc.GetNumberOfPages());
		}

		merger.Close();
	}

	internal static async Task<byte []> TakeScreenshot (string url, string pdfPathScreen)
	{
		using (BrowserFetcher browserFetcher = new())
		{
			await browserFetcher.DownloadAsync().ConfigureAwait(false);
		}

		using IBrowser browser = await Puppeteer.LaunchAsync(new LaunchOptions
		{
			Headless = true,
			DefaultViewport = null,

		}).ConfigureAwait(false);

		using IPage page = await browser.NewPageAsync().ConfigureAwait(false);

		await page.GoToAsync(url).ConfigureAwait(false);
		await page.EvaluateExpressionHandleAsync("document.fonts.ready").ConfigureAwait(false);

		// Делаем настоящий скриншот
		byte [] imageBytes = await page.ScreenshotDataAsync(new ScreenshotOptions()
		{
			FullPage = true,
			CaptureBeyondViewport = true,
		}).ConfigureAwait(false);

		// Преобразование веб-страницы в pdf
		await page.PdfAsync(pdfPathScreen, new PdfOptions()
		{
			DisplayHeaderFooter = true,
			Landscape = true,
			Outline = true,
			PrintBackground = true,
			OmitBackground = false,
		}).ConfigureAwait(false);

		await browser.CloseAsync().ConfigureAwait(false);

		return imageBytes;
	}
}
