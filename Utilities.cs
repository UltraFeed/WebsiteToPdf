#pragma warning disable CA1305
#pragma warning disable CA2000
#pragma warning disable CS8604

using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Layout;
using iText.Layout.Element;
using PuppeteerSharp;

namespace WebsiteToPdf;
internal static class Utilities
{
	internal static async Task CreatePdfFiles (Uri website, string pdfPathText, string pdfPathScreen)
	{
		byte [] imageBytes = await TakeScreenshot(website, pdfPathScreen).ConfigureAwait(false);
		Image img = new(ImageDataFactory.Create(imageBytes));

		using PdfDocument pdf = new(new PdfWriter(new FileStream(pdfPathText, FileMode.Create), new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0).UseSmartMode()));
		using Document doc = new(pdf);
		string fontPath = "WebsiteToPdf.resources.FreeSans.ttf";
		PdfFont font = GetFont(fontPath);

		Paragraph osPara = CreateParagraph(GetOsInfo(), font);
		Paragraph timePara = CreateParagraph($"Время по UTC+0: {await GetNtpTimeAsync("pool.ntp.org").ConfigureAwait(false)}", font);
		Paragraph ipPara = CreateParagraph($"Внешний IP: {await GetExternalIpAddress().ConfigureAwait(false)}", font);
		Paragraph infoPara = CreateParagraph($"Программа сделала скриншот сайта: {website.IdnHost}", font);
		Paragraph whoisPara = CreateParagraph(await GetWhoisResponseAsync(website, "whois.iana.org").ConfigureAwait(false), font);
		Paragraph routePara = CreateParagraph(await TraceRouteAsync(website).ConfigureAwait(false), font);

		_ = doc.Add(osPara);
		_ = doc.Add(timePara);
		_ = doc.Add(ipPara);
		_ = doc.Add(infoPara);
		_ = doc.Add(whoisPara);
		_ = doc.Add(routePara);

		// Новый метод - растягиваем страницу до размеров скриншота
		pdf.SetDefaultPageSize(new(img.GetImageWidth(), img.GetImageHeight()));

		// Старый метод - весь скриншот ужимается до размеров страницы
		//img.ScaleToFit(pdf.GetDefaultPageSize().GetWidth(), pdf.GetDefaultPageSize().GetHeight());

		_ = doc.Add(img);
		doc.Close();
	}

	internal static Paragraph CreateParagraph (string message, PdfFont font)
	{
		string str = Encoding.GetEncoding("Windows-1251").GetString(Encoding.GetEncoding("Windows-1251").GetBytes(message));
		return new Paragraph(str).SetFont(font);
	}

	internal static PdfFont GetFont (string fontPath)
	{
		using Stream? fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fontPath);
		byte [] fontBytes = new BinaryReader(fontStream).ReadBytes((int) fontStream.Length);
		return PdfFontFactory.CreateFont(fontBytes, "Cp1251", PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
	}

	internal static async Task<string> GetExternalIpAddress ()
	{
		Uri IpApiUri = new("https://api.ipify.org?format=text");
		using HttpClient httpClient = new();
		return await httpClient.GetStringAsync(IpApiUri).ConfigureAwait(false);
	}

	internal static void RemoveFiles (List<string> filesPaths)
	{
		foreach (string filePath in filesPaths)
		{
			File.Delete(filePath);
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

	internal static async Task<byte []> TakeScreenshot (Uri website, string pdfPathScreen)
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

		_ = await page.GoToAsync(website.AbsoluteUri).ConfigureAwait(false);
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

	internal static string GetOsInfo ()
	{
		using ManagementObjectSearcher searcher = new("SELECT * FROM Win32_OperatingSystem");
		ManagementObjectCollection osCollection = searcher.Get();
		StringBuilder result = new();

		_ = result.AppendLine("---------------------------------------------------------------------");
		_ = result.AppendLine($"System Info");

		foreach (ManagementObject os in osCollection.Cast<ManagementObject>())
		{
			PropertyDataCollection properties = os.Properties;
			foreach (PropertyData property in properties)
			{
				_ = result.Append($"{property.Name}: {property.Value}\n");
			}
		}

		_ = result.AppendLine("---------------------------------------------------------------------");

		return result.ToString();
	}

	internal static async Task<string> TraceRouteAsync (Uri website)
	{
		StringBuilder result = new();
		IPAddress ipAddress = (await Dns.GetHostAddressesAsync(website.Host).ConfigureAwait(false)) [0];
		Ping ping = new();

		_ = result.AppendLine("\n---------------------------------------------------------------------");
		_ = result.AppendLine($"Traceroute to {website.IdnHost}\n");

		for (int ttl = 1; ttl <= 30; ttl++)
		{
			PingOptions options = new(ttl, true);
			PingReply reply = await ping.SendPingAsync(ipAddress, 1000, new byte [32], options).ConfigureAwait(false);

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

		_ = result.AppendLine("---------------------------------------------------------------------");

		return result.ToString();
	}

	internal static async Task<string> GetWhoisResponseAsync (Uri website, string whoisServer)
	{
		StringBuilder result = new();
		website = new Uri(website.GetLeftPart(UriPartial.Authority));
		string domain = website.IdnHost;

		_ = result.AppendLine("\n---------------------------------------------------------------------");
		_ = result.AppendLine($"Whois to {website.IdnHost}\n");

		using TcpClient tcpClient = new();
		await tcpClient.ConnectAsync(whoisServer, 43).ConfigureAwait(false);

		using NetworkStream stream = tcpClient.GetStream();
		byte [] requestBytes = Encoding.ASCII.GetBytes(domain + "\r\n");
		await stream.WriteAsync(requestBytes).ConfigureAwait(false);

		byte [] responseBytes = new byte [1024];
		int bytesRead = await stream.ReadAsync(responseBytes).ConfigureAwait(false);

		_ = result.AppendLine(Encoding.ASCII.GetString(responseBytes, 0, bytesRead));

		_ = result.AppendLine("---------------------------------------------------------------------");

		return result.ToString();
	}

	internal static async Task<string> GetNtpTimeAsync (string ntpServer)
	{
		byte [] ntpData = new byte [48];
		ntpData [0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

		IPAddress [] addresses = await Dns.GetHostEntryAsync(ntpServer).ContinueWith(t => t.Result.AddressList, TaskScheduler.Default).ConfigureAwait(false);
		IPEndPoint ipEndPoint = new(addresses [0], 123);
		using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

		await socket.ConnectAsync(ipEndPoint).ConfigureAwait(false);
		_ = await socket.SendAsync(new ArraySegment<byte>(ntpData), SocketFlags.None).ConfigureAwait(false);
		_ = await socket.ReceiveAsync(new ArraySegment<byte>(ntpData), SocketFlags.None).ConfigureAwait(false);

		ulong intPart = ((ulong) ntpData [40] << 24) | ((ulong) ntpData [41] << 16) | ((ulong) ntpData [42] << 8) | ntpData [43];
		ulong fractPart = ((ulong) ntpData [44] << 24) | ((ulong) ntpData [45] << 16) | ((ulong) ntpData [46] << 8) | ntpData [47];

		ulong milliseconds = (intPart * 1000) + (fractPart * 1000 / 0x100000000L);
		DateTime networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long) milliseconds);
		return $"{networkDateTime:yyyy-MM-dd HH:mm:ss.fff}";
	}
}
