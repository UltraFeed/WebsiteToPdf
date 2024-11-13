#pragma warning disable CA1305
#pragma warning disable CA2000
#pragma warning disable CS8604

using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Utils;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
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

		Paragraph p1 = CreateParagraph(GetOsInfo(), font);
		Paragraph p2 = CreateParagraph($"Текущее UTC время: {await GetNtpTimeAsync("pool.ntp.org").ConfigureAwait(false)}", font);
		Paragraph p3 = CreateParagraph($"IP: {await GetExternalIpAddress().ConfigureAwait(false)}", font);
		Paragraph p4 = CreateParagraph($"Скриншот сайта: {website.IdnHost}", font);
		Paragraph p5 = CreateParagraph(await GetWhoisResponseAsync(website, "whois.iana.org").ConfigureAwait(false), font);
		Paragraph p6 = CreateParagraph(await TraceRouteAsync(website).ConfigureAwait(false), font);
		Paragraph p7 = CreateParagraph($@"ПРОТОКОЛ №1 от {DateTime.UtcNow:HH:mm:ss dd-MM-yyyy} UTC автоматизированного осмотра информации в сети Интернет", font).SetBold().SetTextAlignment(TextAlignment.CENTER).SetMarginTop(36).SetMarginLeft(72).SetMarginRight(72);
		Paragraph p8 = CreateParagraph("Автоматизированной системой была произведена фиксация следующей информации в сети Интернет:", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT);
		Paragraph p9 = CreateParagraph($@"Страница в сети интернет расположенная по адресу: {website}", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT);
		Paragraph p10 = CreateParagraph($@"Сведения о лице, инициировавшем осмотр: IP адрес {await GetExternalIpAddress().ConfigureAwait(false)}", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT);
		Paragraph p11 = CreateParagraph($@"Задачи осмотра:", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetBold();
		Paragraph p12 = CreateParagraph($@"- Зафиксировать информацию, размещенную по адресу: {website}", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p13 = CreateParagraph($@"Оборудование и используемое программное обеспечение:", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetBold();
		Paragraph p14 = CreateParagraph($@"- Программный комплекс по фиксации информации в сети Интернет", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p16 = CreateParagraph($@"- Локальный сервер под управлением Windows;", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p17 = CreateParagraph($@"Методика проверки корректности осмотра:", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetBold();
		Paragraph p18 = CreateParagraph($@"- В запросе пользователя был приведен Интернет-адрес (адреса) в общепринятой нотации, также известной как «URL» (universal resource locator – универсальный указатель ресурса);", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p19 = CreateParagraph($@"Адрес состоит из указателя на используемый протокол (http: или https:), разделителя (//) и доменного имени. Веб-страница или файл, имеющие такой адрес (URL), должны быть доступны для любого пользователя, имеющего доступ к сети Интернет, по его запросу через клиент (браузер), поддерживающий протокол HTTP за исключением случаев необходимости ввода пароля для получения доступа к соответствующим страницам.", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT);
		Paragraph p20 = CreateParagraph($@"Для достижения полной уверенности в корректности результата осмотра необходимо соблюдение нескольких дополнительных условий. Необходимо удостовериться, что:", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetBold();
		Paragraph p21 = CreateParagraph($@"- корректно работает служба DNS (domain name system - англ. «система доменных имен»);", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p22 = CreateParagraph($@"- компьютер (сервер), при помощи которого производится осмотр, всё время имеет связь с сетью Интернет, передаваемая и получаемая информация не искажается и не подменяется намеренно кем-либо;", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p23 = CreateParagraph($@"- информация получается непосредственно из сети Интернет, а не из кэша (временного буферного хранилища), что могло бы привести к неактуальности полученной информации;", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p24 = CreateParagraph($@"- вся отображаемая информация возвращается именно осматриваемым сайтом, а не каким-либо другим.", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p25 = CreateParagraph($@"Вышеперечисленные условия корректности перед началом осмотра сайта были автоматически проверены Системой следующим образом:", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetBold();
		Paragraph p26 = CreateParagraph($@"- был произведен запрос системы DNS без кэширования результата;", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p27 = CreateParagraph($@"- было использовано оборудование, программное обеспечение и линии связи, которые управляются независимыми, незаинтересованными субъектами и о которых не могла заранее знать сторона, заинтересованная в исходе фиксирования информации;", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p28 = CreateParagraph($@"- специалистами технической поддержки в ходе планового технического обследования Системы выявлены и отключены кэширующие устройства (программы), которые могут привести к тому, что вместо актуальной версии страницы будет зафиксирована более ранняя, сохранённая в кэше (временном буферном хранилище) версия этой страницы;", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p29 = CreateParagraph($@"- Системой установлено, что все условия корректности при фиксации информации в сети Интернет, соблюдены. Признаков некорректности работы используемых элементов или признаков подмены данных не обнаружено.", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p30 = CreateParagraph($@"Непосредственно перед получением изображений осматриваемой страницы Системой были произведены следующие действия:", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetBold();
		Paragraph p31 = CreateParagraph($@"1. Произведена проверка того, что системное время сервера синхронизировано с точным временем по протоколу NTP.
																				NTP (англ. Network Time Protocol — протокол сетевого времени) — сетевой протокол для получения сведений о точном времени и синхронизации с ним внутренних часов компьютерных систем.", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p32 = CreateParagraph($@"2. Произведен запрос WHOIS-сервиса в отношении следующих доменных имен: {website}
																				Доменное имя (домен) - обозначение символами, предназначенное для адресации сайтов в сети Интернет в целях обеспечения доступа к информации, размещенной в сети Интернет.
																				Термин WHOIS(от англ. who is - «кто такой?») означает сетевой протокол прикладного уровня, базирующийся на протоколе ТСР, и применяемый для получения регистрационных данных о владельцах доменных имен.", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p33 = CreateParagraph($@"3. При помощи программных средств трассировки маршрутов устанавливается доступность конечного узла, на котором размещена осматриваемая информация и путь прохождения пакетов от конечного узла до технических средств, используемых для осмотра.
																				DNS(англ.Domain Name System — система доменных имён) — компьютерная распределённая система для получения информации о доменах. Чаще всего используется для получения IP-адреса по имени хоста(компьютера или устройства).", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);
		Paragraph p34 = CreateParagraph($@"4. Выполнены автоматизированные запросы в Архив Интернет(Internet Archive).
																				Отправлен автоматизированный запрос на выполнение архивной копии текущей html - версии следующих интернет-страниц: {website}
																				Архив в Интернета(англ.Internet Archive) — некоммерческая организация, cобирающая копии веб-страниц, графические материалы, видео-и аудиозаписи и программное обеспечение в сети интернет для долгосрочного архивирования собранного материала и бесплатного доступа к своим базам данных для широкой публики.", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT).SetFirstLineIndent(20);

		Paragraph p35 = CreateParagraph($@"После проведения всех вышеуказанных мероприятий, произведено формирование настоящего протокола, в ходе которого:
															Системой автоматически выполнены снимки(получены изображения) осматриваемой страницы расположенной по адресу {website}. 
															Внешний вид и содержание зафиксированных изображений страницы в сети Интернет, приведены в приложениях ниже к настоящему Протоколу.
															
															Формирование настоящего протокола окончено в {DateTime.UtcNow:HH:mm:ss dd-MM-yyyy} UTC.", font).SetMarginLeft(36).SetMarginRight(36).SetTextAlignment(TextAlignment.LEFT);

		Paragraph p36 = CreateParagraph($@"Приложение 1 к автоматизированному Сервису для получения информации о веб-ресурсе", font).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT);
		Paragraph p37 = CreateParagraph($@"Приложение 2 к автоматизированному Сервису для получения информации о веб-ресурсе", font).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT);
		Paragraph p38 = CreateParagraph($@"Приложение 3 к автоматизированному Сервису для получения информации о веб-ресурсе", font).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT);
		Paragraph p39 = CreateParagraph($@"Приложение 4 к автоматизированному Сервису для получения информации о веб-ресурсе", font).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT);

		doc.GetPdfDocument().AddEventHandler(PdfDocumentEvent.END_PAGE, new PageEventHandler());

		_ = doc.Add(p7);
		_ = doc.Add(p8);
		_ = doc.Add(p9);
		_ = doc.Add(p10);
		_ = doc.Add(p11);
		_ = doc.Add(p12);
		_ = doc.Add(p13);
		_ = doc.Add(p14);
		_ = doc.Add(p16);
		_ = doc.Add(p17);
		_ = doc.Add(p18);
		_ = doc.Add(p19);
		_ = doc.Add(p20);
		_ = doc.Add(p21);
		_ = doc.Add(p22);
		_ = doc.Add(p23);
		_ = doc.Add(p24);
		_ = doc.Add(p25);
		_ = doc.Add(p26);
		_ = doc.Add(p27);
		_ = doc.Add(p28);
		_ = doc.Add(p29);
		_ = doc.Add(p30);
		_ = doc.Add(p31);
		_ = doc.Add(p32);
		_ = doc.Add(p33);
		_ = doc.Add(p34);
		_ = doc.Add(p35);
		_ = doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
		_ = doc.Add(p36);
		_ = doc.Add(p1);
		_ = doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
		_ = doc.Add(p37);
		_ = doc.Add(p2);
		_ = doc.Add(p3);
		_ = doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
		_ = doc.Add(p38);
		_ = doc.Add(p5);
		_ = doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
		_ = doc.Add(p39);
		_ = doc.Add(p6);
		_ = doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
		_ = doc.Add(p4);

		// Новый метод - растягиваем страницу до размеров скриншота
		//pdf.SetDefaultPageSize(new(img.GetImageWidth(), img.GetImageHeight()));

		// Старый метод - весь скриншот ужимается до размеров страницы
		//img.ScaleToFit(pdf.GetDefaultPageSize().GetWidth(), pdf.GetDefaultPageSize().GetHeight());

		_ = doc.Add(img);
		doc.Close();
	}

	private sealed class PageEventHandler : IEventHandler
	{
		public void HandleEvent (Event @event)
		{
			PdfDocumentEvent docEvent = (PdfDocumentEvent) @event;
			PdfDocument pdfDoc = docEvent.GetDocument();
			PdfPage page = docEvent.GetPage();
			int pageNumber = pdfDoc.GetPageNumber(page);
			PdfCanvas canvas = new(page);
			_ = canvas.BeginText().SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN), 12)
				.MoveText(page.GetPageSize().GetWidth() / 2, 20)
				.ShowText("Страница " + pageNumber)
				.EndText()
				.Stroke();
		}
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

		BrowserFetcher browserFetcher = new();
		_ = await browserFetcher.DownloadAsync().ConfigureAwait(false);

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

		int widthInPixels = 794;
		int heightInPixels = 1050;

		// Преобразование веб-страницы в PDF
		int startPageNumber = 5; // Начальное значение номера страницы

		PdfOptions pdfOptions = new()
		{
			MarginOptions = new PuppeteerSharp.Media.MarginOptions() { Bottom = "36px", Left = "72px", Right = "72px", Top = "36px" },
			DisplayHeaderFooter = true,
			Landscape = false,
			Outline = true,
			PrintBackground = true,
			OmitBackground = false,
			HeaderTemplate = $@"
							<style>
								.header {{
									font-family: Times New Roman;
									font-size: 12px;
									color: #333;
									width: 100%;
									text-align: right;
									margin-right: 36px;
								}}
							</style>
							<script>
								var startPageNumber = {startPageNumber}; // Начальное значение номера страницы
								function updatePageNumber() {{
									var pageNumberElement = document.querySelector('.pageNumber');
									if (pageNumberElement) {{
										pageNumberElement.textContent = startPageNumber + window.pageYOffset / window.innerHeight;
									}}
								}}
								window.addEventListener('DOMContentLoaded', function () {{
									updatePageNumber();
									window.addEventListener('scroll', updatePageNumber);
								}});

								// Добавляем обработчик события для изменения номера страницы при переходе на новую страницу
								window.addEventListener('hashchange', function() {{
									startPageNumber++; // Увеличиваем значение номера страницы на каждой новой странице
									updatePageNumber(); // Обновляем номер страницы
								}});
							</script>
							<div class='header'>
								Приложение <span class='pageNumber'>{startPageNumber}</span> к автоматизированному Сервису для получения информации о веб-ресурсе
							</div>",
			FooterTemplate = @"
							<div style='font-family: Times New Roman; font-size: 12px; margin-left: 10px; text-align: center;'>
								Страница <span class='pageNumber'></span> из <span class='totalPages'></span> приложений с содержимым веб-страницы
							</div>",
			Width = widthInPixels,
			Height = heightInPixels
		};

		await page.PdfAsync(pdfPathScreen, pdfOptions).ConfigureAwait(false);

		// Увеличиваем переменную pageNumber на 10 для следующей страницы

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
