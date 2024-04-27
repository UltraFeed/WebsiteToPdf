#pragma warning disable CA1305
#pragma warning disable CA2000

using System.Net.NetworkInformation;
using System.Net;
using System.Text;
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
			_ = merger.Merge(inputDoc, 1, inputDoc.GetNumberOfPages());
		}

		merger.Close();
	}

	internal static async Task<byte []> TakeScreenshot (string website, string pdfPathScreen)
	{
		using (BrowserFetcher browserFetcher = new())
		{
			_ = await browserFetcher.DownloadAsync().ConfigureAwait(false);
		}

		using IBrowser browser = await Puppeteer.LaunchAsync(new LaunchOptions
		{
			Headless = true,
			DefaultViewport = null,

		}).ConfigureAwait(false);

		using IPage page = await browser.NewPageAsync().ConfigureAwait(false);

		_ = await page.GoToAsync(website).ConfigureAwait(false);
		_ = await page.EvaluateExpressionHandleAsync("document.fonts.ready").ConfigureAwait(false);

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

	internal static string TraceRoute (string destination)
	{
		IPAddress ipAddress = Dns.GetHostAddresses(destination) [0];
		Ping ping = new();
		StringBuilder result = new();

		_ = result.AppendLine("\n---------------------------------------------------------------------");
		_ = result.AppendLine($"Traceroute to {destination}\n");

		for (int ttl = 1; ttl <= 30; ttl++)
		{
			PingOptions options = new(ttl, true);
			PingReply reply = ping.Send(ipAddress, 1000, new byte [32], options);

			if (reply.Status is IPStatus.TtlExpired or IPStatus.Success)
			{
				_ = result.AppendLine($"{ttl}: {reply.Address} ({reply.RoundtripTime} ms)");

				if (reply.Status == IPStatus.Success)
				{
					_ = result.AppendLine("Reached destination");
					break;
				}
			}
			else
			{
				_ = result.AppendLine($"{ttl}: *"); // Выводим "*", если не получили ответа
			}
		}

		_ = result.AppendLine("\n---------------------------------------------------------------------");

		return result.ToString();
	}
}
